using UnityEngine;
using System.Collections;


public class DrawingWand : WandBase
{

    private static DrawingWand _instance;
    public static DrawingWand Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }

    // TRANSFORM
    public Transform controllerTransform = null;
    public float widthPostClampScaleFactor = 1f;
    public float customParameter = 0f;

    // UNDO / REDO
    public bool undoButtonDown
    {
        set
        {
            if (value && !_drawButtonPressed)
            {
                sceneManager.Undo();
            }
        }
    }
    public bool undoAllButtonDown
    {
        set
        {
            if (value && !_drawButtonPressed)
            {
                sceneManager.UndoAll();
            }
        }
    }
    public bool redoButtonDown
    {
        set
        {
            if (value && !_drawButtonPressed)
            {
                sceneManager.Redo();
            }
        }
    }

    // DRAW
    protected bool _drawButtonPressed = false;
    protected bool _drawButtonDown = false;
    protected bool _drawButtonUp = false;

    public bool drawButtonDown
    {
        get
        {
            return _drawButtonDown;
        }
        set
        {
            if (value && !_drawButtonPressed)
            {
                _drawButtonDown = true;
                _drawButtonPressed = true;
            }
        }

    }
    public bool drawButtonUp
    {
        get
        {
            return _drawButtonUp;
        }
        set
        {
            if (value && _drawButtonPressed)
            {
                _drawButtonUp = true;
                _drawButtonPressed = false;
            }
        }

    }
    public bool drawButtonPressed
    {
        get
        {
            return _drawButtonPressed;
        }
        set
        {
            _drawButtonPressed = value;
        }
    }



    // DRAW TIP ADJUSTMENT
    public Vector3 offsetPosition = Vector3.zero;
    public Quaternion offsetRotation = Quaternion.identity;


    private OrientedMovingPoint prevPoint = null;
    private OrientedMovingPoint prevSamplePoint = null;
    private float prevSampleTime = 0f;
    private OrientedMovingPoint currentPoint = null;

    //public float[] smoothVelocity = new float[] { 1 }; // no smoothing
    //public float[] smoothRotation = new float[] { 1 }; // no smoothing

    public bool drawWandDebug = true;

    public void Update()
    {
        if (controllerTransform != null)
        {
            transform.rotation = controllerTransform.rotation * offsetRotation;
            transform.position = controllerTransform.position + transform.rotation * offsetPosition;
        }
        if (prevPoint == null)
        {
            prevPoint = new OrientedMovingPoint();
        }
        currentPoint = new OrientedMovingPoint(
            transform.position,
            transform.rotation,
            (transform.position - prevPoint.position) / Time.smoothDeltaTime,
            Vector3.zero
        );
        if (prevSampleTime > 0f)
        {
            currentPoint.velocity = (currentPoint.position - prevSamplePoint.position) / (Time.time - prevSampleTime);
        }

        if (_drawButtonPressed)
        {
            if (sceneManager == null)
            {
                Debug.LogWarning("WandProxy has no SceneManager.");
                return;
            }

            BezierSpline currentSpline = sceneManager.GetCurrentSpline();

            if (_drawButtonDown || currentSpline == null)
            {

                // get brush sample distance
                BrushSettings brush = Settings.GetInstance().GetBrushById(selectedBrush);
                if (brush != null)
                {
                    minSampleDistance = brush.splinePathSettings.pointSampleMinDistance;
                }

                // create new spline           
                currentSpline = StartNewSplineWithPoint(currentPoint, widthPostClampScaleFactor, customParameter);
                prevSamplePoint = new OrientedMovingPoint(currentPoint);

                _drawButtonDown = false;

            }
            else
            {

                // add point if sample distance is passed threshold
                if ((currentPoint.position - prevSamplePoint.position).sqrMagnitude > _minSampleDistanceSquared)
                {
                    // create continuing spline when vertex count is passed
                    if (currentSpline.VertexCount >= sceneManager.vertexLimitForSpline)
                    {
                        BezierSpline prevSpline = currentSpline;
                        currentSpline = sceneManager.CreateContinuingSpline(true);
                        if (currentSpline == null)
                        {
                            Debug.LogWarning("Failed to create continuing spline.");
                        }
                    }

                    // add point
                    sceneManager.AddPointToCurrentSpline(currentPoint, widthPostClampScaleFactor, customParameter);
                    prevSamplePoint = new OrientedMovingPoint(currentPoint);

                }
                else
                {
                    // Keep moving last point at wand position
                    if (currentSpline.PointCount == 1)
                    {
                        sceneManager.AddPointToCurrentSpline(currentPoint, widthPostClampScaleFactor, customParameter);
                    }
                    else
                    {
                        sceneManager.SetLastPointOfCurrentSpline(currentPoint, widthPostClampScaleFactor, customParameter);
                    }
                }
            }
        }

        if (_drawButtonUp)
        {

            // end spline
            EndCurrentSpline();
            prevSampleTime = 0f;

            _drawButtonUp = false;
        }

        prevPoint = new OrientedMovingPoint(currentPoint);



        // Debug
        if (drawWandDebug)
        {
            Debug.DrawLine(transform.position + Vector3.zero, transform.position + transform.rotation * new Vector3(0.1f, 0, 0), Color.red);
            Debug.DrawLine(transform.position + Vector3.zero, transform.position + transform.rotation * new Vector3(0, 0.1f, 0), Color.green);
            Debug.DrawLine(transform.position + Vector3.zero, transform.position + transform.rotation * new Vector3(0, 0, 0.1f), Color.blue);
        }
    }


    new void Start()
    {
        base.Start();
    }

    private void Awake()
    {
        if (!_instance)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            Debug.Log("Destroyed " + gameObject.name + ". More than 1 Drawing Wand instances in scene");
        }
    }
}
