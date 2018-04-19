using UnityEngine;

public class LevelEntrance : MonoBehaviour {

    public Level level;
    public bool completed;


    public void OnEnable() {
        GameplayManager.OnEnterLevel += OnEnterLevel;
        GameplayManager.OnExitLevel += OnExitLevel;
    }
    public void OnDisable() {
        GameplayManager.OnEnterLevel -= OnEnterLevel;
        GameplayManager.OnExitLevel -= OnExitLevel;
    }
    
    public void EnterLevel() {
        if (completed || !level || GameplayManager.inLevel) return;
        print("j'entre dans le niveau");
		// Add "enter level" to move history
		GameplayManager.instance.AddMove(new RecordableMove(level, RecordableMove.eType.EnterLevel));
        GameplayManager.instance.SetResetPoint();
		GameplayManager.instance.EnterLevel(level);
    }
    
    void OnEnterLevel(int ID)  {
        if (ID == level.ID)
            completed = false;
    }
    
    void OnExitLevel(int ID, bool completed) {

        if (completed = true && ID == level.ID) {

            // change my material to a regular tile one
            // this should be added to the move history stack, to change back if the player cancels it

        }

    }

}
