using UnityEngine;

public abstract class RecordableObject : MonoBehaviour {
	
	protected int state;
	public void SetState(int newState, bool value) {
		state = state.SetBit(newState, value);
	}
	protected virtual void ApplyState(int state) {
		this.state = state;
		Debug.LogWarning(name + " does not have ApplyState() implemented.");
	}

	public void SetCurrentState(EntityState newState) {
		ApplyState(newState.state);
		transform.position = newState.position;
		transform.rotation = newState.rotation;
		gameObject.SetActive(newState.active);
	}

}