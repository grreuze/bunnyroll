using UnityEngine;

public class Level : MonoBehaviour {

    Carrot[] carrots;

    private void OnEnable() {
        carrots = GetComponentsInChildren<Carrot>();

        foreach (Carrot carrot in carrots)
            carrot.level = this;
    }

    public bool IsComplete() {

        foreach (Carrot carrot in carrots) {
            if (!carrot.FullyEaten) return false;
        }
        GameplayManager.instance.ExitLevel();
        GetComponentInChildren<LevelEntrance>().completed = true;

        return true;
    }


}
