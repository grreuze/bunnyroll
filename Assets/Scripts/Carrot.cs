using UnityEngine;

public class Carrot : MovableEntity {

	[SerializeField] GameObject partA, partB;
    public Level level;

	#region State

	const int EATEN_A = 0, EATEN_B = 1, HALFEATEN = 2, FROZEN = 3;

	bool EatenA {
		get	{ return state.GetBit(EATEN_A);	}
		set	{ SetState(EATEN_A, value);	}
	}

	bool EatenB {
		get	{ return state.GetBit(EATEN_B);	}
		set	{ SetState(EATEN_B, value);	}
	}

	bool HalfEaten {
		get	{ return state.GetBit(HALFEATEN); }
		set { SetState(HALFEATEN, value); }
	}

	public override bool FullyEaten {
		get { return EatenA && EatenB; }
	}

    bool Frozen {
        get { return state.GetBit(FROZEN); }
        set { SetState(FROZEN, value); }
    }

	protected override void ApplyState(int state) {
		this.state = state;
		bool a = EatenA, b = EatenB;
		my.parent = null;

		partA.SetActive(!a);
		partB.SetActive(!b);
		UpdateCollider();
	}
	#endregion

	#region Movement

	public override bool CanMove(Vector3 direction) {
        if (Frozen) return false;

		Vector3 pos = my.position;
		RaycastHit hit;
		if (!EatenA && Physics.Raycast(pos, direction, out hit, 1, layerMask)) {
			MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
			if (!(movable && movable.CanMove(direction)))
				return false;
			movable.Push(direction);
		}
		if (!EatenB && Physics.Raycast(pos + my.forward, direction, out hit, 1, layerMask)) {
			MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
			if (!(movable && movable.CanMove(direction)))
				return false;
			movable.Push(direction);
		}
		return true;
	}

	public override bool EndMove(Vector3 direction) {
		Vector3 pos = my.position;
        RaycastHit hitUnder, hitForward;
        MovableEntity carrier = null;

        bool somethingUnder = Physics.Raycast(pos, -yAxis, out hitUnder, 1, layerMask);
        bool underForward = Physics.Raycast(pos + my.forward, - yAxis, out hitForward, 1, layerMask);

        if (!somethingUnder && !underForward) {
            // there's a hole
            CarriedBy(null);
            ChangePosition(pos, RoundPosition(pos - yAxis), timeToFall, -yAxis); // fall
            return false;
        }
        else if (somethingUnder && !underForward)
            carrier = hitUnder.transform.GetComponent<MovableEntity>();
        else if (underForward && !somethingUnder)
            carrier = hitForward.transform.GetComponent<MovableEntity>();

        CarriedBy(carrier);
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
		return !Frozen && Colinear(direction, my.forward);
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
            col.enabled = false;
            level?.IsComplete();
        }
	}

	public override void StopEating() {

		if (FullyEaten)
			return;

		HalfEaten = true;
		UpdateCollider();

        if (carrying)
            carrying.CarriedBy(null);
	}

	void UpdateCollider() {

		bool a = EatenA, b = EatenB;

		if (HalfEaten) {
			col.size = new Vector3(1, 1, 1);
			if (a)
				col.center = zAxis;
			else
				col.center *= 0;
		} else {
			col.center = zAxis / 2;
			col.size = new Vector3(1, 1, 2);
		}

		col.enabled = !(a && b);
	}


	#endregion
}
