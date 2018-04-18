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

	public List<List<RecordableMove>> moveHistory = new List<List<RecordableMove>>();
	public static int moveIndex;
	public static int resetPoint;

	public void AddMove(RecordableMove newMove) {
		if (moveHistory.Count <= moveIndex)
			moveHistory.Add(new List<RecordableMove>());

		if (moveHistory[moveIndex].Contains(newMove)) return; // failsafe

		moveHistory[moveIndex].Add(newMove);
	}

	public void AddMoveWithOffset(RecordableMove newMove, int offset) {
		moveHistory[moveIndex + offset].Add(newMove);
	}

	void ResetTo(int index) {

		int count = moveHistory.Count - index;

		for (int i = 0; i < count; i++)
			CancelLastMove();

	}

	void CancelLastMove() {
		int index = moveHistory.Count - 1; // Count is different from moveIndex as moveIndex only changes when the move is over.

		if (index < 0) return;

		foreach (RecordableMove state in moveHistory[index]) {
            switch (state.type) {
                case RecordableMove.eType.EntityState:
                    (state as EntityState).SetAsCurrent();
                    break;
                case RecordableMove.eType.EnterLevel:
                    ExitLevel(false);
                    break;
                case RecordableMove.eType.ExitLevel:
                    EnterLevel(state.reference as Level);
                    break;
                case RecordableMove.eType.StartEating:
                    player.StopEating();
                    break;
                case RecordableMove.eType.StopEating:
                    player.currentlyEating = state.reference as MovableEntity;
                    break;
                case RecordableMove.eType.ResetPoint:
                    resetPoint = (state as ResetPoint).index;
                    break;
                default:
                    Debug.LogError("Missing Recordable Move Type");
                    break;
            }
		}

		moveHistory.RemoveAt(index);
		moveIndex = index;
	}

	void ResetMoves() {
		ResetTo(resetPoint);
	}
	
    public void SetResetPoint(int offset = 0) {
        AddMove(new ResetPoint(player, resetPoint)); // Remember the last resetPoint
        resetPoint = moveIndex + offset;
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

            OnEnterLevel?.Invoke(level.ID);
        }
    }

    public void ExitLevel(bool completed) {
        inLevel = false;
		
		foreach (GameObject lvl in levels)
            lvl.SetActive(true);
        
		OnExitLevel?.Invoke(activeLevel, completed);
    }
    
    public delegate void EnterLevelEvent(int ID);
    public static event EnterLevelEvent OnEnterLevel;

    public delegate void ExitLevelEvent(int ID, bool completed);
    public static event ExitLevelEvent OnExitLevel;

    #endregion

}
