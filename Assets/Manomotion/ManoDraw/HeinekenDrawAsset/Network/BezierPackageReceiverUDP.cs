using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class BezierPackageReceiverUDP : BezierPackageReceiver {

	public UDPReceiver networkReceiver = new UDPReceiver();

	public WandPackage wandPackage = null;

	public int port = 8000;

    public int messageLogLength = 0;
	public List<BezierPackage> messageLog = new List<BezierPackage> ();

	private Queue<BezierPackage> messageQueue = new Queue<BezierPackage> ();
	public int MessageCount {
		get {
			return messageQueue.Count;
		}
	}
	public BezierPackage PeekNextMessage() {
		if (MessageCount > 0) {
			return messageQueue.Peek ();
		}
		return null;
	}
    private bool useMutex = false;
    private Mutex queueMutex = new Mutex();


	// Overriding base class
	public override string Status()
	{
		if (networkReceiver != null)
			return networkReceiver.Status;
		return "No UDPReceiver connected";
	}

	public void Start () {
		if (networkReceiver != null) {
			networkReceiver.Listen (port, ThreadedDecodeData);
		}
		messageQueue.Clear ();
	}

	public void Stop () {
		if (networkReceiver != null) {
			networkReceiver.Close ();
		}
		messageQueue.Clear ();
	}

	protected void SetTransformFromPackage(Transform transform, BezierPackage package)
    {
        if (transform != null)
        {
            transform.position = package.pointPosition;
            transform.rotation = package.pointInitialRotation;
        }
    }

	protected void UpdateBrushFromPackage(Settings settings, BezierPackage package)
    {
        if (settings == null)
            return;

        BrushSettings brush = settings.GetBrushById(package.brushIndex);
        if (brush == null)
        {
            Debug.LogWarning("Trying to set remote brush that does not exist locally (" + package.brushIndex + ")");
            return;
        }

        brush.splinePathSettings.simplifyStraightSectionsBelowAngle = package.pointPosition.x;
        brush.splinePathSettings.simplifyStraightSectionsBelowWidthDiff = package.pointPosition.y;
        brush.splineWidthSettings.keepAnimatingAfterSplineEnded = package.pointPosition.z > 0.5f ? true : false;

        brush.pointWidthSettings.widthAnimationTime = package.pointInitialRotation.x;
        //brush.pointWidthSettings.customPostClampWidthMultiplier = package.pointInitialRotation.y;
        brush.pointWidthSettings.minWidth = package.pointInitialRotation.z;
        brush.pointWidthSettings.maxWidth = package.pointInitialRotation.w;
    }


    void Update() {
        if (!useMutex || queueMutex.WaitOne(2))
        {
            while (MessageCount > 0)
            {
				BezierPackage package = messageQueue.Dequeue();
                if (package != null)
                {
                    if (package.action == BezierPackageAction.kPackageCameraPosition)
                    {
                        SetTransformFromPackage(cameraTransform, package);
                    }
                    else if (package.action == BezierPackageAction.kPackageLeftControllerPosition)
                    {
                        SetTransformFromPackage(leftControllerTransform, package);
                    }
                    else if (package.action == BezierPackageAction.kPackageRightControllerPosition)
                    {
                        SetTransformFromPackage(rightControllerTransform, package);
                    }
                    else if (package.action == BezierPackageAction.kPackageBrushUpdate)
                    {
                        UpdateBrushFromPackage(settings, package);
                    }
                    else if (wandPackage != null)
                    {
                        wandPackage.OnReceivePackage(package);
                    }
                    if (recorder != null)
                    {
						if (package.action != BezierPackageAction.kPackageTimeScaleFactor)
	                        recorder.RecordPackage(package);
                    }
                    if (messageLogLength > 0)
                    {
                        if (package.action != BezierPackageAction.kPackageActionSetLastPoint)
                        {
                            messageLog.Add(package);
                        }
                        while (messageLog.Count > messageLogLength)
                        {
                            messageLog.RemoveAt(0);
                        }
                    }
                }
            }
            if (useMutex)
                queueMutex.ReleaseMutex();
        }
	}

	protected void ThreadedDecodeData(byte[] data, int length) {

        if (useMutex)
            queueMutex.WaitOne();

		int offset = 0;
		while (length - offset >= BezierPackage.PackageByteSize) {
			BezierPackage package = BezierPackage.Desserialize (data, offset);
			messageQueue.Enqueue (package);
			// can't call delgates here because it will interfere with main thread.
			offset += (int)BezierPackage.PackageByteSize;
		}

        if (useMutex)
            queueMutex.ReleaseMutex();

		if (length - offset > 0) {
			Debug.Log ("PackageReceiver received data which could not be decoded. (length = " + length + ", offset = " + offset + ")" );
		}
	}
}
