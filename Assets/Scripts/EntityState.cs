using UnityEngine;

public class EntityState : RecordableMove {

	public RecordableObject reference;
	public bool active;
	public Vector3 position;
	public Quaternion rotation;
	public int state;

	public EntityState(RecordableObject reference, bool active, Vector3 position, Quaternion rotation, int state) {
        type = eType.EntityState;
		this.reference = reference;
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
	

	public static EntityState EnterLevel(Level level) {
		return new EntityState(level, true, new Vector3(12, 24.3f, 645.68f), Quaternion.identity, -1);
	}
	public static EntityState ExitLevel(Level level) {
		return new EntityState(level, true, new Vector3(17.54f, 24.3f, 425.4f), Quaternion.identity, -2);
	}
	public bool IsEnterLevel() {
		return state == -1 && position == new Vector3(12, 24.3f, 645.68f);
	}
	public bool IsExitLevel() {
		return state == -2 && position == new Vector3(17.54f, 24.3f, 425.4f);
	}

	public static EntityState StopEating(MovableEntity carrot) {
		return new EntityState(carrot, true, new Vector3(45.21f, 101.2f, 87.7f), Quaternion.identity, -3);
	}
	public static EntityState StartEating(MovableEntity carrot) {
		return new EntityState(carrot, true, new Vector3(685.13f, 453.12f, 4.583f), Quaternion.identity, -4);
	}
	public bool IsStopEating() {
		return state == -3 && position == new Vector3(45.21f, 101.2f, 87.7f);
	}
	public bool IsStartEating() {
		return state == -4 && position == new Vector3(685.13f, 453.12f, 4.583f);
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
