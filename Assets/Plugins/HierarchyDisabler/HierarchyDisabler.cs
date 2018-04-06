#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class HierarchyDisabler : MonoBehaviour {

	public static HierarchyDisabler[] all;

	enum Type {
		Nothing, DisableAllOthers
	}
	[SerializeField] Type onEnable;

	private void OnEnable() {
		if (onEnable == Type.DisableAllOthers) {
			all = FindObjectsOfType<HierarchyDisabler>();

			foreach (HierarchyDisabler hd in all) {
				if (hd != this)
					hd.gameObject.SetActive(false);
			}
		}
	}
}
#endif