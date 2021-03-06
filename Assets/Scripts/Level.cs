﻿using UnityEngine;

public class Level : RecordableObject {

    [HideInInspector] public int ID;
	public bool hidden;

    Carrot[] carrots;
    LevelEntrance entrance;

    private void OnEnable() {
        carrots = GetComponentsInChildren<Carrot>();
        foreach (Carrot carrot in carrots) {
            carrot.level = this;
            carrot.SetFrozen(!hidden);
        }

        entrance = GetComponentInChildren<LevelEntrance>();
        if (entrance)
            entrance.level = this;
        
        GameplayManager.instance.AddLevel(this);

        GameplayManager.OnEnterLevel += OnEnterLevel;
        GameplayManager.OnExitLevel += OnExitLevel;
    }
    
    private void OnDisable() {
        GameplayManager.OnEnterLevel -= OnEnterLevel;
        GameplayManager.OnExitLevel -= OnExitLevel;
    }


    public bool IsComplete() {

        foreach (Carrot carrot in carrots) {
            if (!carrot.FullyEaten) return false;
        }

		// Add "exit level" to move history
		GameplayManager.instance.AddMove(new RecordableMove(this, RecordableMove.eType.ExitLevel));
		GameplayManager.instance.ExitLevel(true);
        GameplayManager.instance.SetResetPoint(+2);
		if (!hidden)
			GetComponentInChildren<LevelEntrance>().completed = true;

        return true;
    }

    

    void OnEnterLevel(int ID) {
        if (this.ID == ID) {
            foreach (Carrot carrot in carrots)
                carrot.SetFrozen(false);
        }
    }

    void OnExitLevel(int ID, bool completed) {
        if (this.ID == ID) {

            foreach (Carrot carrot in carrots) {
                carrot.transform.parent = transform;
                carrot.SetFrozen(true);
            }
        }
    }

}
