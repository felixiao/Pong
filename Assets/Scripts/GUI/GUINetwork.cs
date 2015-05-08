using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
public class GUINetwork : MonoBehaviour {

	public Vector4 btn1,btn2,tfIP,tfName,btnSearch,btnQR,groupList,btnList;
	Rect btnRect,btn2Rect,inputIPRect,inputName,btnSearchRect,groupListRect;
	public List<string> searchSuggests=new List<string>();

	public enum GUIState{
		MainMenu,
		ServerWaiting,
		Text,
		QR,
		LevelSelect,
		ServerView,
		Single,
		ClientWaiting,
		ClientView
	}
	public string inputIPAddr="";
	public GUIState guiState;
	public string Name="";
	// Use this for initialization
	void Start () {
		guiState = GUIState.MainMenu;
		btnRect = new Rect (Screen.width * btn1.x, Screen.height * btn1.y, Screen.width * btn1.z, Screen.width * btn1.w);
		btn2Rect= new Rect (Screen.width * btn2.x, Screen.height* btn2.y, Screen.width * btn2.z, Screen.width * btn2.w);
		inputIPRect=new Rect (Screen.width * tfIP.x, Screen.height * tfIP.y, Screen.width * tfIP.z, Screen.width * tfIP.w);
		btnSearchRect=new Rect (Screen.width * btnSearch.x, Screen.height * btnSearch.y, Screen.width * btnSearch.z, Screen.width * btnSearch.w);
		groupListRect=new Rect (Screen.width * groupList.x, Screen.height * groupList.y, Screen.width * groupList.z, Screen.width * groupList.w);
		inputName = new Rect (Screen.width * tfName.x, Screen.height * tfName.y, Screen.width * tfName.z, Screen.width * tfName.w);
	}
	void Update(){
		btnRect = new Rect (Screen.width * btn1.x, Screen.height * btn1.y, Screen.width * btn1.z, Screen.width * btn1.w);
		btn2Rect= new Rect (Screen.width * btn2.x, Screen.height* btn2.y, Screen.width * btn2.z, Screen.width * btn2.w);
		inputIPRect=new Rect (Screen.width * tfIP.x, Screen.height * tfIP.y, Screen.width * tfIP.z, Screen.width * tfIP.w);
		btnSearchRect=new Rect (Screen.width * btnSearch.x, Screen.height * btnSearch.y, Screen.width * btnSearch.z, Screen.width * btnSearch.w);
		groupListRect=new Rect (Screen.width * groupList.x, Screen.height * groupList.y, Screen.width * groupList.z, Screen.width * groupList.w);
		inputName = new Rect (Screen.width * tfName.x, Screen.height * tfName.y, Screen.width * tfName.z, Screen.width * tfName.w);
	}
	void OnGUI(){
		GUI.Box (new Rect (0, 0, Screen.width, 20), "w/W:" + Screen.width / 640.0f + ",h/H:" + Screen.height / 1136.0f);
		switch (guiState) {
		
		case GUIState.MainMenu:
			if (GUI.Button (btnRect,"Server")) {
				guiState = GUIState.ServerWaiting;
			}
			
			if (GUI.Button (btn2Rect, "Client")) {
				guiState = GUIState.Text;
			}
			break;
		case GUIState.ServerWaiting:
			guiState = GUIState.ServerView;
			break;
		case GUIState.Text:
			inputIPAddr=GUI.TextField(inputIPRect,inputIPAddr,22);
			Client.Name=GUI.TextField(inputName,Client.Name,22);
			if (GUI.Button (btnSearchRect,"Search")) {
				string addr=inputIPAddr.Split(':')[0];
				int port=int.Parse(inputIPAddr.Split(':')[1]);
				Client.serverIP=new IPEndPoint(IPAddress.Parse(addr),port);
				searchSuggests.Add(inputIPAddr);
				guiState = GUIState.ClientWaiting;
			}
			GUI.BeginGroup(groupListRect);

			int i=searchSuggests.Count-1;
			for(; i>=0;i--){
				if(GUI.Button(new Rect(Screen.width * btnList.x, Screen.height * (btnList.y+btnList.w*(i+0.2f)), Screen.width * btnList.z, Screen.width * btnList.w),searchSuggests[i])){
					string addr=searchSuggests[i].Split(':')[0];
					int port=int.Parse(searchSuggests[i].Split(':')[1]);
					Client.serverIP=new IPEndPoint(IPAddress.Parse(addr),port);
					guiState = GUIState.ClientWaiting;
				}
			}
			GUI.EndGroup();
			break;
		case GUIState.QR:
			
			break;
		case GUIState.LevelSelect:

			break;
		case GUIState.ServerView:
			Application.LoadLevel("ServerView");
			break;
		case GUIState.ClientWaiting:
			guiState =GUIState.ClientView;
			break;
		case GUIState.Single:
			Application.LoadLevel("Single");
			break;
		case GUIState.ClientView:
			Application.LoadLevel("ClientView");
			break;
		}

	}
}
