using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void OnSplineEndedDelegate(BezierSpline spline);
public delegate void OnSplineCreatedDelegate(BezierSpline spline);

public class BezierSplineSceneManager : MonoBehaviour {

	public bool updateDebugData = false;
	private bool didChange = true;
	private int _vertexCount = -1;
	public int VertexCount {
		get{ return _vertexCount; }
	}
	private int _triangleCount = -1;
	public int TriangleCount {
		get{ return _triangleCount; }
	}


    public bool isRemoteControlled = false;
	public GameObject splinePrefab = null;
    public BezierPackageSender packageSender = null;
	private BezierSpline currentSpline = null;
	private BezierSpline lastEndedSpline = null;
    private float _timeScaleFactor = 1f;
    public float timeScaleFactor
    {
        get
        {
            return _timeScaleFactor;
        }
        set
        {
            _timeScaleFactor = value;
            Debug.LogWarning("SCENE MANAGER: Setting timeScaleFactor = " + value);
        }
    }

	public int splineIdCounter = 0;

	public int vertexLimitForSpline = 60000;

	private Stack<GameObject> splines = new Stack<GameObject> ();
	private Stack<GameObject> undoStack = new Stack<GameObject> ();

	// WAND INTERFACE
	public BezierSpline CreateNewSpline(Vector3 startingPosition, int brushIndex, Color32 color, bool overrideBrushColor = false, bool muteNetworkPackage = false) {
		didChange = true;

        if (currentSpline != null)
        {
			EndCurrentSpline(muteNetworkPackage);
        }


		ClearUndoStack ();
		splineIdCounter++; // increase counter even if initialization fails

		//GameObject splineGameObject = new GameObject("Spline");
		GameObject splineGameObject = (GameObject)Instantiate (splinePrefab, startingPosition, Quaternion.identity);
		if (splineGameObject == null) {
			Debug.LogError ("Failed to initiate prefab.");
			return null;
		}
		BezierSpline spline = splineGameObject.AddComponent<BezierSpline> ();
		if (spline == null) {
			Destroy (splineGameObject);
			Debug.LogError ("Failed to add spline script to new spline game object");
			return null;
		}

		spline.id = splineIdCounter;
		splineGameObject.transform.parent = transform; // set new spline a child of the manager.
		splines.Push (splineGameObject);
		spline.IsExistingPointsEditable = false;
		spline.createTime = Time.time;
        spline.timeScaleFactor = timeScaleFactor;


		Settings settings = Settings.GetInstance();
		BrushSettings brush = settings.GetBrushById (brushIndex);
		if (brush == null) {
			Debug.LogWarning ("The brush (" + brushIndex + ") does not exist.");
		} else {
			spline.brushIndex = brushIndex;
			Material material = new Material(brush.brushMaterial);
			if (overrideBrushColor) {
				material.color = color;
				brush.color = color;
                spline.color = color;
            } else {
				//brush.color = color;
				material.color = brush.color;
				spline.color = brush.color;
            }
			splineGameObject.GetComponent<MeshRenderer> ().material = material;

            spline.extrudeShapeSettings = new ExtrudeShapeSettings(brush.extrudeShapeSettings);
            spline.splinePathSettings = new BezierSpline.SplinePathSettings(brush.splinePathSettings);
            spline.splineWidthSettings = new BezierSpline.SplineWidthSettings( brush.splineWidthSettings );
            spline.pointWidthSettings = new BezierPoint.PointWidthSettings(brush.pointWidthSettings);
		}

		currentSpline = spline;
		for (int i = 0; i < onSplineCreatedDelegates.Count; ++i) {
			onSplineCreatedDelegates [i] (spline);
		}

       

		return currentSpline;
	}

	public BezierSpline CreateContinuingSpline (bool invisibleSplit = false) {
		didChange = true;

        if (currentSpline == null)
        {
            return null;
        }

		return SplitSplineAtPointIndex (currentSpline, currentSpline.PointCount - 2, invisibleSplit);


	}
	public void EndCurrentSpline(bool muteNetworkPackage = false) {
		didChange = true;

        if (currentSpline != null)
        {
            lastEndedSpline = currentSpline;
            currentSpline = null;
            for (int i = 0; i < onSplineEndedDelegates.Count; ++i)
            {
                onSplineEndedDelegates[i](lastEndedSpline);
            }
			if (packageSender != null && !muteNetworkPackage)
            {
                packageSender.EnqueueEndSpline(Time.time);
            }
        }
	}

	public BezierPoint AddPointToCurrentSpline(OrientedMovingPoint point, float widthPostClampScaleFactor = 1f, float customParameter = 0f)
	{
		didChange = true;

        if (currentSpline == null)
            return null;

        BezierPoint bp0 = currentSpline.GetLastPoint();
        BezierPoint bp1 = currentSpline.AddPointGlobal(point);
        if (bp1 == null)
            return null;
		bp1.initialWidthPostClampScaleFactor = widthPostClampScaleFactor;
		bp1.initialCustomParameter = customParameter;
        bp1.CalcWidthAndThicknessTarget(bp0);

        if (packageSender != null)
        {
			packageSender.EnqueueAddPoint(currentSpline, point, currentSpline.createTime + bp1.createTimeRelativeToSpline, widthPostClampScaleFactor, customParameter);
        }

        return bp1;
    }

	public BezierPoint SetLastPointOfCurrentSpline(OrientedMovingPoint point, float widthPostClampScaleFactor = 1f, float customParameter = 0f)
	{
		didChange = true;

        if (currentSpline == null)
            return null;

        BezierPoint bp0 = currentSpline.GetLastPoint();
        BezierPoint bp1 = currentSpline.SetLastPointGlobal(point);
        if (bp1 == null)
			return null;
		bp1.initialWidthPostClampScaleFactor = widthPostClampScaleFactor;
		bp1.initialCustomParameter = customParameter;
        bp1.CalcWidthAndThicknessTarget(bp0);

        if (packageSender != null)
        {
			packageSender.EnqueueSetLastPoint(currentSpline, point, currentSpline.createTime + bp1.createTimeRelativeToSpline, widthPostClampScaleFactor, customParameter);
        }

        return bp1;
    }

	public bool Undo() {
		didChange = true;

		if (packageSender != null)
		{
			packageSender.EnqueueUndo(Time.time);
		}
		if (currentSpline == null && splines.Count > 0) {
			GameObject splineToRemove = splines.Pop ();
			undoStack.Push (splineToRemove);
			splineToRemove.SetActive (false);
            return true;
		}
		return false;
    }
    public bool UndoAll()
	{
		didChange = true;

        if (currentSpline != null)
        {
            return false;
        }
        while (splines.Count > 0)
        {
            GameObject splineToRemove = splines.Pop();
            undoStack.Push(splineToRemove);
            splineToRemove.SetActive(false);
        }
        if (packageSender != null)
        {
            packageSender.EnqueueUndoAll(Time.time);
        }
        return true;
    }
    public bool Redo()
	{
		didChange = true;

		if (packageSender != null)
		{
			packageSender.EnqueueRedo(Time.time);
		}
		if (currentSpline == null && undoStack.Count > 0) {
			GameObject splineToReinsert = undoStack.Pop ();
			splines.Push (splineToReinsert);
			splineToReinsert.SetActive (true);
            return true;
		}
		return false;
	}
	/*public void ClearAll() {
		while (splines.Count > 0) {
			GameObject splineToDestroy = splines.Pop ();
			Destroy (splineToDestroy);
		}
		ClearUndoStack ();
	}*/


	// INTERNAL FUNCTIONS
	private void ClearUndoStack() {
		didChange = true;

		while (undoStack.Count > 0) {
			GameObject splineToDestroy = undoStack.Pop ();
			Destroy (splineToDestroy);
		}
	}



	// GET SPLINES
	public BezierSpline GetSplineById (int id) {
		if (id < 0 || id >= splines.Count)
			return null;
		BezierSpline[] splinesInScene = gameObject.GetComponentsInChildren<BezierSpline> ();
		for (int i = 0; i < splinesInScene.Length; ++i) {
			if (splinesInScene [i].id == id) {
				return splinesInScene [i];
			}
		}
		return null;
	}
	public BezierSpline GetCurrentSpline() {
		return currentSpline;
	}
    public void SetCurrentSpline(BezierSpline spline)
    {
        currentSpline = spline;
    }
	public BezierSpline GetLastEndedSpline() {
		return lastEndedSpline;
	}


	// CALLBACKS
	protected List<OnSplineEndedDelegate> onSplineEndedDelegates = new List<OnSplineEndedDelegate>();
	public int OnSplineEndedDelegateCount {
		get{
			return onSplineEndedDelegates.Count;
		}
	}
	public void AddOnSplineEndedDelegate(OnSplineEndedDelegate del) {
		onSplineEndedDelegates.Add (del);
	}
	protected List<OnSplineCreatedDelegate> onSplineCreatedDelegates = new List<OnSplineCreatedDelegate>();
	public int OnSplineCreatedDelegateCount {
		get{
			return onSplineCreatedDelegates.Count;
		}
	}
	public void AddOnSplineCreatedDelegate(OnSplineCreatedDelegate del) {
		onSplineCreatedDelegates.Add (del);
	}



	// UNITY
	void Start () {
		splines = new Stack<GameObject> ();
		undoStack = new Stack<GameObject> ();

		didChange = true;


		/*if (wandGameObject == null) {
			Debug.LogError ("No Wand Game Object connected to Scene Manager.");
		} else {
			WandSimulator wandSimulator = wandGameObject.GetComponent<WandSimulator> ();
			if (wandSimulator != null) {
				//wandSimulator.SetSceneManager (this);	
				wandSimulator.sceneManager = this;
			} else {
				Debug.LogError ("The Wand Game Object is not a valid Wand.");
			}
		}*/
	}



    // SPLIT SPLINES
    // returns the last part of the split spline
    private BezierSpline SplitSplineAtMarkedPoints(BezierSpline spline)
	{
		didChange = true;

        BezierSpline remainingSpline = spline;
        BezierSpline firstPartSpline = null;
        while(remainingSpline != null && remainingSpline.needsSplit)
        {
            int i = 0;
            BezierPoint point = remainingSpline.GetPoint(i);
            while (point != null && !point.splitSplineHere)
            {
                ++i;
                point = remainingSpline.GetPoint(i);
            }
            if (point != null) // this point is set for split
            {
                point.splitSplineHere = false;
                firstPartSpline = remainingSpline;
                remainingSpline = SplitSplineAtPointIndex(firstPartSpline, i);
                firstPartSpline.needsSplit = false;
            } else // no more points to split
            {
                remainingSpline.needsSplit = false;
            }
        }
        return remainingSpline;
    }

    // Returns the last, new half of the spline, or null if split was not possible. 
	public BezierSpline SplitSplineAtPointIndex(BezierSpline spline, int index, bool invisibleSplit = false)
	{
		didChange = true;

        if (spline == null || index <= 0 || index >= spline.PointCount - 1)
        {
            //Debug.Log("Split spline ...");
            return null;
        }

		//Debug.Log("Split spline " + spline.id + " at point " + index);

		spline.color = Settings.GetInstance().GetBrushById(0).color;
        BezierSpline newSpline = CreateNewSpline(spline.transform.position, spline.brushIndex, spline.color, true, true);
        if (newSpline == null)
        {
            Debug.LogError("Failed to split spline");
            return null;
        }
        
        //splineGameObject.transform.parent = spline.transform; // set new spline a child of the spline splitting from.
        newSpline.createTime = spline.createTime;
        newSpline.IsBeingDrawnByWand = spline.IsBeingDrawnByWand;
        spline.IsBeingDrawnByWand = false;

        newSpline.CopyPointsFromOther(spline, index);
        spline.RemovePoints(index + 1, spline.PointCount-1);
        newSpline.wasSplitAtFront = true;
        spline.wasSplitAtEnd = true;
		newSpline.ghostPointFront = invisibleSplit ? spline.GetPoint(spline.PointCount - 2) : null;
		spline.ghostPointEnd = invisibleSplit ? newSpline.GetPoint(1) : null;
        spline.SetDirtySplineFromPoint(spline.PointCount - 3);

        float lastV = spline.GetLargestVCoord();
        newSpline.VCoordOffset = lastV - (int)lastV;

        //Debug.Log("Split spline");

        if (packageSender != null)
        {
            packageSender.EnqueueSplitSplineAtIndex(spline, newSpline, index, Time.time);
        }

        if (spline == currentSpline)
        {
            currentSpline = newSpline;
        }

        return newSpline;
    }

	public void RemovePointFromSpline(BezierSpline spline, int pointIndex) {
		didChange = true;

		if (spline != null) {
			spline.RemovePoint (pointIndex);
			if (packageSender != null) {
				packageSender.EnqueueRemovePointFromSpline (spline, pointIndex, Time.time);
			}
		}
	}

	public void RemoveMarkedPointsFromSpline(BezierSpline spline) {
		didChange = true;

		int i = 0;
		BezierPoint point = spline.GetPoint (i);
		while (point != null) {
			if (point.removeThisPoint) {
				spline.RemovePoint (i);
				if (packageSender != null) {
					packageSender.EnqueueRemovePointFromSpline (spline, i, Time.time);
				}
			} else {
				++i;
			}
			point = spline.GetPoint (i);
		}
	}

	void Update () {
		if (!isRemoteControlled && currentSpline != null) {
			if (currentSpline.hasPointsToRemove) {
				RemoveMarkedPointsFromSpline (currentSpline);
			}	
			if (currentSpline.needsSplit) {
				currentSpline = SplitSplineAtMarkedPoints (currentSpline);
				if (currentSpline == null) {
					Debug.Log ("Current spline is null after split.");
				}
			}
        }
        /*if (lastEndedSpline != null && lastEndedSpline.needsSplit)
        {
            lastEndedSpline = SplitSplineAtMarkedPoints(lastEndedSpline);
        }*/

		if (updateDebugData && didChange) {
			_vertexCount = 0;
			_triangleCount = 0;
			BezierSpline[] splinesInScene = gameObject.GetComponentsInChildren<BezierSpline> ();
			for (int i = 0; i < splinesInScene.Length; ++i) {
				if (splinesInScene [i].enabled) {
					_vertexCount += splinesInScene [i].VertexCount;
					_triangleCount += splinesInScene [i].TriangleCount;
				}
			}
			didChange = false;
		}
    }



	// Debug


}
