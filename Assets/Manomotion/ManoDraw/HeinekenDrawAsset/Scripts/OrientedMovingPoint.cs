using UnityEngine;
using System;

[Serializable]
public class OrientedMovingPoint {

	[SerializeField]
	protected Vector3 _position = Vector3.zero;
	public Vector3 position {
		get {
			return _position;
		}
		set {
			_position = value;
		}
	}

	public Quaternion rotation = Quaternion.identity;

	[SerializeField]
	protected Vector3 _velocity = Vector3.zero; // OBS. Not related to rotation!
	public Vector3 velocity {
		get {
			return _velocity;
		}
		set {
			_velocity = value;
			_speed = velocity.magnitude;
		}
	}

	protected float _speed = -1f;
	public float speed {
		get {
			if (_speed < 0f) {
				_speed = velocity.magnitude;
			}
			return _speed;
		}
	}

	public Vector3 size = Vector3.zero;

	public OrientedMovingPoint() {
	}

	public OrientedMovingPoint(OrientedMovingPoint other) {
		this._position = other.position;
		this.rotation = other.rotation;
		this._velocity = other.velocity;
		this._speed = -1f;
		this.size = other.size;
	}

	public OrientedMovingPoint( Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 size ) {
		this._position = position;
		this.rotation = rotation;
		this._velocity = velocity;
		this.size = size;
	}

	public Vector3 LocalToWorld( Vector3 point ) {
		return position + rotation * point;
	}

	public Vector3 WorldToLocal( Vector3 point ) {
		return Quaternion.Inverse( rotation ) * ( point - position );
	}

	public Vector3 LocalToWorldDirection( Vector3 dir ) {
		return rotation * dir;
	}
}

