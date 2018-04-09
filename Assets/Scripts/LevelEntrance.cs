using UnityEngine;

public class LevelEntrance : MonoBehaviour {

    public bool completed;

    public void EnterLevel() {
        if (completed) return;
        GameplayManager.instance.EnterLevel(transform.parent);
    }
    
}
