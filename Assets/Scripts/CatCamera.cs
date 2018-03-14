using UnityEngine;

public class CatCamera : MonoBehaviour {

	[SerializeField] Transform target;
	[SerializeField] Vector3 baseOffset = new Vector3(0, 1, -1);
	[SerializeField] float distance = 8;
	[SerializeField] float damp = 0.1f;

	float deltaTime;
	Transform my;
	Vector3 camPosition, lastFrameCamPos;
	
	void Start () {
		my = transform;
	}
	
	void Update () {
		deltaTime = Time.deltaTime;

		camPosition = target.position + baseOffset * distance;
		my.position = SmoothApproach(my.position, lastFrameCamPos, camPosition, deltaTime/damp);
		lastFrameCamPos = camPosition;
	}

	Vector3 SmoothApproach(Vector3 pastPosition, Vector3 pastTargetPosition, Vector3 targetPosition, float t) {
		Vector3 v = (targetPosition - pastTargetPosition) / t;
		Vector3 f = pastPosition - pastTargetPosition + v;
		return targetPosition - v + f * Mathf.Exp(-t);
	}

}
