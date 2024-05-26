using UnityEngine;

public class UniqueMesh : MonoBehaviour {
	public int ownerID; // To ensure they have a unique mesh
	MeshFilter _mf;
	MeshFilter mf { // Tries to find a mesh filter, adds one if it doesn't exist yet
		get{
			if (_mf == null) {
				_mf = gameObject.GetComponent<MeshFilter> ();
			}
			if (_mf == null) {
				_mf = gameObject.AddComponent<MeshFilter> ();
				//_mf.name = "MeshFilter [UM:" + Time.time + "]";
			}
			return _mf;
		}
	}
	Mesh _mesh;
	protected Mesh mesh { // The mesh to edit
		get{
			bool isOwner = ownerID == gameObject.GetInstanceID();
			if( mf.sharedMesh == null || !isOwner ){
				mf.sharedMesh = _mesh = new Mesh();
				ownerID = gameObject.GetInstanceID();
				_mesh.name = "Mesh [" + ownerID + "]";
			}
			//Debug.Log ("Mesh Filter name: " + _mf.name);
			//Debug.Log ("Mesh name: " + _mesh.name);
			return _mesh;
		}
	}

	// cj
	protected void ForceNewMesh() {
		ownerID = 0;
		//if (_mf != null && _mf.sharedMesh != null) {
		//	DestroyImmediate (_mf.sharedMesh);
		//	_mf.sharedMesh = null;
		//}
		//_mesh = null;
	}
}