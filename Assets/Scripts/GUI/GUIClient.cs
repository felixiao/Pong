using UnityEngine;
using System.Collections;

public class GUIClient : MonoBehaviour {
	public Rect labelIPAddrRect,groupServerRect, debugInfoRect;
	public ConnectionState state;
	public GUIStyle guiStyle;

	public bool showMsg=true;
	GUIStyle msgStyle;
	public Color msgTextColor;

	public enum ConnectionState{
		Waiting,
		Connecting,
		Connected,
		Ready,
		Lost,
		Quiting,
		Quited
	}
	public bool showDebugInfo=true;
	GUIStyle debugStyle;
	public int debugFontSize=20;
	public Color debugTextColor;
	Transform ball;
	NetMgr netmgr;
	// Use this for initialization
	void Start () {
		debugStyle=new GUIStyle();
		debugStyle.wordWrap=true;
		msgStyle = new GUIStyle ();
		msgStyle.wordWrap = true;
		netmgr=this.GetComponent<NetMgr>();
		ball = GameObject.Find ("ball").transform;
	}
	
	// Update is called once per frame
	void Update () {
		debugStyle.normal.textColor = debugTextColor;
		debugStyle.fontSize=debugFontSize;
		msgStyle.normal.textColor = msgTextColor;
		msgStyle.fontSize=debugFontSize;

	}
	void OnGUI(){
		if (netmgr.client.status != Client.ClientStatus.Connected) {
			GUI.Label (new Rect(Screen.width*labelIPAddrRect.x,Screen.height*labelIPAddrRect.y,Screen.width*labelIPAddrRect.width,
			                    Screen.width*labelIPAddrRect.height), netmgr.client.status.ToString(),guiStyle);

		} 
		if(netmgr.client.status == Client.ClientStatus.Connected){
			//display local info 
			GUI.Label (new Rect(Screen.width*labelIPAddrRect.x,Screen.height*labelIPAddrRect.y,Screen.width*labelIPAddrRect.width,
			                    Screen.width*labelIPAddrRect.height), netmgr.client.ID+" - "+Client.Name,guiStyle);

//			if (GUI.Button (new Rect(Screen.width*groupServerRect.x,Screen.height*groupServerRect.y,
//			                         Screen.width*groupServerRect.width, Screen.height*groupServerRect.height), "Ready")) {
//				netmgr.client.SentToServer(NetMgr.NetworkToken.Game,"Ready");
//			}
			if (GUI.Button (new Rect(Screen.width*groupServerRect.x,Screen.height*groupServerRect.y,
			                         Screen.width*groupServerRect.width, Screen.height*groupServerRect.height), "Quit "+ball.position.x.ToString("F4")+","+ball.position.y.ToString("F4"))) {
				netmgr.client.Close();
			}
			int index=0;
			foreach(OtherClient oc in netmgr.client.others.Values){
				GUI.Box (new Rect(Screen.width*groupServerRect.x,Screen.height*groupServerRect.y+Screen.height*groupServerRect.height*(index+1),
				                  Screen.width*groupServerRect.width, Screen.height*groupServerRect.height), "["+oc.id+"] "+oc.name+" Ready");
				index++;
			}
		}
		//display message content
		if (showMsg) {
			Vector2 lastRect=new Vector2(Screen.height,0);
			for(int i=0;i<netmgr.msgStrings.Count;i++){
				string msg="["+i+"] "+netmgr.msgStrings[i];
				float height=msgStyle.CalcHeight(new GUIContent(msg),Screen.width*1.0f);
				
				if(GUI.Button(new Rect(0,lastRect.x-height,Screen.width,height),msg)){
					netmgr.msgStrings.RemoveAt(i);
				}
				//GUI.Label(new Rect(0,lastRect.x-lastRect.y-height,Screen.width,height),msg,msgStyle);
				lastRect=new Vector2(lastRect.x-height,height);
			}
		}
		
		//display debug info
		if (showDebugInfo) {
			Vector2 lastRect=new Vector2(0,0);
			for(int i=0;i<netmgr.errorStrings.Count;i++){
				string msg="E["+i+"] "+netmgr.errorStrings[i];
				float height=debugStyle.CalcHeight(new GUIContent(msg),Screen.width*1.0f);
				if(GUI.Button(new Rect(0,lastRect.x+lastRect.y,Screen.width,height),msg)){
					netmgr.errorStrings.RemoveAt(i);
				}
				//GUI.Label(new Rect(0,lastRect.x+lastRect.y,Screen.width,height),msg,debugStyle);
				lastRect=new Vector2(lastRect.x+lastRect.y,height);
			}
		}
	}
}
