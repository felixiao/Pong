using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class GUIServer : MonoBehaviour {

	public Rect labelIPAddrRect,groupServerRect, groupClientsRect, debugInfoRect;
	public ConnectionState state;
	public List<ServerClient> clients;
	public GUIStyle guiStyle;


	public bool showDebugInfo=true;
	GUIStyle debugStyle;
	public int debugFontSize=10;
	public Color debugTextColor;


	public bool showMsg=true;
	GUIStyle msgStyle;
	public Color msgTextColor;

	NetMgr netmgr;

	public enum ConnectionState{
		Waiting,
		Connecting,
		Connected,
		Ready,
		Lost,
		Quiting,
		Quited
	}
	// Use this for initialization
	void Start () {
		debugStyle=new GUIStyle();
		debugStyle.wordWrap=true;

		msgStyle = new GUIStyle ();
		msgStyle.wordWrap = true;
		netmgr=this.GetComponent<NetMgr>();

	}
	
	// Update is called once per frame
	void Update () {
		debugStyle.normal.textColor = debugTextColor;
		debugStyle.fontSize=debugFontSize;

		msgStyle.normal.textColor = msgTextColor;
		msgStyle.fontSize=debugFontSize;

	}
	void OnGUI(){
		if (netmgr.server.status == Server.ServerStatus.Init||netmgr.server.status == Server.ServerStatus.Close||netmgr.server.status == Server.ServerStatus.Established) {
			GUI.Label (new Rect(Screen.width*labelIPAddrRect.x,Screen.height*labelIPAddrRect.y,Screen.width*labelIPAddrRect.width,
			                    Screen.width*labelIPAddrRect.height), netmgr.server.status.ToString(),guiStyle);
		} else if (netmgr.server.status == Server.ServerStatus.Receiving) {
			clients = netmgr.server.clients;
			
			GUI.Label (new Rect(Screen.width*labelIPAddrRect.x,Screen.height*labelIPAddrRect.y,Screen.width*labelIPAddrRect.width,
			                    Screen.width*labelIPAddrRect.height), netmgr.server.iepRecv.Address.ToString()+":"+netmgr.server.iepRecv.Port,guiStyle);
			
			//display local info 
			if (GUI.Button (new Rect(Screen.width*groupServerRect.x,Screen.height*groupServerRect.y,
			                         Screen.width*groupServerRect.width, Screen.height*groupServerRect.height), "Server Ready")) {
				netmgr.server.SendToAll(NetMgr.NetworkToken.Game,"Start");
			}
			
			//display clients info
			int index=0;
			foreach(ServerClient sc in clients){
				if(sc.status!=ServerClient.Status.Quited){
					GUI.Box (new Rect(Screen.width*groupClientsRect.x,Screen.height*groupClientsRect.y+Screen.height*groupClientsRect.height*index,
					                  Screen.width*groupClientsRect.width,Screen.height*groupClientsRect.height),sc.name+" "+sc.status.ToString());
					index++;
				}
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
