using UnityEngine;
using System.Collections;

public class unityNetwork : MonoBehaviour {
	public string gameNameUnique="FelixPongGameTest";
	public string gameName="Pong Game by Felix";
	public int maxPlayer=32;
	public int portNumer=11791;
	private HostData[] hostData;
	private bool refreshing=false;

	private float btnX,btnY,btnW,btnH;

	// Use this for initialization
	void Start () {
		btnX = Screen.width * 0.2f;
		btnY = Screen.height * 0.2f;
		btnW = Screen.width * 0.1f;
		btnH = Screen.height * 0.1f;
	}
	
	// Update is called once per frame
	void Update () {
		if (refreshing) {
			if(MasterServer.PollHostList().Length>0){
				refreshing=false;
				hostData=MasterServer.PollHostList();
			}
		}
	
	}

	void StartServer(){
		Network.InitializeServer (maxPlayer, portNumer, !Network.HavePublicAddress());
		MasterServer.RegisterHost (gameNameUnique, gameName, "This is a simple Pong Game made by Felix");

	}
	void RefreshHostList(){
		MasterServer.RequestHostList (gameNameUnique);
		refreshing = true;
	}
	void OnGUI(){
		if (!Network.isClient && !Network.isServer) {
				if (GUI.Button (new Rect (btnX, btnY, btnW, btnH), "Start Server")) {
						Debug.Log ("Starting server");
						StartServer ();
				}
				if (GUI.Button (new Rect (btnX, btnY * 2.2f, btnW, btnH), "Refreshing Host")) {
						Debug.Log ("Refreshing host");
						RefreshHostList ();
				}
				if (hostData!=null && hostData.Length > 0) {
						for (int i=0; i<hostData.Length; i++) {
								if (GUI.Button (new Rect (btnX * 2.5f, btnY * (1.2f + i), btnW * 3f, btnH), hostData [i].gameName)) {
										Network.Connect (hostData [i]);
								}
						}
				}
		}
	}
}
