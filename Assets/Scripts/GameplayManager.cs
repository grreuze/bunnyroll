using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour {

    public static GameplayManager instance;
	public static BunnyController player;

	public static bool inLevel;
    public static int activeLevel;
    
    List<GameObject> levels = new List<GameObject>();
    List<MovableEntity> actors = new List<MovableEntity>();

    public void OnEnable() {
        if (!instance)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    public void Update() {
		if (Input.GetButtonDown("Cancel"))
			CancelLastMove();
		else if (Input.GetButtonDown("Restart"))
			ResetMoves();
    }

	#region Move History

	public List<List<EntityState>> moveHistory = new List<List<EntityState>>();
	public static int moveIndex;
	public static int resetPoint;

	public void AddMove(EntityState newMove) {
		if (moveHistory.Count <= moveIndex)
			moveHistory.Add(new List<EntityState>());

		if (moveHistory[moveIndex].Contains(newMove)) return; // failsafe

		moveHistory[moveIndex].Add(newMove);
	}
	public void AddMoveOnPrevious(EntityState newMove) {
		moveHistory[moveIndex-1].Add(newMove);
	}


	void ResetTo(int index) {

		int count = moveHistory.Count - index;

		for (int i = 0; i < count; i++)
			CancelLastMove();

	}

	void CancelLastMove() {
		int index = moveHistory.Count - 1; // Count is different from moveIndex as moveIndex only changes when the move is over.

		if (index < 0) return;

		foreach (EntityState state in moveHistory[index]) {
			
			if (state.IsEnterLevel()) {
				ExitLevel(false);
			} else if (state.IsExitLevel()) {
				EnterLevel(state.reference.GetComponent<Level>());
			} else if (state.IsStopEating()) {
				player.currentlyEating = state.reference.GetComponent<MovableEntity>();
			} else if (state.IsStartEating()) {
				player.StopEating();
			} else
				state.SetAsCurrent();

		}

		moveHistory.RemoveAt(index);
		moveIndex = index;
	}

	void ResetMoves() {
		ResetTo(resetPoint);
	}
	
    #endregion


    #region Level Management

    public void AddLevel(Level level) {
        if (!levels.Contains(level.gameObject)) {
            levels.Add(level.gameObject);
            level.ID = levels.Count - 1;
		}
    }

    public void EnterLevel(Level level) {
        if (!inLevel) {
            inLevel = true;
			
            foreach (GameObject lvl in levels)
                lvl.SetActive(false);

            level.gameObject.SetActive(true);
            activeLevel = level.ID;

			resetPoint = moveIndex;
            OnEnterLevel?.Invoke(level.ID);
        }
    }

    public void ExitLevel(bool completed) {
        inLevel = false;
		
		foreach (GameObject lvl in levels)
            lvl.SetActive(true);

		resetPoint = moveIndex;
		OnExitLevel?.Invoke(activeLevel, completed);
    }
    
    public delegate void EnterLevelEvent(int ID);
    public static event EnterLevelEvent OnEnterLevel;

    public delegate void ExitLevelEvent(int ID, bool completed);
    public static event ExitLevelEvent OnExitLevel;

    #endregion

}
