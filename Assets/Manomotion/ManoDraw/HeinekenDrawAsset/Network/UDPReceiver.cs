using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;


public class UDPReceiver {
	

	// THREAD
	public delegate void ThreadedDecodeDataDelegate(byte[] data, int length);
	private ThreadedDecodeDataDelegate decodeDataDelegate = null;
	Thread receiveThread = null;
	private bool _threadRunning = false;


	// PROPERTIES
	//private int _port = 8000;
	private int _isListeningToPort = -1;



	private string _status;
	public string Status {
		get {
			return _status;
		}
	}
	public bool IsListening {
		get {
			return _isListeningToPort >= 0;
		}
	}
	public bool IsThreadRunning {
		get {
			return _threadRunning;
		}
	}



	public void Listen(int port, ThreadedDecodeDataDelegate _delegate = null) {
		decodeDataDelegate = _delegate;

		if (port == _isListeningToPort) {
			_status = "Already listening to port " + _isListeningToPort;
			return;
		}
		Close ();

		_isListeningToPort = port;
		receiveThread = new Thread(new ThreadStart(ThreadedReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
		_status = "Listening to port " + _isListeningToPort;
	}
	public void Close() {
		if (IsListening) {
			_status = "Closed port " + _isListeningToPort;
			_isListeningToPort = -1;
			if (receiveThread != null) {
				receiveThread.Abort ();
				receiveThread = null;
			}
			return;
		}
		_status = "Not listening";
	}

	// receive thread
	private void ThreadedReceiveData()
	{
		_threadRunning = true;
		UdpClient client = new UdpClient(_isListeningToPort);
		IPEndPoint listenIP = new IPEndPoint(IPAddress.Any, _isListeningToPort);
		while (_threadRunning)
		{
			try
			{
				// Bytes empfangen.
				byte[] data = client.Receive(ref listenIP);
				if (data.Length > 0) {
					if (decodeDataDelegate != null) {
						decodeDataDelegate(data, data.Length);
					}
				}
			}
			catch (Exception err)
			{
				// Abort thread trigger this.
				//_isListening = false;
				_threadRunning = false;
				//Debug.Log("UDPReceive.thread : " + err.ToString());
			}
		}
		client.Close ();
		_threadRunning = false;
	}

}
