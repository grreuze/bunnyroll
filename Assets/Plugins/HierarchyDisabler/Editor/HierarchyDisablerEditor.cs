using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
[CanEditMultipleObjects]
[CustomEditor(typeof(HierarchyDisabler))]
public class HierarchyDisablerEditor : Editor {

	static HierarchyDisablerEditor() {
		EditorApplication.hierarchyWindowItemOnGUI -= HierarchyWindowItem;
		EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItem;
	}

	static void HierarchyWindowItem(int selectionID, Rect selectionRect) {
		GameObject o = (GameObject)EditorUtility.InstanceIDToObject(selectionID);
		if (!o) return;

		HierarchyDisabler target = o.GetComponent<HierarchyDisabler>();
		if (target) {

			GUIStyle style = new GUIStyle();
			style.alignment = TextAnchor.MiddleLeft;

			//Comment
			style.normal.textColor = Color.red;
			Rect commentRect = selectionRect;
			commentRect.width = style.CalcSize(new GUIContent("boy")).x;
			
			commentRect.x -= commentRect.width + 8;

			//style.Draw(commentRect, new GUIContent("x"), selectionID);

			o.SetActive(GUI.Toggle(commentRect, o.activeSelf, new GUIContent()));

			EditorApplication.RepaintHierarchyWindow();
			
		}

	}
	
}
