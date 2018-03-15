using System.Collections.Generic;
using UnityEngine;

public class GameplayManager : MonoBehaviour {

    public static GameplayManager instance;
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
        if (!actors.Contains(actor))
            actors.Add(actor);
    }

    public void RemoveActor(MovableEntity actor)
    {
        if (actors.Contains(actor))
            actors.Remove(actor);
    }




}
