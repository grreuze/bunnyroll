using UnityEngine;

[ExecuteInEditMode]
public class HierarchyDisabler : MonoBehaviour {

	public static HierarchyDisabler[] all;
	
	private void OnEnable() {
		all = FindObjectsOfType<HierarchyDisabler>();

		foreach (HierarchyDisabler hd in all) {
			if (hd != this)
				hd.gameObject.SetActive(false);
		}
	}
	
}
