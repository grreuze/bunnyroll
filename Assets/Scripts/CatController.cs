using UnityEngine;

public class CatController : MovableEntity {
	
	Vector3 input;

	MovableEntity currentlyEating;

	// States
	bool onHead;
	bool onEars;
	bool suspended;
	bool eating;
	bool ateThisFrame;


	#region MonoBehaviour
	
	void Update () {
		if (inMovement == 0) {
			input.x = Round(Input.GetAxis("Horizontal"));
			input.z = Round(Input.GetAxis("Vertical"));
			
			if (input.sqrMagnitude == 1) {
				DetermineMovement();
			}
		}
	}
	#endregion

	void DetermineMovement() {
		if (suspended) {
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

		if (eating) {
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

		ateThisFrame = false;
		bool readyToEat = eating && direction == my.forward;

		if (suspended && readyToEat) {
			ateThisFrame = true;
			currentlyEating.Eat(finalPosition);

		} else {

			if (SameDirection(my.up, input) && OnTheSides() && Physics.Raycast(finalPosition, -yAxis, 1, layerMask)) {
				return; // les oreilles se plient pas sur le côté
			}
			RaycastHit hit;
			if (Physics.Linecast(initialPosition, readyToEat ? finalPosition + direction : finalPosition, out hit, layerMask)) {

				if (!HandleCollision(hit.transform, direction)) {
					// if stuck on your head, your ears push you up
					if (ShouldRiseOnEars()) {
						onEars = true;
						onHead = false;
						ChangePosition(my.position, my.position + yAxis, timeToFall, yAxis);
					}
					if (readyToEat) {
						ateThisFrame = true;
						currentlyEating.Eat(finalPosition);
					} else
						return; // there's a wall blocking us
				}
			} else if (eating && !StandingUp()) {
				if (SameDirection(direction, my.forward) && Physics.Raycast(initialPosition + my.forward, -yAxis, 1, layerMask)) {
					return; // tu peux pas tourner t'as une carotte dans la bouche

				} else if (Physics.Linecast(initialPosition, finalPosition + direction, out hit, layerMask)) {
					//return;
				}
				
			}
		}

		if (suspended && direction == -my.forward) {
			eating = false;
			currentlyEating = null;
			EndMove(Vector3.zero);
		} else
			ChangePosition(initialPosition, finalPosition, timeToMove, direction);
		
		if (!eating || (!StandingUp() && !ateThisFrame)) {
			if (eating)
				currentlyEating.transform.parent = my;

			Vector3 axis = new Vector3(direction.z, 0, -direction.x);

			Quaternion initialRotation = my.rotation;
			Quaternion finalRotation = Quaternion.AngleAxis(90, axis) * initialRotation;
			finalRotation = RoundRotation(finalRotation);
			ChangeRotation(initialRotation, finalRotation, timeToMove, direction);
		}
		if (eating && !ateThisFrame && currentlyEating.transform.parent != my)
			currentlyEating.Push(direction);

		if (currentlyEating && currentlyEating.FullyEaten) {
			eating = false;
			currentlyEating = null;
		}
	}

	protected override bool EndMove(Vector3 direction) {

		Vector3 pos = my.position;
		suspended = false;

		RaycastHit hit;
		bool onGround = Physics.Raycast(pos, -yAxis, out hit, 1, layerMask);

		if (SameDirection(my.forward, -yAxis)) {

			if (eating) {
				ateThisFrame = true;
				currentlyEating.Eat(pos - yAxis);
			} else if (onGround) {
				MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
				if (movable) {
					if (CollideWithMovableObject(movable, -yAxis) && eating) {
						HandleFalling(pos);
					}
				}
			}
		}

		if (StandingUp() && Physics.Raycast(pos, yAxis, out hit, 1, layerMask)) {

			MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
			if (movable) {
				if (movable.CanMove(direction)) {
					movable.Push(direction);
				} else if (movable.CanMove(yAxis))
					movable.Push(yAxis);
				return false;
			}
		}

		if (!onGround) {
			// there's a hole
			onEars = UpsideDown() && Physics.Raycast(pos, -yAxis, 2, layerMask);

			if (onEars) return true; // on tient sur les oreilles tout va bien
			
			if (eating && !Colinear(my.forward, yAxis) && Physics.Raycast(pos + my.forward, -yAxis, 1, layerMask)) {
				suspended = true;
				return true;
			}
			HandleFalling(pos);

			return false;
		}
		
		if (UpsideDown() && !onEars && !eating) {
			onHead = true;
			ExecuteMove(direction);

		} else {
			onEars = onHead = false;
			if (Physics.Raycast(pos, my.up, out hit, 1, layerMask)) {
				//mes oreilles sont dans un mur

				MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
				if (movable && movable.CanMove(my.up)) {
					movable.Push(my.up);

				} else if (!Physics.Raycast(pos, -my.up, 1, layerMask)) {
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
		else if (SameDirection(direction, my.forward) && !eating && movable.CanBeEaten(direction) && !ShouldRiseOnEars()) {
			eating = true;
			currentlyEating = movable;
			ateThisFrame = true;
			currentlyEating.Eat(my.position + direction);
		}
		else return false; // blocked by wall
		return true;
	}

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
		return onHead && (!Physics.Raycast(my.position, yAxis, 1, layerMask) || eating);
	}

	#endregion

}