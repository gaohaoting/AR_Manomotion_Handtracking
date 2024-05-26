using UnityEngine;

public class ExtrudeVertex {
	public Vector3 position;
	public Vector3 normal;
	public Vector2 uv;


	public ExtrudeVertex() : base() {}
	public ExtrudeVertex(Vector3 position, Vector3 normal, Vector2 uv) : this() {
		this.position = position;
		this.normal = normal.normalized;
		this.uv = uv;
	}
	public ExtrudeVertex(float px, float py, float pz, float nx, float ny, float nz, float u, float v, float scale = 1f) : this() {
		position = new Vector3 (px, py, pz) * scale;
		normal = new Vector3 (nx, ny, nz).normalized;
		uv = new Vector2(u, v);
	}
	public ExtrudeVertex(float px, float py, float nx, float ny, float u, float scale = 1f) : this() {
		position = new Vector3 (px, py, 0f) * scale;
		normal = new Vector3 (nx, ny, 0f).normalized;
		uv = new Vector2(u, 0f);
	}


}
