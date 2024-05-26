/*
     
*/
using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
//using System.Threading;

public class TCPSender
{
	// prefs
	private string _ip = "";
	private int _port = 0;

	// "connection" things
	TcpClient client = null;

	// gui
	//private string _status;
	public string Status {
		get {
			if (client == null || !client.Connected) {
				return "Not connected";
			}
			if (client.GetStream ().CanWrite) {
				return "Can write to " + _ip + ":" + _port;
			}
			return "Connected but can't write to " + _ip + ":" + _port;
		}
	}
	//private int _bytesSent = 0;
	public int BytesSent {
		get {
			//return _bytesSent;
			if (client != null) {
				return (int)client.GetStream ().Length;
			}
			return 0;
		}
	}
	public bool IsConnected {
		get {
			return client != null && client.Connected;
		}
	}

	public void Connect(string ip, int port) {
		if (client != null) {
			Disconnect ();
		}
		_ip = ip;
		_port = port;
		try {
		client = new TcpClient ();
			Debug.Log("TCPSender:Connect to " + _ip + ":" + _port);
			client.BeginConnect(IPAddress.Parse(_ip), _port, new AsyncCallback(ConnectCallback), client);
		}
		catch (ArgumentNullException e) 
		{
			Debug.Log("TCPSender:Connect ArgumentNullException: " + e.ToString());
		} 
		catch (SocketException e) 
		{
			Debug.Log("TCPSender:Connect SocketException: " + e.ToString());
		}
	}
	private static void ConnectCallback(IAsyncResult ar) {
		try {
			// Retrieve the socket from the state object.
			TcpClient c = (TcpClient)ar.AsyncState;
			c.EndConnect(ar);
			Debug.Log("TCPSender:ConnectCallback Connected");
		} catch (Exception e) {
			Debug.Log("TCPSender:ConnectCallback Exception: " + e.ToString());
		}
	}
	public void Disconnect() {
		if (client != null) {
			client.Close ();
			client = null;
		}
	}

	public void Send(byte[] data, int bytes) {
		if (!IsConnected) {
			return;
		}
		try {
			NetworkStream stream = client.GetStream();
			if (stream.CanWrite) {
				//stream.Write(data, 0, bytes);
				stream.BeginWrite(data, 0, bytes, new AsyncCallback(SendCallback), stream);
			}
		} catch (IOException e) {
			Debug.Log ("TCPSender:Send IOException: " + e.ToString ());
			Disconnect ();
		} catch (Exception e) {
			Debug.Log ("TCPSender:Send Exception: " + e.ToString ());
			Disconnect ();
		}
	}
	private static void SendCallback(IAsyncResult ar) {
		try {
			NetworkStream s = (NetworkStream)ar.AsyncState;
			s.EndWrite(ar);
		} catch (Exception e) {
			Debug.Log("TCPSender:SendCallback Exception: " + e.ToString());
		}
	}
	public void Send(byte[] data) {
		Send(data, data.Length);
	}
	public void SendString(string s) {
		Send(Encoding.UTF8.GetBytes(s));
	}
}