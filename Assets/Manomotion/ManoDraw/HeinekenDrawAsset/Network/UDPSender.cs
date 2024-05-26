/*
     
*/
using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
//using System.Threading;

public class UDPSender
{
	// prefs
	private string _ip = "";
	private int _port = 0;

	// "connection" things
	IPEndPoint remoteEndPoint = null;
	UdpClient client = null;

	// gui
	private string _status;
	public string Status {
		get {
			return _status;
		}
	}
	private int _bytesSent = 0;
	public int BytesSent {
		get {
			return _bytesSent;
		}
	}
	public bool IsConnected {
		get {
			return remoteEndPoint != null;
		}
	}

	public void Connect(string ip, int port) {
		if (client != null) {
			Disconnect ();
		}
		_ip = ip;
		_port = port;
		client = new UdpClient ();
		remoteEndPoint = new IPEndPoint(IPAddress.Parse(_ip), _port);
		client.Connect (remoteEndPoint);
		_status = "Sending to " + _ip + ":" + _port;
	}
	public void Disconnect() {
		remoteEndPoint = null;
		if (client != null) {
			client.Close ();
			client = null;
		}
		_status = "Not sending";
	}

	public void Send(byte[] data, int bytes) {
		if (remoteEndPoint == null || client == null) {
			return;
		}
		try {
			_bytesSent += client.Send(data, bytes);

		} catch (Exception e) {
			Debug.Log (e.ToString ());
			Disconnect ();
			_status = "Error sending, disconnected";
			//_status = "Error sending : " + e.ToString ();
		}
	}
	public void Send(byte[] data) {
		Send(data, data.Length);
	}
	public void SendString(string s) {
		Send(Encoding.UTF8.GetBytes(s));
	}
}