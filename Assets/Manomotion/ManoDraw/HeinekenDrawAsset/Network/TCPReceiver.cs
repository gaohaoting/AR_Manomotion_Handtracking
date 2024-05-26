using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;


public class TCPReceiver {

	public delegate void ThreadedDecodeDataDelegate(byte[] data, int length);
	private ThreadedDecodeDataDelegate decodeDataDelegate = null;
	Thread receiveThread = null;
	private bool _threadRunning = false;
	//TcpClient client = null;

	// PROPERTIES
	private string _isListeningToIp = "127.0.0.1";
	private int _isListeningToPort = -1;
	public int bufferSize = 1024;



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

		/*TcpListener listener = new TcpListener(IPAddress.Any, _isListeningToPort);
		listener.
		client = listener.AcceptTcpClient();*/

		receiveThread = new Thread(new ThreadStart(ThreadedReceiveData));
		receiveThread.IsBackground = true;
		receiveThread.Start();
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
		_status = "Listening for client at " + _isListeningToIp + ":" + _isListeningToPort;

		/*if (Security.PrefetchSocketPolicy (_isListeningToIp, _isListeningToPort)) {
			Debug.Log ("TCPReceiver Security ok");
		} else
		{
			Debug.Log ("TCPReceiver Security fail");
		}*/
		//TcpListener listener = new TcpListener(IPAddress.Parse(_isListeningToIp), _isListeningToPort);
		TcpListener listener = new TcpListener(IPAddress.Any, _isListeningToPort);
		listener.Start ();
		TcpClient client = listener.AcceptTcpClient();
		NetworkStream stream = client.GetStream ();
		listener.Stop ();

		_status = "Listening to client " + client.Client.RemoteEndPoint.ToString ();
		//Debug.Log("TCPReceiver Client connected from " + client.Client.RemoteEndPoint.ToString ());

		int _bufferSize = bufferSize;
		byte[] data = new byte[_bufferSize];
		while (_threadRunning)
		{
			try
			{
				int bytes = stream.Read(data, 0, _bufferSize);
				if (bytes > 0) {
					if (decodeDataDelegate != null) {
						decodeDataDelegate(data, bytes);
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
		stream.Close ();
		client.Close ();
		_isListeningToPort = -1;
		_threadRunning = false;
	}

}
