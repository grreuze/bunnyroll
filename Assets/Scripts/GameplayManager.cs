using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameplayManager : MonoBehaviour {

    public static GameplayManager instance;
	public static CatController player;

	public static bool inLevel;

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

    public void AddActor(MovableEntity actor)
    {
		if (!actors.Contains(actor)) {
			actors.Add(actor);

			if (player) {
				actors.Remove(player);
				actors.Add(player); // we want the player last in our list

			} else {
				CatController bunny = actor.GetComponent<CatController>();
				if (bunny) player = bunny;
			}
		}
    }

    public void RemoveActor(MovableEntity actor)
    {
        if (actors.Contains(actor))
            actors.Remove(actor);
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

}
