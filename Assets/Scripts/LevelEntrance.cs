using UnityEngine;

public class LevelEntrance : MonoBehaviour {

    public Level level;
    public bool completed;


    public void OnEnable() {
        GameplayManager.OnExitLevel += OnExitLevel;
    }
    public void OnDisable() {
        GameplayManager.OnExitLevel -= OnExitLevel;
    }
    
    public void EnterLevel() {
        if (completed || !level) return;
        GameplayManager.instance.EnterLevel(level);
    }
    

    void OnExitLevel(int ID, bool completed) {

        if (completed = true && ID == level.ID) {

            // change my material to a regular tile one
            // this should be added to the move history stack, to change back if the player cancels it

        }

    }

}
