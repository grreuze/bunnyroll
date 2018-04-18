using UnityEngine;

public class BunnyController : MovableEntity {
	
	Vector3 input;

	public MovableEntity currentlyEating;
    float lastInput;

	#region State

	// States
	const int ON_HEAD = 0, ON_EARS = 1, SUSPENDED = 2, EATING = 4, JUSTATE = 5;

	bool OnHead	{
		get	{
			return state.GetBit(ON_HEAD);
		}
		set	{
			SetState(ON_HEAD, value);
		}
	}

	bool OnEars	{
		get	{
			return state.GetBit(ON_EARS);
		}
		set	{
			SetState(ON_EARS, value);
		}
	}

	bool Suspended {
		get	{
			return state.GetBit(SUSPENDED);
		}
		set	{
			SetState(SUSPENDED, value);
		}
	}

	bool Eating {
		get	{
			return state.GetBit(EATING);
		}
		set	{
			SetState(EATING, value);
		}
	}

	bool JustAte {
		get	{
			return state.GetBit(JUSTATE);
		}
		set	{
			SetState(JUSTATE, value);
		}
	}


	protected override void ApplyState(int state) {
		StopCurrentMovement();

		this.state = state;

		RaycastHit hit;
        // this does not work if we're eating a carrot downward (or a few other cases)
		if (Eating && Physics.Raycast(my.position + my.up + my.forward, -my.up, out hit, 1, layerMask)) {
			MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
			if (!movable) {
				Debug.LogWarning("Eating not found");
				return;
			}
			currentlyEating = movable;
			movable.transform.parent = my;
		}
	}


	#endregion

	#region MonoBehaviour

	void Update () {
		if (globalActions == 0) {
			input.x = Round(Input.GetAxis("Horizontal"));
			input.z = Round(Input.GetAxis("Vertical"));
            
            if (input.sqrMagnitude == 1 && Time.time > lastInput + timeBetweenMoves) {
				AddMove();
				DetermineMovement();

                if (Physics.Raycast(my.position + input * 2, -yAxis, 1, layerMask))
                    lastInput = Time.time;
                else
                    lastInput = Time.time + timeBeforeFallInput;
            }
            else if (input.sqrMagnitude == 0)
                lastInput = 0;
		}
	}

	private void Start() {
		GameplayManager.player = this;
	}
	#endregion

	#region Movement

	public override void ChangePosition(Vector3 startPos, Vector3 endPos, float duration, Vector3 direction) {

        if (carrying && carrying.transform.parent != my)
            carrying.Push(yAxis);
        else if (currentlyEating && currentlyEating.carrying && currentlyEating.carrying.transform.parent != my)
            currentlyEating.carrying.Push(yAxis);

        base.ChangePosition(startPos, endPos, duration, direction);
    }

    public override void ChangeRotation(Quaternion startRot, Quaternion endRot, float duration, Vector3 direction) {
        
        if (carrying && carrying.transform.parent != my)
            carrying.Push(yAxis);
        else if (currentlyEating && currentlyEating.carrying && currentlyEating.carrying.transform.parent != my)
            currentlyEating.carrying.Push(yAxis);

        base.ChangeRotation(startRot, endRot, duration, direction);
    }


    void DetermineMovement() {
		if (Suspended) {
			if (Colinear(my.forward, input))
				ExecuteMove(input);
			return;
		}

		if (StandingUp()) {
			if (Colinear(my.forward, input))
				ExecuteMove(input);
			else
				ExecuteRotation(input);
		} else
			ExecuteMove(input);
	}


	void ExecuteRotation(Vector3 direction) {

		Quaternion initialRotation = my.rotation;
		Quaternion finalRotation = Quaternion.FromToRotation(my.forward, direction) * initialRotation;
		finalRotation = RoundRotation(finalRotation);

		if (Eating) {
			Vector3 initialPosition = RoundPosition(my.position + my.forward);
			Vector3 intermediatePosition = RoundPosition(initialPosition + direction);
			Vector3 finalPosition = RoundPosition(my.position + direction);

			RaycastHit hit;
			if (Physics.Linecast(initialPosition, intermediatePosition, out hit, layerMask)) {
				if (!HandleCollision(hit.transform, direction))
					return; // there's a wall blocking us
			}
			if (Physics.Linecast(intermediatePosition, finalPosition, out hit, layerMask)) {
				if (!HandleCollision(hit.transform, -my.forward))
					return; // there's a wall blocking us
			}
			currentlyEating.transform.parent = my;
		}
		ChangeRotation(initialRotation, finalRotation, timeToRotate, direction);
	}
	
	void ExecuteMove(Vector3 direction) {

		Vector3 initialPosition = my.position;
		Vector3 finalPosition = RoundPosition(initialPosition + direction);

		JustAte = false;
		bool readyToEat = Eating && direction == my.forward;

		if (Suspended && readyToEat) {
			JustAte = true;
			currentlyEating.Eat(finalPosition);

		} else {
			RaycastHit hit;
						
			if (Physics.Linecast(initialPosition, readyToEat ? finalPosition + direction : finalPosition, out hit, layerMask)) {

				if (!HandleCollision(hit.transform, direction)) {
					// if stuck on your head, your ears push you up
					if (ShouldRiseOnEars()) {

                        if (Physics.Raycast(my.position, yAxis, out hit, 1, layerMask)) {

                            MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
                            print("movable above: " + movable);
                            if (movable && movable.CanMove(yAxis))
                                movable.Push(yAxis); // should check if not eating first (carrot could be stuck under ceiling)
                            else return;

                        } else if (Eating && Physics.Raycast(my.position + my.forward, yAxis, out hit, 1, layerMask)) {

                            MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
                            if (movable && movable.CanMove(yAxis))
                                movable.Push(yAxis);
                            else return;
                        }
                        
                        OnEars = true;
						OnHead = false;
						ChangePosition(my.position, my.position + yAxis, timeToFall, yAxis);

                    }
					if (readyToEat) {
						JustAte = true;
						currentlyEating.Eat(finalPosition);
					} else
						return; // there's a wall blocking us
				}
			} else if (Eating && !StandingUp()) {
				if (SameDirection(direction, my.forward) && Physics.Raycast(initialPosition + my.forward, -yAxis, 1, layerMask)) {
					// check if movable, if movable, push it
					return; // tu peux pas tourner t'as une carotte dans la bouche

				} else if (SameDirection(yAxis, my.forward) && Physics.Linecast(initialPosition, finalPosition + direction, out hit, layerMask)) {
					print("bump");
					return; // do rotation, bump in wall then come back
				}
			}


			if (OnTheSides()) { // les oreilles se plient pas sur le côté

				if (SameDirection(my.up, direction) && Physics.Raycast(finalPosition, -yAxis, 1, layerMask)) {
					return;

				} else if (SameDirection(-my.up, direction) && Physics.Raycast(my.position + my.up, yAxis, out hit, 1, layerMask)) {

					MovableEntity movable = hit.transform.GetComponent<MovableEntity>();

					if (movable && movable.CanMove(direction)) {
						// this wall check is gonna be annoying

						movable.Push(direction * 3);
					} else return;
				}
			}
		}

		if (Suspended && direction == -my.forward) {
			currentlyEating.StopEating();
			StopEating();
			EndMove(0*zAxis);

		} else
			ChangePosition(initialPosition, finalPosition, timeToMove, direction);

		if (!Eating || (!StandingUp() && !JustAte)) {
			if (Eating)
				currentlyEating.transform.parent = my;

			Vector3 axis = new Vector3(direction.z, 0, -direction.x);

			Quaternion initialRotation = my.rotation;
			Quaternion finalRotation = Quaternion.AngleAxis(90, axis) * initialRotation;
			finalRotation = RoundRotation(finalRotation);
			ChangeRotation(initialRotation, finalRotation, timeToMove, direction);
        }
		if (Eating && !JustAte && currentlyEating.transform.parent != my)
			currentlyEating.Push(direction);

		if (currentlyEating && currentlyEating.FullyEaten)
            StopEating();
	}

	public override bool EndMove(Vector3 direction) {

		Vector3 pos = my.position;
		Suspended = false;

		RaycastHit hit;
		bool onGround = Physics.Raycast(pos, -yAxis, out hit, 1, layerMask);

        if (StandingUp() && onGround) {
            LevelEntrance entry = hit.transform.GetComponent<LevelEntrance>();
            if (entry)
                entry.EnterLevel();
        }

		if (SameDirection(my.forward, -yAxis)) {

			if (Eating) {
				JustAte = true;
				currentlyEating.Eat(pos - yAxis);
			} else if (onGround) {
				MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
				if (movable) {
					if (CollideWithMovableObject(movable, -yAxis) && Eating)
						HandleFalling(pos);
				}
			}
		}
        
		if (!onGround) {
			// there's a hole
			OnEars = UpsideDown() && Physics.Raycast(pos, -yAxis, 2, layerMask);
			if (OnEars) return true; // on tient sur les oreilles tout va bien

            if (Eating && !Colinear(my.forward, yAxis) && Physics.Raycast(pos + my.forward, -yAxis, 1, layerMask)) {

                if (Physics.Raycast(pos + my.forward, yAxis, 1, layerMask)) {
                    Suspended = true;
                    return true;// a ceiling holds the carrot, wer'e suspended
                }

                currentlyEating.transform.parent = my;

                Vector3 axis = new Vector3(-my.forward.z, 0, my.forward.x);

                Quaternion initialRotation = my.rotation;
                Quaternion finalRotation = Quaternion.AngleAxis(90, axis) * initialRotation;
                finalRotation = RoundRotation(finalRotation);
                ChangeRotation(initialRotation, finalRotation, timeToMove, -my.forward);
            }
            HandleFalling(pos);

			return false;
        }

        if (StandingUp() && Physics.Raycast(pos, yAxis, out hit, 1, layerMask)) {

            MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
            if (movable) {
                if (movable.CanMove(direction))
                    movable.Push(direction);
                else if (movable.CanMove(yAxis))
                    movable.Push(yAxis);
                return false;
            }
        }

        if (UpsideDown() && !OnEars && !Eating) {
			OnHead = true;
            ExecuteMove(direction);

		} else {
			OnEars = OnHead = false;
			if (Physics.Raycast(pos, my.up, out hit, 1, layerMask)) {
				//mes oreilles sont dans un mur

				MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
				if (movable && movable.CanMove(my.up)) {
					movable.Push(my.up);

				} else {

                    if (Physics.Raycast(my.position, -my.up, out hit, 1, layerMask)) {
						movable = hit.transform.GetComponent<MovableEntity>();
						if (movable && movable.CanMove(-my.up)) {
							movable.Push(-my.up); // should check if not eating first (carrot could be stuck under ceiling)
							//carrying = movable;
						} else return true;

                    }
                    else if (Eating && Physics.Raycast(my.position + my.forward, -my.up, out hit, 1, layerMask)) {
						movable = hit.transform.GetComponent<MovableEntity>();
						if (movable && movable.CanMove(-my.up)) {
							movable.Push(-my.up);
							//carrying = movable;
						} else return true;
                    }
                    ChangePosition(pos, RoundPosition(pos - my.up), timeToMove, -my.up);
					return false;
				}
			}
		}
		return true;
	}

	void HandleFalling(Vector3 pos) {
		if (OnTheSides() && Physics.Raycast(pos + my.up, -yAxis, 1, layerMask)) {

			Vector3 axis = new Vector3(-my.up.z, 0, my.up.x);

			Quaternion initialRotation = my.rotation;
			Quaternion finalRotation = Quaternion.AngleAxis(90, axis) * initialRotation;
			finalRotation = RoundRotation(finalRotation);
			ChangeRotation(initialRotation, finalRotation, timeToMove, -my.up);
		}
		ChangePosition(pos, RoundPosition(pos - yAxis), timeToFall, -yAxis); // fall
	}

	bool HandleCollision(Transform obstacle, Vector3 direction) {
		MovableEntity movable = obstacle.GetComponent<MovableEntity>();
		if (movable)
			return CollideWithMovableObject(movable, direction);
		else return false; // blocked by wall
	}

	bool CollideWithMovableObject(MovableEntity movable, Vector3 direction) {
		if (movable.CanMove(direction))
			movable.Push(direction);
		else if (SameDirection(direction, my.forward) && !Eating && movable.CanBeEaten(direction) && !ShouldRiseOnEars()) {
			StartEating(movable, direction);
		}
		else return false; // blocked by wall
		return true;
	}

	public void StartEating(MovableEntity movable, Vector3 direction) {

		Eating = true;
		currentlyEating = movable;
		JustAte = true;
		currentlyEating.Eat(my.position + direction);
		GameplayManager.instance.AddMove(new RecordableMove(currentlyEating, RecordableMove.eType.StartEating));
	}

    public override void StopEating() {
		GameplayManager.instance.AddMove(new RecordableMove(currentlyEating, RecordableMove.eType.StopEating));

        Eating = false;
        currentlyEating.transform.parent = null;
        currentlyEating = null;
    }

	#endregion

	#region Current State Utility

	bool StandingUp() {
		return SameDirection(my.up, yAxis);
	}

	bool OnTheSides() {
		return Colinear(my.right, yAxis);
	}

	bool UpsideDown() {
		return SameDirection(my.up, -yAxis);
	}

	bool ShouldRiseOnEars() {
        return OnHead;// && (Eating || !Physics.Raycast(my.position, yAxis, 1, layerMask));
	}

	#endregion

}