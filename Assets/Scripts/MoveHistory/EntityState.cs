using UnityEngine;

public class EntityState : RecordableMove {

	public bool active;
	public Vector3 position;
	public Quaternion rotation;
	public int state;

	public EntityState(RecordableObject reference, bool active, Vector3 position, Quaternion rotation, int state) : base(reference, eType.EntityState) {
		this.active = active;
		this.position = position;
		this.rotation = rotation;
		this.state = state;
	}
	

	public void SetAsCurrent() {
		if (!reference.gameObject.activeSelf)
			reference.gameObject.SetActive(true);

		reference.SetCurrentState(this);
	}
	
	public static bool operator == (EntityState a, EntityState b)
    {
        return a.active == b.active && a.position == b.position && a.rotation == b.rotation && a.state == b.state;
    }

    public static bool operator != (EntityState a, EntityState b)
    {
        return !(a == b);
    }
}
