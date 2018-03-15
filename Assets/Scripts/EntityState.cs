using UnityEngine;

public struct EntityState {

	public Vector3 position;
	public Quaternion rotation;

	public EntityState(Vector3 position, Quaternion rotation) {
		this.position = position;
		this.rotation = rotation;
	}
	
    public static bool operator == (EntityState a, EntityState b)
    {
        return a.position == b.position && a.rotation == b.rotation;
    }

    public static bool operator != (EntityState a, EntityState b)
    {
        return !(a.position == b.position && a.rotation == b.rotation);
    }
}
