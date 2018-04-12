using System.Collections.Generic;
using System.Collections;
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
        {
            foreach(MovableEntity actor in actors)
                actor.CancelLastMove();

        } else if (Input.GetButtonDown("Restart"))
        {
            foreach (MovableEntity actor in actors)
                actor.ResetMoves();
        }
    }

    #region Move History

    public void AddActor(MovableEntity actor)
    {
		if (!actors.Contains(actor)) {
			actors.Add(actor);

			if (player) {
				actors.Remove(player);
				actors.Add(player); // we want the player last in our list

			} else {
				BunnyController bunny = actor.GetComponent<BunnyController>();
				if (bunny) player = bunny;
			}
		}
    }

    public void RemoveActor(MovableEntity actor)
    {
        if (actors.Contains(actor))
            actors.Remove(actor);
    }

	public void SetNewResetPoint() {
        foreach (MovableEntity actor in actors)
            actor.SetResetPoint(+1);
    }

	public void WaitForEndOfMove() {
		StopAllCoroutines();

        //foreach (MovableEntity actor in actors)
        //    actor.EndMove(Vector3.zero);

        StartCoroutine(_WaitForEndOfMove());
	}

	IEnumerator _WaitForEndOfMove() {
		while (MovableEntity.globalActions > 0) yield return null;

		foreach (MovableEntity actor in actors)
			actor.AddMove();
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

            SetNewResetPoint();

            OnEnterLevel?.Invoke(level.ID);
        }
    }

    public void ExitLevel(bool completed) {
        inLevel = false;

        foreach (GameObject lvl in levels)
            lvl.SetActive(true);
        
        SetNewResetPoint();

        OnExitLevel?.Invoke(activeLevel, completed);
    }
    
    public delegate void EnterLevelEvent(int ID);
    public static event EnterLevelEvent OnEnterLevel;

    public delegate void ExitLevelEvent(int ID, bool completed);
    public static event ExitLevelEvent OnExitLevel;

    #endregion

}
