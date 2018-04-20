using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField] Image gameOverPanel;

    private void Start() {

        gameOverPanel.gameObject.SetActive(false);



    }


}
