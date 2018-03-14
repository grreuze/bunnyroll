using UnityEngine;

public struct EntityState {

	public Vector3 position;
	public Quaternion rotation;

	public EntityState(Vector3 position, Quaternion rotation) {
		this.position = position;
		this.rotation = rotation;
	}
	
}
