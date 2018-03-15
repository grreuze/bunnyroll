using UnityEngine;

public class Carrot : MovableEntity {

	[SerializeField] GameObject partA, partB;

	#region State

	const int EATEN_A = 0, EATEN_B = 1;

	bool EatenA {
		get	{
			return state.GetBit(EATEN_A);
		}
		set	{
			SetState(EATEN_A, value);
		}
	}

	bool EatenB {
		get	{
			return state.GetBit(EATEN_B);
		}
		set	{
			SetState(EATEN_B, value);
		}
	}
	
	public override bool FullyEaten
	{
		get { return EatenA && EatenB; }
	}

	protected override void ApplyState(int state) {
		this.state = state;
		bool a = EatenA, b = EatenB;
		my.parent = null;

		partA.SetActive(!a);
		partB.SetActive(!b);
		GetComponent<Collider>().enabled = !(a && b);
	}
	#endregion

	#region Movement

	public override bool CanMove(Vector3 direction) {
		Vector3 pos = my.position;
		RaycastHit hit;
		if (Physics.Raycast(pos, direction, out hit, 1, layerMask)) {
			MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
			if (!(movable && movable.CanMove(direction)))
				return false;
			movable.Push(direction);
		}
		if (Physics.Raycast(pos + my.forward, direction, out hit, 1, layerMask)) {
			MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
			if (!(movable && movable.CanMove(direction)))
				return false;
			movable.Push(direction);
		}
		return true;
	}

	protected override bool EndMove(Vector3 direction) {
		Vector3 pos = my.position;
		if (!Physics.Raycast(pos, -yAxis, 1, layerMask) && !Physics.Raycast(pos + my.forward, -yAxis, 1, layerMask)) {
			// there's a hole
			ChangePosition(pos, RoundPosition(pos - yAxis), timeToFall, -yAxis); // fall
			return false;
		}
		return true;
	}

	public override void Push(Vector3 direction) {
		// todo : si je suis à la verticale et qu'on me pousse depuis le haut, je tombe
		if (!Colinear(direction, my.forward) && !Colinear(my.forward, yAxis)) {
			Vector3 axis = new Vector3(direction.z, 0, -direction.x);
			Quaternion initialRotation = my.rotation;
			Quaternion finalRotation = Quaternion.AngleAxis(90, axis) * initialRotation;
			finalRotation = RoundRotation(finalRotation);
			ChangeRotation(initialRotation, finalRotation, timeToMove, direction);
		}

		base.Push(direction);
	}

	#endregion

	#region Eating

	public override bool CanBeEaten(Vector3 direction) {
		return Colinear(direction, my.forward);
	}

	public override void Eat(Vector3 position) {
		Vector3 myPos = RoundPosition(my.position);
		position = RoundPosition(position);

		if (position == myPos) {
			EatenA = true;
			partA.SetActive(false);
		} else if (position == RoundPosition(myPos + my.forward)) {
			EatenB = true;
			partB.SetActive(false);
		}

		if (FullyEaten) {
			GetComponent<Collider>().enabled = false;
		}
	}
	#endregion
}
