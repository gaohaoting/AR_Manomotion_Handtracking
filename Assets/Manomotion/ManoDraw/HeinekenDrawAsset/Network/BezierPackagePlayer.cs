using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class BezierPackagePlayer : MonoBehaviour {

	public OscOut oscOut = null;
	public BezierPackageSenderUDP udpSender = null;

	public string filePath = "";
    public bool startPlayingDirectly = true;
    public bool repeatWhenDone = false;
    public bool undoAllOnRepeat = true;
    public float playFromTime = -1f;
    public float playToTime = -1f;
    public bool sortDataByTimeOnPlay = true;
    private bool _fileReachedEnd = false;
    [SerializeField]
    private float _playSpeed = 1f;
    public float playSpeed
    {
        get
        {
            return _playSpeed;
        }
        set
        {
            _playSpeed = value;
            if (sceneManager != null)
            {
                sceneManager.timeScaleFactor = _playSpeed;
			}
			if (udpSender != null) {
				udpSender.EnqueueTimeScaleFactor (_playSpeed, Time.time);
			}
        }
    }
    public WandPackage wandPackage = null;
    public BezierSplineSceneManager sceneManager = null;

    public Transform cameraTransform = null;
    public Transform leftControllerTransform = null;
    public Transform rightControllerTransform = null;

    private bool _isPlaying = false;
	public bool isPlaying {
		get {
			return _isPlaying;
		}
	}
	private float _timePlayer = -1f;
	private float _timeOfLastPackage = -1f;
	public float timePlayer {
		get {
			return _timePlayer;
		}
	}

	private Queue<BezierPackage> packages = new Queue<BezierPackage>();
	public int packagesRemaining {
		get {
			return packages.Count + (nextPackage == null ? 0 : 1);
		}
	}
	private BezierPackage nextPackage = null;

    public bool PathOk()
    {
        return filePath != null && filePath.Length > 0 && filePath[filePath.Length - 1] != '/';
    }
    public float NextPackageTime()
    {
        return nextPackage == null ? -1000f : nextPackage.timeAbsolute;
    }

	public void PlayFile() {
		if (_isPlaying) {
			StopPlayingFile ();
		}
		if (filePath == null || filePath.Length == 0) {
			Debug.LogWarning ("WandFile can't play file : '" + filePath + "'");
			return;
		}

		packages.Clear ();
        ArrayList packageList = new ArrayList();
		Debug.Log("Loading file : '" + filePath + "'");

		FileStream stream = new FileStream (filePath, FileMode.Open, FileAccess.Read);
		byte[] bytes = new byte[BezierPackage.PackageByteSize + 1];
		int totalBytesRead = 0;
		bool endOfFile = false;
		while (!endOfFile) {
			int bytesRead = stream.Read (bytes, 0, (int)BezierPackage.PackageByteSize);
			if (bytesRead == BezierPackage.PackageByteSize) {
				try {
					BezierPackage package = BezierPackage.Desserialize (bytes, 0);

					// Check if first package is ok, else the file is another type of file.

					if (package != null) {
                        if ((playFromTime < 0f || package.timeAbsolute >= playFromTime) &&
                            (playToTime < 0f || package.timeAbsolute <= playToTime))
                        {
                            if (sortDataByTimeOnPlay)
                            {
                                packageList.Add(package);
                            }
                            else
                            {
                                packages.Enqueue(package);
                            }
                        }
						totalBytesRead = bytesRead;
					} else {
						Debug.Log("Failed parsing bytes");
						endOfFile = true;
					}	
				}
				catch (Exception e) {
					Debug.Log("Failed to deserialize bytes with exception : " + e.ToString());
					endOfFile = true;
				}
			} else {
				endOfFile = true;
			}
		}

        // SORT LIST AND MOVE TO QUEUE
        if (sortDataByTimeOnPlay)
        {
            packageList.Sort();
			foreach (BezierPackage package in packageList)
            {
                packages.Enqueue(package);
            }
            packageList.Clear();
        }
        
		nextPackage = packages.Dequeue();
		
		_timeOfLastPackage = _timePlayer = nextPackage.timeAbsolute - 1f; // 1 sec delay before first action.
		_isPlaying = true;
        _fileReachedEnd = false;
	}

	public void StopPlayingFile () {
		if (_isPlaying) {
            if (wandPackage != null)
            {
				BezierPackage package = new BezierPackage();
                package.action = BezierPackageAction.kPackageActionEndSpline;
                wandPackage.OnReceivePackage(package);
            }
		}
		_isPlaying = false;
        _fileReachedEnd = false;
	}

    public void ClearSceneManager()
    {
        if (sceneManager != null)
        {
            sceneManager.UndoAll();
        }
    }


	// UNITY
	void Start () {
        if (startPlayingDirectly)
        {
            if (PathOk())
            {
                PlayFile();
            }
            else
            {
                Debug.LogWarning("PLAYER: Path '" + filePath + "' looks too suspicious to auto-play.");
            }
        }
	}

	void Update () {
		if (_isPlaying) {
			_timePlayer += Time.deltaTime * playSpeed;
			if (nextPackage == null) {
				StopPlayingFile ();
			} else {
				if (nextPackage.timeAbsolute < _timeOfLastPackage) {
					// Recorded data from another session?
					// reset timer, plus a little offset
					_timeOfLastPackage = _timePlayer = nextPackage.timeAbsolute - 1f;
				}
				while (nextPackage != null && _timePlayer >= nextPackage.timeAbsolute) {
                    if (nextPackage.action == BezierPackageAction.kPackageCameraPosition)
                    {
                        if (cameraTransform != null)
                        {
                            cameraTransform.position = nextPackage.pointPosition;
                            cameraTransform.rotation = nextPackage.pointInitialRotation;
                        }
                    }
                    else if (nextPackage.action == BezierPackageAction.kPackageLeftControllerPosition)
                    {
                        if (leftControllerTransform != null)
                        {
                            leftControllerTransform.position = nextPackage.pointPosition;
                            leftControllerTransform.rotation = nextPackage.pointInitialRotation;
                        }
                    }
                    else if (nextPackage.action == BezierPackageAction.kPackageRightControllerPosition)
                    {
                        if (rightControllerTransform != null)
                        {
                            rightControllerTransform.position = nextPackage.pointPosition;
                            rightControllerTransform.rotation = nextPackage.pointInitialRotation;
                        }
                    }
                    else if (wandPackage != null)
                    {
						wandPackage.OnReceivePackage(nextPackage);
                    }

					if (oscOut != null) {
						oscOut.Send (ConvertToMessage (nextPackage, playSpeed));
					}
					if (udpSender != null) {
						udpSender.EnqueuePackage (nextPackage);
					}

					_timeOfLastPackage = nextPackage.timeAbsolute;
					if (packages.Count > 0) {
						nextPackage = packages.Dequeue ();
					} else {
						StopPlayingFile ();
						nextPackage = null;
                        _fileReachedEnd = true;
					}
				}
			}
		}
        else if (_fileReachedEnd)
        {
            if (repeatWhenDone)
            {
                if (undoAllOnRepeat)
                {
					if (sceneManager != null) {
						sceneManager.UndoAll ();
					}
					if (oscOut != null) {
						OscMessage msg = new OscMessage ("/undoall");
						msg.Add (Time.time);
						oscOut.Send (msg);
					}
					if (udpSender != null) {
						udpSender.EnqueueUndoAll (Time.time);
					}
                }
                PlayFile();
            }
            _fileReachedEnd = false;
        }
	}


	// Package <-> OSC message 
	public static OscMessage ConvertToMessage(BezierPackage pkg, float playSpeed = 1f) {

		if (pkg.action == BezierPackageAction.kPackageActionUndo) {
			OscMessage msg = new OscMessage ("/undo");
			msg.Add (pkg.timeAbsolute);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageActionUndoAll) {
			OscMessage msg = new OscMessage ("/undoall");
			msg.Add (pkg.timeAbsolute);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageActionRedo) {
			OscMessage msg = new OscMessage ("/redo");
			msg.Add (pkg.timeAbsolute);
			return msg;
		}

		if (pkg.action == BezierPackageAction.kPackageActionSplitSpline) {
			OscMessage msg = new OscMessage ("/split");
			msg.Add (pkg.timeAbsolute);
			msg.Add (pkg.splineId);
			msg.Add (pkg.brushIndex);
			int pointIndex = (int)pkg.pointVelocity.x;
			msg.Add (pointIndex);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageActionRemovePointFromSpline) {
			OscMessage msg = new OscMessage ("/rempt");
			msg.Add (pkg.timeAbsolute);
			msg.Add (pkg.splineId);
			msg.Add (pkg.brushIndex);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageActionCreateContinuingSpline) {
			OscMessage msg = new OscMessage ("/cont");
			msg.Add (pkg.timeAbsolute);
			msg.Add (pkg.brushIndex);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageActionEndSpline) {
			OscMessage msg = new OscMessage ("/end");
			msg.Add (pkg.timeAbsolute);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageActionAddPoint || pkg.action == BezierPackageAction.kPackageActionSetLastPoint) {
			OscMessage msg = new OscMessage (pkg.action == BezierPackageAction.kPackageActionAddPoint ? "/addpt" : "/setpt");
			msg.Add (pkg.timeAbsolute);
			msg.Add (pkg.splineId);
			msg.Add (pkg.brushIndex);
			msg.Add (pkg.color);

			BezierPackageSenderOSC.AddVector3ToMessage (msg, pkg.pointPosition);
			BezierPackageSenderOSC.AddQuatToMessage (msg, pkg.pointInitialRotation);
			BezierPackageSenderOSC.AddVector3ToMessage (msg, pkg.pointVelocity);

			msg.Add (pkg.pointWidthPostClampScaleFactor);
			msg.Add (pkg.pointCustomParameter);

			msg.Add (playSpeed);

			return msg;
		}


		if (pkg.action == BezierPackageAction.kPackageCameraPosition) {
			OscMessage msg = new OscMessage ("/camera");
			msg.Add (pkg.timeAbsolute);
			BezierPackageSenderOSC.AddVector3ToMessage (msg, pkg.pointPosition);
			BezierPackageSenderOSC.AddQuatToMessage (msg, pkg.pointInitialRotation);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageCameraPosition) {
			OscMessage msg = new OscMessage ("/leftctrl");
			msg.Add (pkg.timeAbsolute);
			BezierPackageSenderOSC.AddVector3ToMessage (msg, pkg.pointPosition);
			BezierPackageSenderOSC.AddQuatToMessage (msg, pkg.pointInitialRotation);
			return msg;
		}
		if (pkg.action == BezierPackageAction.kPackageCameraPosition) {
			OscMessage msg = new OscMessage ("/rightctrl");
			msg.Add (pkg.timeAbsolute);
			BezierPackageSenderOSC.AddVector3ToMessage (msg, pkg.pointPosition);
			BezierPackageSenderOSC.AddQuatToMessage (msg, pkg.pointInitialRotation);
			return msg;
		}



		if (pkg.action == BezierPackageAction.kPackageBrushUpdate) {
			OscMessage msg = new OscMessage ("/brush");
			msg.Add (pkg.timeAbsolute);
			msg.Add (pkg.brushIndex);
			msg.Add (pkg.color);
			msg.Add (pkg.pointPosition.x);
			msg.Add (pkg.pointPosition.y);
			bool keepAnimatingAfterSplineEnded = pkg.pointPosition.z > 0.5f;
			msg.Add (keepAnimatingAfterSplineEnded);
			msg.Add (pkg.pointInitialRotation.x);
			msg.Add (pkg.pointInitialRotation.z);
			msg.Add (pkg.pointInitialRotation.w);
			return msg;
		}

		return null;
	}
}
