using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;

public class BezierPackageRecorder : MonoBehaviour {

	public string outputFolder = "~/Desktop";
	protected FileStream stream = null;
	public string currentFileName = "";
	public int packagesWritenToFile = 0;
	public int filesWritten = 0;
    public bool startWhenCalledUpon = false;


	//protected Queue<BezierUDPPointPackage> packagesToSave = new Queue<BezierUDPPointPackage> ();
	protected float timeLastWrite = 0f;
	//protected float timeInLastPackage = -1f;

    public bool IsRecording()
    {
        return stream != null;
    }

    public void StartRecording()
    {
        if (IsRecording())
        {
            StopRecording();
        }

        currentFileName = "UDP" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".heinekendraw";
        stream = new FileStream(outputFolder + "/" + currentFileName, FileMode.Append, FileAccess.Write);
        if (outputFolder.Length == 0)
        {
            Debug.LogWarning("UDP Recorder: Path is empty. Set output folder?");
        }
        packagesWritenToFile = 0;
        filesWritten++;
    }

	public void RecordBytes(byte[] bytes) {

		if (startWhenCalledUpon && !IsRecording())
		{
			StartRecording();
		}

		if (IsRecording())
		{
			stream.Write(bytes, 0, bytes.Length);
			timeLastWrite = Time.time;
			packagesWritenToFile++;
		}
	}

	public void RecordPackage(BezierPackage package) {
		//packagesToSave.Enqueue (package);

        if (startWhenCalledUpon && !IsRecording())
        {
            StartRecording();
        }

		if (IsRecording())
        {
			stream.Write(package.Serialize(), 0, (int)BezierPackage.PackageByteSize);
            timeLastWrite = Time.time;
            packagesWritenToFile++;
        }
	}

	public void RecordOscMessage(OscMessage message) {
		BezierPackage package = ConvertToPackage (message);
		if (package != null)
			RecordPackage (package);
	}

	// THREAD
	/*
	Thread saveThread = null;
	private bool _threadRunning = false;
	static readonly object _locker = new object();


	// Threaded function
	private void ThreadedRecorder()
	{
		_threadRunning = true;
		while (_threadRunning)
		{
			lock (_locker) {
				int packagesCount = packagesToSave.Count;
				bytes = new byte[packagesCount * BezierUDPPointPackage.PackageByteSize];
				for (int i = 0; i < packagesCount; ++i) {
					
				}
			}
			try
			{
				
			}
			catch (Exception err)
			{
				// Abort thread trigger this.
				_threadRunning = false;
			}

			System.Threading.Thread.Sleep (1000);
		}
		_threadRunning = false;
	}
	private void TreadedSave() {
		if 
	}
	*/

	public void StopRecording() {
		// stop thread.

		if (stream != null) {
			stream.Close ();
			stream = null;
		}
	}


	// UNITY
	public void Reset() {
		StopRecording ();
	}

	public void OnDisable() {
        StopRecording();
	}

	// OSC message <-> Package 
	public static BezierPackage ConvertToPackage(OscMessage msg) {

		if (msg.address.CompareTo ("/undo") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageActionUndo;
			msg.TryGet (0, out package.timeAbsolute);
			return package;
		}
		if (msg.address.CompareTo ("/undoall") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageActionUndoAll;
			msg.TryGet (0, out package.timeAbsolute);
			return package;
		}
		if (msg.address.CompareTo ("/redo") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageActionRedo;
			msg.TryGet (0, out package.timeAbsolute);
			return package;
		}

		if (msg.address.CompareTo ("/split") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageActionSplitSpline;
			msg.TryGet (0, out package.timeAbsolute);
			msg.TryGet (1, out package.splineId);
			msg.TryGet (2, out package.brushIndex);
			int pointIndex;
			msg.TryGet (3, out pointIndex);
			package.pointVelocity.x = pointIndex;
			return package;
		}
		if (msg.address.CompareTo ("/rempt") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageActionRemovePointFromSpline;
			msg.TryGet (0, out package.timeAbsolute);
			msg.TryGet (1, out package.splineId);
			msg.TryGet (2, out package.brushIndex);
			return package;
		}
		if (msg.address.CompareTo ("/cont") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageActionCreateContinuingSpline;
			msg.TryGet (0, out package.timeAbsolute);
			msg.TryGet (1, out package.brushIndex);
			return package;
		}
		if (msg.address.CompareTo ("/end") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageActionEndSpline;
			msg.TryGet (0, out package.timeAbsolute);
			return package;
		}
		if (msg.address.CompareTo ("/addpt") == 0 || msg.address.CompareTo ("/setpt") == 0) {

			// OSC /addpt
			//    float timeAbs
			//   'Spline' spline data
			// 	 'Point' point data
			//    float width factor
			// 	  float custom param

			BezierPackage package = new BezierPackage();
			package.action = msg.address.CompareTo ("/addpt") == 0 ? BezierPackageAction.kPackageActionAddPoint : BezierPackageAction.kPackageActionSetLastPoint;
			msg.TryGet (0, out package.timeAbsolute);

			msg.TryGet (1, out package.splineId);
			msg.TryGet (2, out package.brushIndex);
			msg.TryGet (3, out package.color);
			package.pointPosition = BezierPackageReceiverOSC.ReadVector3FromMessage (msg, 4);
			package.pointInitialRotation = BezierPackageReceiverOSC.ReadQuatFromMessage (msg, 7);
			package.pointVelocity = BezierPackageReceiverOSC.ReadVector3FromMessage (msg, 11);
			msg.TryGet (14, out package.pointWidthPostClampScaleFactor);
			msg.TryGet (15, out package.pointCustomParameter);

			return package;
		}


		if (msg.address.CompareTo ("/camera") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageCameraPosition;
			msg.TryGet (0, out package.timeAbsolute);
			package.pointPosition = BezierPackageReceiverOSC.ReadVector3FromMessage (msg, 1);
			package.pointInitialRotation = BezierPackageReceiverOSC.ReadQuatFromMessage (msg, 4);
			return package;
		}
		if (msg.address.CompareTo ("/leftctrl") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageLeftControllerPosition;
			msg.TryGet (0, out package.timeAbsolute);
			package.pointPosition = BezierPackageReceiverOSC.ReadVector3FromMessage (msg, 1);
			package.pointInitialRotation = BezierPackageReceiverOSC.ReadQuatFromMessage (msg, 4);
			return package;
		}
		if (msg.address.CompareTo ("/rightctrl") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageRightControllerPosition;
			msg.TryGet (0, out package.timeAbsolute);
			package.pointPosition = BezierPackageReceiverOSC.ReadVector3FromMessage (msg, 1);
			package.pointInitialRotation = BezierPackageReceiverOSC.ReadQuatFromMessage (msg, 4);
			return package;
		}



		if (msg.address.CompareTo ("/brush") == 0) {
			BezierPackage package = new BezierPackage();
			package.action = BezierPackageAction.kPackageBrushUpdate;
			msg.TryGet (0, out package.timeAbsolute);
			msg.TryGet (1, out package.brushIndex);
			msg.TryGet (2, out package.color);
			msg.TryGet (3, out package.pointPosition.x);
			msg.TryGet (4, out package.pointPosition.y);
			bool keepAnimatingAfterSplineEnded;
			msg.TryGet (5, out keepAnimatingAfterSplineEnded);
			package.pointPosition.z = keepAnimatingAfterSplineEnded ? 1f : 0f;
			msg.TryGet (6, out package.pointInitialRotation.x);
			msg.TryGet (7, out package.pointInitialRotation.z);
			msg.TryGet (8, out package.pointInitialRotation.w);
			return package;
		}

		return null;
	}
}
