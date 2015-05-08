using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using System.Collections.Generic;
public class NetMgr : MonoBehaviour {

	public enum ServerOrClient{
		Unknown,
		Server,
		Client
	}
	
	public ServerOrClient serverOrClient=ServerOrClient.Unknown;
	public enum NetworkToken{
		Message = 0,
		Join 	= 1,
		Accept	= 2,
		Reject	= 3,
		Quit	= 4,
		BallPos	= 5,
		Game	= 6,
		GetIP	= 7,
		Slider	= 8
	}
	public enum GameEvent{
		Start,
		Ready,
		Play,
		Pause,
		Quit
	}
	public static GameEvent gameEvent;
	public Server server;
	public Client client;

	public List<string> errorStrings=new List<string>();
	public List<string> msgStrings=new List<string>();


	Transform ball,slider,borderSide,sliderL,sliderR,borderL,borderR;
	public static float frameCount=24.0f;
	float sendFrequency,sendTimeElapsed=0f;
	Vector2 ballpos=Vector2.zero;
	float sliderLY,sliderRY;
	bool RemoteUpdateBall=false,RemoteUpdateSliderL=false,RemoteUpdateSliderR=false;
	float timeSinceLastCall = 0;
	// Use this for initialization
	void Start () {
	}
	void OnGUI(){
//		if (serverOrClient == ServerOrClient.Server) {
//			if (GUI.Button (new Rect (0.2f * Screen.width, 0.2f * Screen.width, 0.1f * Screen.width, 0.1f * Screen.width), "+")) {
//				frameCount += 1.0f;
//			}
//			GUI.Box (new Rect (0.3f * Screen.width, 0.2f * Screen.width, 0.4f * Screen.width, 0.1f * Screen.width), frameCount.ToString ());
//			if (GUI.Button (new Rect (0.7f * Screen.width, 0.2f * Screen.width, 0.1f * Screen.width, 0.1f * Screen.width), "-")) {
//				frameCount -= 1.0f;
//			}
//		}
	}
	void Awake(){
		ball = GameObject.Find ("ball").transform;

		if (serverOrClient == ServerOrClient.Server) {
				sliderL = GameObject.Find ("sliderL").transform;
				sliderR = GameObject.Find ("sliderR").transform;
				borderL = GameObject.Find ("borderL").transform;
				borderR = GameObject.Find ("borderR").transform;
		} else if (serverOrClient == ServerOrClient.Client) {
			slider = GameObject.Find ("slider").transform;
			borderSide = GameObject.Find ("borderSide").transform;
		}

		if (serverOrClient == ServerOrClient.Server)
			StartServer ();
		else if (serverOrClient == ServerOrClient.Client)
			StartClient ();
	}
	// Update is called once per frame
	void Update () {
		if (frameCount <= 0) frameCount = 1;
		sendFrequency = 1.0f / frameCount;
		timeSinceLastCall += Time.deltaTime;
		if (serverOrClient == ServerOrClient.Client) {
			Vector2 predPos = Prediction.GetQuadratic (timeSinceLastCall);
			ball.position = new Vector3 (predPos.x, predPos.y, 0);
			Debug.Log("Pred:" +ball.position.x+","+ball.position.y+" dt:"+timeSinceLastCall);
		}
		if (RemoteUpdateBall) {
			if(serverOrClient==ServerOrClient.Client){
				Prediction.Add(ballpos,timeSinceLastCall);
				timeSinceLastCall=0;
				ball.position=new Vector3(ballpos.x,ballpos.y,0);
			}
			else 
			RemoteUpdateBall=false;
		}
		if (RemoteUpdateSliderL) {
			if(serverOrClient==ServerOrClient.Server){
				sliderL.position=new Vector3(sliderL.position.x,sliderLY,sliderL.position.z);
			}
			RemoteUpdateSliderL=false;
		}
		if (RemoteUpdateSliderR) {
			if(serverOrClient==ServerOrClient.Server){
				sliderR.position=new Vector3(sliderR.position.x,sliderRY,sliderR.position.z);
			}
			RemoteUpdateSliderR=false;
		}

		if (gameEvent == GameEvent.Start) {
			if(serverOrClient==ServerOrClient.Client)	SetStage();
			if(serverOrClient==ServerOrClient.Server) UpdateServerStage();
		}
		if (gameEvent == GameEvent.Play) {
			if(serverOrClient==ServerOrClient.Server){
				SendBallPos();
				MoveBall.moveable=true;
			}else if(serverOrClient==ServerOrClient.Client){
				SendSliderPos();
			}
		}
	}
	void SetStage(){
		Camera.current.transform.position = client.cameraPos;
		//borderTop.position=new Vector3(client.cameraPos.x,borderTop.position.y,borderTop.position.z);
		//borderBottom.position=new Vector3(client.cameraPos.x,borderBottom.position.y,borderBottom.position.z);
		if (client.stageType != 0) {
			slider.GetComponent<Renderer>().enabled=true;
			borderSide.GetComponent<Renderer>().enabled=true;
			if(client.stageType==-1){
				slider.position=new Vector3(client.cameraPos.x-2.7f,0,0);
				borderSide.position=new Vector3(client.cameraPos.x-3.0f,0,0);
			}
			else if(client.stageType==1){
				slider.position=new Vector3(client.cameraPos.x+2.7f,0,0);
				borderSide.position=new Vector3(client.cameraPos.x+3.0f,0,0);
			}
		}
		gameEvent = GameEvent.Play;

	}
	void UpdateServerStage(){
		if (server.GetLiveClientsCount () > 1) {
				sliderL.GetComponent<Renderer>().enabled = true;
				sliderR.GetComponent<Renderer>().enabled = true;
				borderL.GetComponent<Renderer>().enabled = true;
				borderR.GetComponent<Renderer>().enabled = true;
				sliderL.position = new Vector3 (-2.7f, 0, 0);
				sliderR.position = new Vector3 ((server.GetLiveClientsCount () - 1) * 6.0f + 2.7f, 0, 0);
				borderL.position = new Vector3 (-3.0f, 0, 0);
				borderR.position = new Vector3 ((server.GetLiveClientsCount () - 1) * 6.0f + 3.0f, 0, 0);
				gameEvent = GameEvent.Play;
		}
	}
	//UNDONE
	void StartServer(){
		server = new Server ();
		Server.errorMsg += ErrorHandler;
		Server.netMsgEvt += ServerMessageHandler;
		server.Init ();
	}
	//Join|name,id|msg
	//BallPos|num,num
	//GetIP|
	//Message|msg
	//Game|event|msg
	//Game|Start|x,y,z|num
	//Quit|name|msg
	//Accept|id|name,id;name,id;name,id
	//Reject|msg
	void ClientMessageHandler(string msg){
		if(msg.Contains("|")){
			string[] s = msg.Split ('|');
			if (s.Length > 0) {
				switch(s[0]){
				case "Join":
					msgStrings.Add(msg);
					break;
				case "Accept":
					msgStrings.Add(msg);
					break;
				case "Reject":
					msgStrings.Add(msg);
					break;
				case "Quit":
					msgStrings.Add(msg);
					break;
				case "BallPos":
					ballpos=new Vector2(float.Parse(s[1].Split(',')[0]),float.Parse(s[1].Split(',')[1]));
					RemoteUpdateBall=true;
					break;
				case "Game":
					msgStrings.Add(msg);
					if(s[1]=="Start"){
						gameEvent=GameEvent.Start;
						client.cameraPos=new Vector3(float.Parse(s[2]),0,-10);
						client.stageType=int.Parse(s[3]);
					}
					break;
				case "Message":
					msgStrings.Add(msg);
					break;
				}
			}
		}
	}
	public void SendBallPos(){
		if (sendTimeElapsed > sendFrequency) {
			if (serverOrClient == ServerOrClient.Server)
				server.SendToAll (NetMgr.NetworkToken.BallPos, ball.position.x + "," + ball.position.y);
			sendTimeElapsed=0;
		}
		sendTimeElapsed += Time.deltaTime;
	}
	//Join|name|msg
	//BallPos|num,num|ID
	//GetIP|
	//Message|msg
	//Game|event|msg
	//Quit|name,id|msg
	//Slider|num|leftorright
	void ServerMessageHandler(string msg){
		if(msg.Contains("|")){
			string[] s = msg.Split ('|');
			if (s.Length > 0) {
				switch(s[0]){
				case "Join":
					msgStrings.Add(msg);
					break;
				case "Quit":
					msgStrings.Add(msg);
					gameEvent=GameEvent.Quit;
					break;
				case "Slider":
					if(int.Parse(s[2])==-1){
						sliderLY=float.Parse(s[1]);
						RemoteUpdateSliderL=true;
					}
					else if(int.Parse(s[2])==1){
						sliderRY=float.Parse(s[1]);
						RemoteUpdateSliderR=true;
					}
					RemoteUpdateBall=true;
					break;
				case "BallPos":
					//ballpos=new Vector2(float.Parse(s[1].Split(',')[0]),float.Parse(s[1].Split(',')[1]));
					//
					break;
				case "Game":
					break;
				case "GetIP":
					break;
				case "Message":
					msgStrings.Add(msg);
					break;
				}
			}
		}
	}
	public void SendSliderPos(){
		if (client.stageType == 0) return;
		if (sendTimeElapsed > sendFrequency) {
			if (serverOrClient == ServerOrClient.Client)
				client.SentToServer (NetMgr.NetworkToken.Slider, slider.position.y+"|"+client.stageType);
			sendTimeElapsed=0;
		}
		sendTimeElapsed += Time.deltaTime;
	}
	//UNDONE
	void ErrorHandler(string errorMsg){
		errorStrings.Add (errorMsg);
	}

	//TODO
	void StartClient(){
		client = new Client ();
		Client.errorMsg += ErrorHandler;
		Client.netMsgEvt += ClientMessageHandler;
		client.Init ();
	}
}
