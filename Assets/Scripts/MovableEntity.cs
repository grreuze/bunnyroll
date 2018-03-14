using UnityEngine;
using System.Collections;

public class MovableEntity : MonoBehaviour {

	public LayerMask layerMask;

	protected Transform my;
	Vector3 spawnPoint;
	Quaternion spawnOrientation;
	
	static protected int inMovement;
	static protected float timeBetweenMoves = 0.06f;
	static protected float timeToMove = 0.3f;
	static protected float timeToRotate = 0.3f;
	static protected float timeToFall = 0.2f;

	private void Awake() {
		my = transform;
	}

	public void SetSpawn(Vector3 position, Quaternion rotation) {
		spawnPoint = position;
		spawnOrientation = rotation;
	}

	public void ResetToSpawn() {
		transform.position = spawnPoint;
		transform.rotation = spawnOrientation;
	}

	public virtual bool CanMove(Vector3 direction) {
		Vector3 pos = my.position;
		RaycastHit hit;
		if (Physics.Raycast(pos, direction, out hit, 1, layerMask)) {
			MovableEntity movable = hit.transform.GetComponent<MovableEntity>();
			if (movable) {
				movable.Push(direction);
				return movable.CanMove(direction);
			} else return false;
		}
		return true;
	}

	public virtual void Push(Vector3 direction) {
		if (CanMove(direction))
			ChangePosition(my.position, RoundPosition(my.position+direction), timeToMove, direction);
	}
	
	public virtual bool CanBeEaten(Vector3 direction) {
		return false;
	}

	public virtual void Eat(Vector3 position) {
		return;
	}

	public virtual bool FullyEaten
	{
		get { return true; }
	}

	protected virtual bool EndMove(Vector3 direction) {
		Vector3 pos = my.position;
		if (!Physics.Raycast(pos, -yAxis, 1, layerMask)) {
			// there's a hole
			ChangePosition(pos, RoundPosition(pos - yAxis), timeToFall, -yAxis); // fall
			return false;
		}
		return true;
	}

	#region Execute Movement

	public void ChangePosition(Vector3 startPos, Vector3 endPos, float duration, Vector3 direction) {
		StartCoroutine(_ChangePosition(startPos, endPos, duration, direction));
	}
	public void ChangeRotation(Quaternion startRot, Quaternion endRot, float duration, Vector3 direction) {
		StartCoroutine(_ChangeRotation(startRot, endRot, duration, direction));
	}

	IEnumerator _ChangePosition(Vector3 startPos, Vector3 endPos, float duration, Vector3 direction) {

		inMovement++;
		for (float elapsed = 0, t = 0; elapsed < duration; elapsed += Time.deltaTime) {
			t = elapsed / duration;
			my.position = Vector3.Lerp(startPos, endPos, t);
			yield return null;
		}
		my.position = endPos;

		if (EndMove(direction)) {
			yield return new WaitForSeconds(timeBetweenMoves);
		}
		inMovement--;
	}
	IEnumerator _ChangeRotation(Quaternion startRot, Quaternion endRot, float duration, Vector3 direction) {

		inMovement++;
		for (float elapsed = 0, t = 0; elapsed < duration; elapsed += Time.deltaTime) {
			t = elapsed / duration;
			my.rotation = Quaternion.Lerp(startRot, endRot, t);
			yield return null;
		}
		my.rotation = endRot;

		if (EndMove(direction)) {
			yield return new WaitForSeconds(timeBetweenMoves);
		}
		inMovement--;
	}
	#endregion
	
	#region Utility

	protected Vector3 xAxis = Vector3.right;
	protected Vector3 yAxis = Vector3.up;
	protected Vector3 zAxis = Vector3.forward;

	protected float Round(float a) {
		return a > 0 ? 1 : a < 0 ? -1 : 0;
	}

	protected float RoundAngle(float a) {
		return Mathf.Round(a / 90) * 90;
	}

	protected Vector3 RoundPosition(Vector3 a) {
		a.x = Mathf.Round(a.x);
		a.y = Mathf.Round(a.y);
		a.z = Mathf.Round(a.z);
		return a;
	}

	protected Quaternion RoundRotation(Quaternion a) {
		Vector3 euler = a.eulerAngles;
		euler.x = RoundAngle(euler.x);
		euler.y = RoundAngle(euler.y);
		euler.z = RoundAngle(euler.z);
		return Quaternion.Euler(euler);
	}

	protected bool SameDirection(Vector3 a, Vector3 b) {
		return Mathf.Round(Vector3.Dot(a, b)) == 1;
	}
	protected bool Colinear(Vector3 a, Vector3 b) {
		return Mathf.Round(Mathf.Abs(Vector3.Dot(a, b))) == 1;
	}

	#endregion
}
