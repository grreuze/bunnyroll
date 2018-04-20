using UnityEngine;

public class PlaneOfDeath : MonoBehaviour {
    
    private void OnTriggerEnter(Collider other) {

        if (other.GetComponent<MovableEntity>()) {

            print("game over " + other.gameObject);
        }

    }

}
