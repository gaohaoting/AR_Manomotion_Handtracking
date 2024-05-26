using UnityEngine;
using System.Collections;

public enum WandSimulationMovementType {
	kWandSimulationMovementConst,
	kWandSimulationMovementSpiral,
	kWandSimulationMovementRandomWalk
}

public class WandSimulator : WandBase {

	public WandSimulationMovementType simulationMovementType = WandSimulationMovementType.kWandSimulationMovementConst;



	// ANIMATION VARIABLES
	public float animationSpeed = 1f;
	public Vector3 animationTarget = Vector3.zero;
	public float animationDistanceFromTarget = 3f;

	public float secondsDrawDown = 5f;
	public float secondsDrawUp = 3f;
	public float secondsUndo = 4f;
	public float secondsRedo = 5f;


	// WAND STATE
	private bool drawButtonDown = false;
	public void OnDrawButton (bool down = false) {
		if (down && !drawButtonDown) {
			BezierSpline spline = StartNewSplineWithPoint (currentPoint);
			drawButtonDown = true;
			drawButtonTime = Time.time;
		} else if (!down && drawButtonDown) {
			EndCurrentSpline ();
			drawButtonDown = false;
			drawButtonTime = Time.time;
		}
	}
	public bool IsDrawButtonDown() {
		return drawButtonDown;
	}
	private float drawButtonTime = 0;

	public void OnUndoButton(bool down = false) {
		if (!drawButtonDown && down) {
			if (sceneManager != null) {
                sceneManager.Undo();
			}
		}
		undoButtonTime = Time.time;
	}
	private float undoButtonTime = 0;

	public void OnRedoButton(bool down = false) {
		if (!drawButtonDown && down) {
			if (sceneManager != null) {
                sceneManager.Redo();
            }
		}
		redoButtonTime = Time.time;
	}
	private float redoButtonTime = 0;



	private OrientedMovingPoint prevPoint = new OrientedMovingPoint (Vector3.zero, Quaternion.identity, Vector3.forward, Vector3.zero);
	private OrientedMovingPoint prevSamplePoint = new OrientedMovingPoint();
	private OrientedMovingPoint currentPoint = new OrientedMovingPoint();



	// UNITY
	new void Start () {
		base.Start();
		prevSamplePoint = new OrientedMovingPoint(transform.position, transform.rotation, Vector3.zero, Vector3.zero);
		drawButtonTime = Time.time;
		undoButtonTime = Time.time;
		redoButtonTime = Time.time;
	}

	void Update () {

		// Animate
		switch (simulationMovementType) {
		case WandSimulationMovementType.kWandSimulationMovementConst:
			{
				currentPoint.rotation = prevPoint.rotation;
				//currentPoint.speed = prevPoint.speed;
				currentPoint.velocity = prevPoint.velocity;
				currentPoint.position = prevPoint.position + currentPoint.velocity.normalized * currentPoint.speed * Time.smoothDeltaTime;
				transform.position = currentPoint.position;
				break;
			}
		case WandSimulationMovementType.kWandSimulationMovementSpiral:
			{
				currentPoint.position = animationTarget + new Vector3 (
					animationDistanceFromTarget * Mathf.Sin (-Time.time * animationSpeed), 
					prevPoint.position.y + animationSpeed * 0.025f * Time.smoothDeltaTime, 
					animationDistanceFromTarget * Mathf.Cos (Time.time * animationSpeed)
				);
				transform.position = currentPoint.position;
				transform.LookAt(animationTarget);
				currentPoint.rotation = Quaternion.LookRotation (transform.rotation * Vector3.right, transform.rotation * Vector3.up);
				currentPoint.velocity = (currentPoint.position - prevPoint.position) / Time.deltaTime;
				//currentPoint.speed = currentPoint.velocity.magnitude;
				break;
			}
		case WandSimulationMovementType.kWandSimulationMovementRandomWalk:
			{
				float speed = Mathf.Clamp( currentPoint.speed + (Random.value - 0.5f) * 0.5f, 0.1f, 10f);
				if (currentPoint.velocity.sqrMagnitude == 0) {
					currentPoint.velocity = Vector3.forward;
				}
				currentPoint.velocity = currentPoint.velocity.normalized * speed;
				Vector3 directionDeviation = (Vector3.forward * 2f + Random.insideUnitSphere).normalized;
				Vector3 newDirection = Quaternion.FromToRotation (Vector3.forward, directionDeviation) * currentPoint.velocity;
				currentPoint.velocity = newDirection.normalized * currentPoint.speed;
				currentPoint.position += currentPoint.velocity * Time.smoothDeltaTime;
				currentPoint.rotation = Quaternion.SlerpUnclamped(currentPoint.rotation, Random.rotation, 0.05f);
				transform.position = currentPoint.position;
				transform.rotation = currentPoint.rotation;
				break;
			}
		}
		prevPoint = new OrientedMovingPoint(currentPoint);

		if (drawButtonDown) {
			if (Time.time - drawButtonTime > secondsDrawDown) {
				OnDrawButton (false);
			}
		} else {
			if (Time.time - drawButtonTime > secondsDrawUp) {
				OnDrawButton (true);
			}
		}

		if (secondsUndo > 0f && Time.time - undoButtonTime > secondsUndo) {
			OnUndoButton (true);
		}
		if (secondsRedo > 0f && Time.time - redoButtonTime > secondsRedo) {
			OnRedoButton (true);
		}

        // 
        BezierSpline spline = (sceneManager == null) ? null : sceneManager.GetCurrentSpline();
		if (spline != null && drawButtonDown) {
			if ((currentPoint.position - prevSamplePoint.position).sqrMagnitude > _minSampleDistanceSquared) {

				if (spline.VertexCount >= sceneManager.vertexLimitForSpline) {
					BezierSpline prevSpline = spline;
					spline = sceneManager.CreateContinuingSpline ();
					if (spline == null) {
						Debug.LogError ("Failed to create continuing spline.");
					}/* else if (udpSender != null) {
						BezierPoint lastPoint = prevSpline.GetLastPoint ();
						OrientedMovingPoint lastPointGlobal = new OrientedMovingPoint(
							prevSpline.transform.TransformPoint(lastPoint.position),
							prevSpline.transform.rotation * lastPoint.rotation,
							prevSpline.transform.rotation * lastPoint.velocity,
							lastPoint.size
						);
						udpSender.EnqueueAddPoint (spline, lastPointGlobal, Time.time);
					}*/
				}
                sceneManager.AddPointToCurrentSpline(currentPoint);

				prevSamplePoint = new OrientedMovingPoint( currentPoint);
			} else {
				// Keep moving last point at wand position
				if (spline.PointCount == 1) {
                    sceneManager.AddPointToCurrentSpline(currentPoint);
				} else {
                    sceneManager.SetLastPointOfCurrentSpline(currentPoint);
				}
			}
		}
	}
}
