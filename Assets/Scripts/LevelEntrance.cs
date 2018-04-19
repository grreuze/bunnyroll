using UnityEngine;

public class LevelEntrance : MonoBehaviour {

    public Level level;
    public bool completed;

    [Header("Rendering")]
    public Material regularMat;
    public Material completeMat;
    public GameObject aura;
    public ParticleSystem burst;
    
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
        // Add "enter level" to move history
		GameplayManager.instance.AddMove(new RecordableMove(level, RecordableMove.eType.EnterLevel));
        GameplayManager.instance.SetResetPoint();
		GameplayManager.instance.EnterLevel(level);
        burst.Play();
    }
    
    void OnEnterLevel(int ID)  {
        if (ID == level.ID) {
            completed = false;
            aura.SetActive(false);
            GetComponent<Renderer>().sharedMaterial = completeMat;
        }
    }
    
    void OnExitLevel(int ID, bool completed) {

        if (ID == level.ID) {

            if (completed == true)
                aura.SetActive(false);
            else
            {
                GetComponent<Renderer>().sharedMaterial = regularMat;
                aura.SetActive(true);
            }
        }
    }

}
