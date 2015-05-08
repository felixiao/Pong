using UnityEngine;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using System.Runtime.InteropServices;

public class Server {
	public delegate void ErrorMsgEvent(string msg);
	public static event ErrorMsgEvent errorMsg;

	public delegate void NetMsgEvt(string msg);
	public static event NetMsgEvt netMsgEvt;

	public enum ServerStatus
	{
		Init,
		Established,
		Receiving,
		Close
	}
	public ServerStatus status;

	public List<ServerClient> clients=new List<ServerClient>();
	public IPEndPoint iepRecv,iepIncoming;
	UdpClient receiver, sender;

	int listenPort=11791;

	//IPEndPoint localSendIEP;
	string name;

	public int maxAttempTime=5;
	int attempTime=1;
	public int Init(){
		try{
			bool start=true;
			status=ServerStatus.Init;

			while(start){
				try{
					//iepRecv =new IPEndPoint(IPAddress.Any,listenPort);
					receiver=new UdpClient(listenPort);//Listener
					start=false;
				}catch (System.Net.Sockets.SocketException e){
					Debug.Log(e.ToString());
					errorMsg("[Server.Init()@49] "+e.Message);
					if(attempTime>=maxAttempTime)
						return -1;
					listenPort++;
					attempTime++;
				}
			}
			sender = new UdpClient();
			//localSendIEP = new IPEndPoint(IPAddress.None,0);
			//AddNewClient("Felix",new IPEndPoint(IPAddress.Any,11001));
			//status=ServerStatus.Established;
			Receive();
			GetRecvIP();
		}catch(Exception ex){ 
			Debug.Log(ex.ToString());
			errorMsg("[Server.Init()@64] "+ex.Message);
			return -2;
		}
		return 0;
	}
	public void GetRecvIP(){
		try{
		byte[] bytes = Encoding.Default.GetBytes ("GetIP|" );
		sender.EnableBroadcast = true;
		sender.Send (bytes, bytes.Length,new IPEndPoint(IPAddress.Broadcast, listenPort));
		}catch(Exception e){
			Debug.Log(e.ToString());
			errorMsg("[Server.GetRecvIP()@76 ]"+e.Message);
		}
	}
	public void Close(){
		receiver.Close();
		sender.Close();
	}

	public void Receive(){
		try{
			receiver.BeginReceive(new AsyncCallback(Receive), iepIncoming);
			status=ServerStatus.Established;
		}catch (Exception e){
			Debug.Log(e.ToString());
			errorMsg("[Server.Receive()@90] "+e.Message);
		}
	}
	//UNDONE
	//Join|name|msg
	//BallPos|num,num
	//GetIP|
	//Message|msg
	//GameEvent|num|msg
	//Quit|name,id|msg
	public void Receive(IAsyncResult ar){
		try
		{
			iepIncoming = (IPEndPoint)ar.AsyncState;
			string incomingMsg;
			Byte[] receiveBytes = receiver.EndReceive(ar, ref iepIncoming);
			if(receiveBytes.Length>0){
				try{
					incomingMsg=Encoding.Default.GetString ( receiveBytes );
					if(incomingMsg.Contains("GetIP|")){
						iepRecv=iepIncoming;
						iepRecv.Port=listenPort;
						status=ServerStatus.Receiving;
					}
					if(incomingMsg.Contains("Join|")){
						AddNewClient(incomingMsg.Split('|')[1],iepIncoming);
					}
					if(incomingMsg.Contains("Quit|")){
						string quit=incomingMsg.Split('|')[1];
						string name=quit.Split(',')[0];
						int id=int.Parse(quit.Split(',')[1]);
						RemoveClient(name,id);
					}
					netMsgEvt(incomingMsg);

				}catch(Exception ex){
					Debug.Log(ex.ToString());
					errorMsg("[Server.Receive(IA)@127] "+ex.Message);
				}
			}
			receiver.BeginReceive(new AsyncCallback(Receive), iepIncoming);
		}
		catch (Exception ex)
		{
			Debug.Log(ex.ToString());
			errorMsg("[Server.Receive(IA)@137] "+ex.Message);
		}
	}

	public void RemoveClient(string name,int id){
		if (clients [id].name == name) {
			SendToAll (NetMgr.NetworkToken.Quit, name+","+id);
			clients[id].status=ServerClient.Status.Quited;
		}
		else
			errorMsg("[Server.RemoveClient()@147]"+ name +" was not found!");
	}

	public void AddNewClient(string name,IPEndPoint iep){
		try{
			foreach(ServerClient c in clients){
				if(c.iep==iep) return;
				if(c.name==name&&c.status!=ServerClient.Status.Quited){
					ServerClient sc = new ServerClient (clients.Count, name, iep);
					Debug.Log (name+" already exist.");
					sc.Send(NetMgr.NetworkToken.Reject,"Name already exist.");
					return;
				}
			}
			Debug.Log ("Add new client ["+clients.Count+"] " + name + "," + iep.Address.ToString () + ":" + iep.Port);

			SendToAll(NetMgr.NetworkToken.Join,name+","+clients.Count+"|"+iep.Address.ToString()+":"+iep.Port.ToString());
			string lists=clients.Count.ToString();
			string liststring=GetList();
			if(clients.Count>0&&liststring.Length>0)
				lists+="|"+liststring;
			ServerClient client = new ServerClient (clients.Count, name, iep);
			clients.Add (client);
			SendTo (client.id, NetMgr.NetworkToken.Accept, lists);
		}catch(Exception e){
			Debug.Log(e.ToString());
			errorMsg("[Server.AddNewClient()@164] "+e.Message);
		}
	}
	public List<ServerClient> GetLiveClients(){
		List<ServerClient> sc = new List<ServerClient> ();
		foreach (ServerClient client in clients) {
			if(client.status!=ServerClient.Status.Quited){
				sc.Add(client);
			}
		}
		return sc;
	}
	public int GetLiveClientsCount(){
		int count = 0;
		foreach (ServerClient client in clients) {
			if(client.status!=ServerClient.Status.Quited){
				count++;
			}
		}
		return count;
	}
	public int SendToAll(NetMgr.NetworkToken token,string msg){
		if (token == NetMgr.NetworkToken.Game) {
			if (msg == "Start") {
				List<ServerClient> c = GetLiveClients ();
				if(c.Count<=1){
					errorMsg("Require 2 clients.");
					return 1;
				}
				int i = 0;
				foreach (ServerClient client in c) {
					string message = "Start|" + (6.0f * i).ToString () + "|";

					if (i == 0) message += "-1";
					else if (i == c.Count - 1) message += "1";
					else message += "0";

					int result = client.Send (token, message);
					if (result != 0) return result;
					i++;
				}
				NetMgr.gameEvent=NetMgr.GameEvent.Start;
			}
		} else {
			//Debug.Log ("SendToAll: " + msg);
			foreach (ServerClient client in clients) {
				int result = client.Send (token, msg);
				if (result != 0) return result;
			}
		}
		return 0;
	}
	public int SendTo(int remoteID, NetMgr.NetworkToken token, string msg){
		Debug.Log ("SendTo " + remoteID + ": " + msg);
		return clients [remoteID].Send (token, msg);
	}
	public string GetList(){
		StringBuilder sb = new StringBuilder ();
		foreach (ServerClient sc in clients) {
			if(sc.status!=ServerClient.Status.Quited) sb.Append(sc.name+","+sc.id+";");
		}
		if(sb.Length>0) sb=sb.Remove(sb.Length-1,1);
		return sb.ToString ();
	}
}

public class ServerClient{

	public enum Status{
		Connected,
		Ready,
		Wait,
		Play,
		Paused,
		View,
		Quited,
	}

	public string name;
	public int id;
	public IPEndPoint iep;
	public Status status;

	UdpClient m_sender = new UdpClient ();
	public ServerClient(int _id,string _name, IPEndPoint _iep){
		id = _id;
		name = _name;
		iep = _iep;
		status = Status.Connected;
	}
	public int Send(NetMgr.NetworkToken token,string msg){
		if (status != Status.Quited) {
			string netmsg = token+"|" + msg;
			try{
				byte[] buffer = Encoding.Default.GetBytes(netmsg);
				m_sender.Send(buffer, buffer.Length, iep);
			}
			catch (Exception e){
				Debug.Log(e.ToString());
				return id+1;
			}
		}
		return 0;
	}
}
