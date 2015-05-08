using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using System.Runtime.InteropServices;

public class Client {
	public delegate void ErrorMsgEvent(string msg);
	public static event ErrorMsgEvent errorMsg;
	
	public delegate void NetMsgEvt(string msg);
	public static event NetMsgEvt netMsgEvt;


	public int ID;
	public static string Name="";
	public static int Score = 0;
	public Vector3 cameraPos;
	public int stageType;//-1: left side, 0: middle, 1: right side
	public Dictionary<int,OtherClient> others=new Dictionary<int, OtherClient>();
	public UdpClient udpClient;
	public IPEndPoint iepSend;
	public IPEndPoint iepIncoming;
	public static IPEndPoint serverIP;

	public enum ClientStatus{
		Init,
		Established,
		Connecting,
		Connected,
		Quit
	}
	public ClientStatus status;

	public Client(){
	}

	public int Init(){
		try{
			//serverIP=new IPEndPoint(IPAddress.Parse("172.16.1.205"),11791);
			udpClient = new UdpClient ();
			status = ClientStatus.Init;
			others=new Dictionary<int, OtherClient>();
			Receive();
			status=ClientStatus.Connecting;
			SentToServer(NetMgr.NetworkToken.Join,Name);
		}catch(Exception e){
			Debug.Log(e.ToString());
			errorMsg("[Client.Init()@48] "+e.Message);
			return -1;
		}
		return 0;
	}
	public void Close(){
		status = ClientStatus.Quit;
		SentToServer (NetMgr.NetworkToken.Quit, Name+","+ID);
		udpClient.Close();
		Application.LoadLevel ("Main");
	}

	public void Receive(){
		try{
			udpClient.BeginReceive(new AsyncCallback(Receive), iepIncoming);
			status=ClientStatus.Established;
		}catch (Exception e){
			Debug.Log(e.ToString());
			errorMsg("[Client.Receive()@65] "+e.Message);
		}
	}
	//Join|name,id|addr|msg
	//BallPos|num,num
	//GetIP|
	//Message|msg
	//GameEvent|num|msg
	//Quit|name,id|msg
	//Accept|id|name,id;name,id;name,id
	//Reject|msg
	public void Receive(IAsyncResult ar){
		try{
			iepIncoming = (IPEndPoint)ar.AsyncState;
			string incomingMsg;
			if(status==ClientStatus.Quit) return;
			Byte[] receiveBytes = udpClient.EndReceive(ar, ref iepIncoming);
			if(receiveBytes.Length>0){
				try{
					incomingMsg=Encoding.Default.GetString(receiveBytes);
					if(incomingMsg.Contains("Join|")){
						string clientString=incomingMsg.Split('|')[1];
						string name=clientString.Split(',')[0];
						int id=int.Parse(clientString.Split(',')[1]);
						AddNewClient(id,name); 
					}
			      	if(incomingMsg.Contains("Accept|")){
						string[] strings=incomingMsg.Split('|');
						ID=int.Parse(strings[1]);
						if(strings.Length>2){
							string[] others=strings[2].Split(';');
							foreach(string s in others){
								string name=s.Split(',')[0];
								int id=int.Parse(s.Split(',')[1]);
								AddNewClient(id,name);
							}
						}
						status=ClientStatus.Connected;
					}
               		if(incomingMsg.Contains("Quit|")){
						string clientString=incomingMsg.Split('|')[1];
						string name=clientString.Split(',')[0];
						int id=int.Parse(clientString.Split(',')[1]);
						RemoveClient(id,name);
               		}
					if(incomingMsg.Contains("Reject|")){
						udpClient.Close();
						Application.LoadLevel ("Main");
					}
					netMsgEvt(incomingMsg);

				}catch(Exception ex){
					Debug.Log(ex.ToString());
					errorMsg("[Client.Receive(IA)@111] "+ex.Message);
				}
			}
		}catch(Exception e){
			Debug.Log(e.ToString());
			errorMsg("[Client.Receive(IA)@116] "+e.Message);
		}
		if (status == ClientStatus.Quit) return;
			udpClient.BeginReceive(new AsyncCallback(Receive), iepIncoming);
	}
	public void AddNewClient(int id,string name){
		others.Add(id,new OtherClient(name,id));
	}
	public int RemoveClient(int id,string name){
		if (others [id].name == name)
			others.Remove (id);
		return 0;
	}

	public void SentToServer(NetMgr.NetworkToken token,string msg){
		try{
			string netmsg=token.ToString()+"|"+msg;
			Debug.Log("SendToServer: "+netmsg);
			byte[] buffer = Encoding.Default.GetBytes(netmsg);
			udpClient.Send	(buffer, buffer.Length, serverIP);

		}catch(Exception e){
			Debug.Log(e.ToString ());
			errorMsg("[Client.SendToServer()@138] "+e.Message);
		}
	}
	public void Send(NetMgr.NetworkToken token,string msg,IPEndPoint toIEP){
		try{
			string netmsg=token.ToString()+"|"+msg;
			Debug.Log("SendTo "+toIEP.Address.ToString()+": "+netmsg);
			byte[] buffer = Encoding.Default.GetBytes(netmsg);
			udpClient.Send (buffer, buffer.Length, toIEP);
		}
		catch(Exception e){
			Debug.Log(e.ToString ());
			errorMsg("[Client.Send()@150] "+e.Message);
		}
	}

}

public class OtherClient{
	public string name;
	public int id;
	public OtherClient(string _name,int _id){
		name 	= _name;
		id = _id;
	}
}
