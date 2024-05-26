using UnityEngine;
using System;

public enum ExtrudeShapeType {
	kExtrudeShapeNone = 0,
	kExtrudeShapeRibbon_OneSided,
    kExtrudeShapeRibbon_TwoSided,
    kExtrudeShapeSquare_SoftEdge,
	kExtrudeShapeSquare_HardEdge,
	kExtrudeShapeCircle_SoftEdge,
    kExtrudeShapeCircle_HardEdge,
    kExtrudeShapePolygonWithBevel_HardEdge,
    kExtrudeShapeEllipse
};

[Serializable]
public class ExtrudeShapeSettings {
	public ExtrudeShapeSettings() {}
	public ExtrudeShapeSettings(ExtrudeShapeSettings other) {
		type = other.type;
		scale = other.scale;
		segments = other.segments;
        customParameter01 = other.customParameter01;
        separateVerticesAlongCurve = other.separateVerticesAlongCurve;
		//drawEndCaps = other.drawEndCaps;
		beginWithZeroWidth = other.beginWithZeroWidth;
		endWithZeroWidth = other.endWithZeroWidth;
	}
	public ExtrudeShapeType type = ExtrudeShapeType.kExtrudeShapeNone;
	public float scale = 1f;
	public int segments = 16;
    public float customParameter01 = 0f;
    public bool separateVerticesAlongCurve = false;
	//public bool drawEndCaps = false;
	public bool beginWithZeroWidth = true; // only false when continuing spline
	public bool endWithZeroWidth = true; // only false when there's a continuing spline after
}

public class ExtrudeShape {

	public static ExtrudeShape CreateFromSettings (ExtrudeShapeSettings settings) {
		ExtrudeShape shape = null;

		switch (settings.type) {
		case ExtrudeShapeType.kExtrudeShapeRibbon_OneSided:
			shape = ExtrudeShape.Ribbon_OneSided (settings.scale);
			break;
        case ExtrudeShapeType.kExtrudeShapeRibbon_TwoSided:
            shape = ExtrudeShape.Ribbon_TwoSided(settings.scale);
            break;
        case ExtrudeShapeType.kExtrudeShapeSquare_SoftEdge:
			shape = ExtrudeShape.Square_SoftEdge (settings.scale);
			break;
		case ExtrudeShapeType.kExtrudeShapeSquare_HardEdge:
			shape = ExtrudeShape.Square_HardEdge (settings.scale);
			break;
		case ExtrudeShapeType.kExtrudeShapeCircle_SoftEdge:
			shape = ExtrudeShape.Circle_SoftEdge (settings.segments, settings.scale);
			break;
        case ExtrudeShapeType.kExtrudeShapeCircle_HardEdge:
            shape = ExtrudeShape.Circle_HardEdge(settings.segments, settings.scale);
            break;
        case ExtrudeShapeType.kExtrudeShapePolygonWithBevel_HardEdge:
            shape = ExtrudeShape.PolygonWithBevel_HardEdge(settings.segments, settings.scale, settings.customParameter01);
            break;
            case ExtrudeShapeType.kExtrudeShapeEllipse:
			shape = ExtrudeShape.Ellipse (settings.scale);
			break;
		case ExtrudeShapeType.kExtrudeShapeNone:
		default:
			shape = null;
			break;
		}

		return shape;
	}

	// edge loop
	public ExtrudeVertex[] vertices;
	public int[] lines;
	public float uSpan;


	// end cap
	public ExtrudeVertex[] endCapVertices;
	public int[] endCapTriangels;


	// stipple
	public float stipplePeriodLength = -1f;
	public float stippleEmptyPercent = 0.5f;


	// z = forward
	// y = up
	// x = right


	// Default shapes creators
	public static ExtrudeShape Ribbon_OneSided(float scale = 1f) {
		ExtrudeShape shape = new ExtrudeShape ();

		// edge loop
		shape.vertices = new ExtrudeVertex[] {
			new ExtrudeVertex (-0.5f, 0f, 0f, 1f, 0f, scale),
			new ExtrudeVertex (0.5f, 0f, 0f, 1f, 1f, scale)
		};
		shape.lines = new int[] { // third number flips triangle
			0, 1, 0
		};
		shape.uSpan = scale * 1f;

		// end cap
		shape.endCapVertices = new ExtrudeVertex[] { };
		shape.endCapTriangels = new int[] { };

		return shape;
	}
    
    public static ExtrudeShape Ribbon_TwoSided(float scale = 1f)
    {
        ExtrudeShape shape = new ExtrudeShape();

        // edge loop
        shape.vertices = new ExtrudeVertex[] {
            new ExtrudeVertex (-0.5f, 0f, 0f, 1f, 0f, scale),
            new ExtrudeVertex (0.5f, 0f, 0f, 1f, 1f, scale),
            new ExtrudeVertex (0.5f, 0f, 0f, -1f, 1f, scale),
            new ExtrudeVertex (-0.5f, 0f, 0f, -1f, 0f, scale)
        };
        shape.lines = new int[] { // third number flips triangle
            0, 1, 0,
            2, 3, 1
        };
        shape.uSpan = scale * 1f;

        // end cap
        shape.endCapVertices = new ExtrudeVertex[] { };
        shape.endCapTriangels = new int[] { };

        return shape;
    }

    public static ExtrudeShape Square_SoftEdge(float scale = 1f) {
		ExtrudeShape shape = new ExtrudeShape ();

		// edge loop
		shape.vertices = new ExtrudeVertex[] {
			new ExtrudeVertex (-0.5f, 0.5f, -1f, 1f, 0f, scale),
			new ExtrudeVertex (0.5f, 0.5f, 1f, 1f, 0.25f, scale),
			new ExtrudeVertex (0.5f, -0.5f, 1f, -1f, 0.5f, scale),
			new ExtrudeVertex (-0.5f, -0.5f, -1f, -1f, 0.75f, scale)
			//new ExtrudeVertex (-0.5f, 0.5f, -1f, 1f, 1f, scale)
		};
		shape.lines = new int[] { // third number flips triangle
			0, 1, 0,
			1, 2, 0,
			2, 3, 0,
			3, 0, 0
		};
		shape.uSpan = scale * 1f;

		// end cap
		shape.endCapVertices = new ExtrudeVertex[] { };
		shape.endCapTriangels = new int[] { };

		return shape;
	}

	public static ExtrudeShape Square_HardEdge(float scale = 1f) {
		ExtrudeShape shape = new ExtrudeShape ();

		// edge loop
		shape.vertices = new ExtrudeVertex[] {
			new ExtrudeVertex (-0.5f, 0.5f, 0f, 1f, 0f, scale),
			new ExtrudeVertex (0.5f, 0.5f, 0f, 1f, 1f, scale),
			new ExtrudeVertex (0.5f, 0.5f, 1f, 0f, 0f, scale),
			new ExtrudeVertex (0.5f, -0.5f, 1f, 0f, 1f, scale),
			new ExtrudeVertex (0.5f, -0.5f, 0f, -1f, 0f, scale),
			new ExtrudeVertex (-0.5f, -0.5f, 0f, -1f, 1f, scale),
			new ExtrudeVertex (-0.5f, -0.5f, -1f, 0f, 0f, scale),
			new ExtrudeVertex (-0.5f, 0.5f, -1f, 0f, 1f, scale)
		};
		shape.lines = new int[] { // third number flips triangle
			0, 1, 0,
			2, 3, 0,
			4, 5, 0,
			6, 7, 0
		};
		shape.uSpan = scale * 1f;

		// end cap
		shape.endCapVertices = new ExtrudeVertex[] {
			//new ExtrudeVertex(0.5f, 0.5f, 0f, 0f, 0f, 1f, 0f, 0f, scale),
			//new ExtrudeVertex(-0.5f, 0.5f, 0f, 0f, 0f, 1f, 1f, 0f, scale),
			//new ExtrudeVertex(-0.5f, -0.5f, 0f, 0f, 0f, 1f, 1f, 1f, scale),
			//new ExtrudeVertex(0.5f, -0.5f, 0f, 0f, 0f, 1f, 0f, 1f, scale)
		};
		shape.endCapTriangels = new int[] {
			//0, 1, 2,
			//3, 0, 2
		};

		return shape;
	}

	public static ExtrudeShape Ellipse(float scale = 1f) {
		ExtrudeShape shape = new ExtrudeShape ();

		// edge loop
		scale = scale / 6f;
		float la = (new Vector2 (-3f, 0f) - new Vector2 (-2f, 1f)).magnitude * scale;
		float lb = (new Vector2 (-2f, 1f) - new Vector2 (0f, 1.5f)).magnitude * scale;
		float ltot = la + lb + lb + la;
		shape.vertices = new ExtrudeVertex[] {
			new ExtrudeVertex (-3f, 0f, -1f, 0f, 0f, scale),
			new ExtrudeVertex (-2f, 1f, -1f, 3f, (la)/ltot, scale),
			new ExtrudeVertex (0f, 1.5f, 0f, 1f, (la+lb)/ltot, scale),
			new ExtrudeVertex (2f, 1f, 1f, 3f, (la+lb+lb)/ltot, scale),
			new ExtrudeVertex (3f, 0f, 1f, 0f, 1f, scale),
			new ExtrudeVertex (2f, -1f, 1f, -3f, (la+lb+lb)/ltot, scale),
			new ExtrudeVertex (0f, -1.5f, 0f, -1f, (la+lb)/ltot, scale),
			new ExtrudeVertex (-2f, -1f, -1f, -3f, (la)/ltot, scale),
		};
		shape.lines = new int[] { // third number flips triangle
			0, 1, 0,
			1, 2, 0,
            2, 3, 0,
            3, 4, 0,
            4, 5, 0,
            5, 6, 0,
            6, 7, 0,
            7, 0, 0
		};
		shape.uSpan = ltot;

		// end cap
		shape.endCapVertices = new ExtrudeVertex[] { };
		shape.endCapTriangels = new int[] { };

		return shape;
	}


	public static ExtrudeShape Circle_SoftEdge(int segments, float scale) {
		if (segments <= 1) {
			return null;
		}
		ExtrudeShape shape = new ExtrudeShape ();

		float radiansPerSegment = 2f * Mathf.PI / (float)segments;
		shape.vertices = new ExtrudeVertex[segments];
		shape.lines = new int[segments*3];
		float angle = 0;
		shape.uSpan = 0f;
		Vector2 p, n, prevP = Vector2.zero, firstP = Vector2.zero;
		for (int i = 0; i < segments; ++i) {
			p = new Vector2 (Mathf.Cos (angle), Mathf.Sin (angle)) * scale * 0.5f;
			n = p.normalized;
			float u = i / (float)segments;
			shape.vertices [i] = new ExtrudeVertex (p.x, p.y, n.x, n.y, u, 1f);
			shape.lines [i * 3 + 0] = i;
			shape.lines [i * 3 + 1] = (i+1) % segments;
            shape.lines [i * 3 + 2] = i % 2;
            //Debug.Log ("v[" + i + "] = " + p + " | line = " + i + " to " + ((i + 1) % segments));
            if (i > 0) {
				shape.uSpan += (p - prevP).magnitude;
			} else {
				firstP = p;
			}
			prevP = p;
			angle -= radiansPerSegment;
		}
		shape.uSpan += (firstP - prevP).magnitude; // uSpan relativ till omkrets
		//shape.uSpan *= 0.5f;
		//Debug.Log ("uSpan = " + shape.uSpan);

		// end cap
		shape.endCapVertices = new ExtrudeVertex[] { };
		shape.endCapTriangels = new int[] { };

		return shape;
    }


    public static ExtrudeShape Circle_HardEdge(int segments, float scale)
    {
        if (segments <= 1)
        {
            return null;
        }
        ExtrudeShape shape = new ExtrudeShape();

        float radiansPerSegment = 2f * Mathf.PI / (float)segments;
        shape.vertices = new ExtrudeVertex[segments * 2];
        shape.lines = new int[segments * 3];
        float angle = 0;
        shape.uSpan = 0f;
        Vector2 p0, p1, n;
        float u0, u1 = 0f;
        p1 = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * scale * 0.5f;
        for (int i = 0; i < segments; ++i)
        {
            angle -= radiansPerSegment;
            p0 = p1;
            p1 = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * scale * 0.5f;
            //n = p.normalized;
            n = ((p0 + p1) * 0.5f).normalized;
            u0 = u1;
            u1 = (i+1) / (float)segments;
            shape.vertices[i*2 + 0] = new ExtrudeVertex(p0.x, p0.y, n.x, n.y, u0, 1f);
            shape.vertices[i*2 + 1] = new ExtrudeVertex(p1.x, p1.y, n.x, n.y, u1, 1f);
            shape.lines[i * 3 + 0] = i*2;
            shape.lines[i * 3 + 1] = i*2 + 1;
            shape.lines[i * 3 + 2] = 0;
            //Debug.Log ("v[" + i + "] = " + p + " | line = " + i + " to " + ((i + 1) % segments));
            shape.uSpan += (p1 - p0).magnitude;
            //angle -= radiansPerSegment;
        }

        // end cap
        shape.endCapVertices = new ExtrudeVertex[] { };
        shape.endCapTriangels = new int[] { };

        return shape;
    }

    public static ExtrudeShape PolygonWithBevel_HardEdge(int segments, float scale, float bevelAnglePct)
    {
        if (segments <= 1)
        {
            return null;
        }
        ExtrudeShape shape = new ExtrudeShape();

        bevelAnglePct = Mathf.Clamp01(bevelAnglePct);
        float radiansPerSegment = 2f * Mathf.PI / (float)segments;
        float radiansPerMain = (1f - bevelAnglePct) * radiansPerSegment;
        float radiansPerBevel = bevelAnglePct * radiansPerSegment;
        shape.vertices = new ExtrudeVertex[segments * 4];
        shape.lines = new int[segments * 6];
        float angle = radiansPerBevel * 0.5f; // to get bevel up, or whatever
        shape.uSpan = 0f;
        Vector2 p0, p1, n;
        float u0, u1 = 0f;
        p1 = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * scale * 0.5f;
        for (int i = 0; i < segments*2; ++i)
        {
            angle -= (i % 2) == 0 ? radiansPerBevel : radiansPerMain;
            p0 = p1;
            p1 = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * scale * 0.5f;
            //n = p.normalized;
            n = ((p0 + p1) * 0.5f).normalized;
            u0 = u1;
            u1 = (i + 1) / (float)segments;
            shape.vertices[i * 2 + 0] = new ExtrudeVertex(p0.x, p0.y, n.x, n.y, u0, 1f);
            shape.vertices[i * 2 + 1] = new ExtrudeVertex(p1.x, p1.y, n.x, n.y, u1, 1f);
            shape.lines[i * 3 + 0] = i * 2;
            shape.lines[i * 3 + 1] = i * 2 + 1;
            shape.lines[i * 3 + 2] = 0;
            //Debug.Log ("v[" + i + "] = " + p + " | line = " + i + " to " + ((i + 1) % segments));
            shape.uSpan += (p1 - p0).magnitude;
            //angle -= radiansPerSegment;
        }

        // end cap
        shape.endCapVertices = new ExtrudeVertex[] { };
        shape.endCapTriangels = new int[] { };

        return shape;
    }
}
