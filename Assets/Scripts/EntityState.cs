using UnityEngine;

public struct EntityState {

	public int state;
	public Vector3 position;
	public Quaternion rotation;

	public EntityState(Vector3 position, Quaternion rotation, int state) {
		this.position = position;
		this.rotation = rotation;
		this.state = state;
	}
	
    public static bool operator == (EntityState a, EntityState b)
    {
        return a.position == b.position && a.rotation == b.rotation && a.state == b.state;
    }

    public static bool operator != (EntityState a, EntityState b)
    {
        return !(a == b);
    }
}
