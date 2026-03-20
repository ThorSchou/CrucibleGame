using UnityEngine;
using UnityEngine.SceneManagement;

// Place on the arena entrance door AND the hub exit door.
// Toggle isArenaEntrance on the door that starts a new round.
public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private string loadSceneName;
    [SerializeField] private bool isArenaEntrance; // true on the door going INTO the arena
    [SerializeField] private bool clearInventory;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject != NewPlayer.Instance.gameObject) return;

        if (isArenaEntrance)
            RoundManager.Instance.StartNewRound();

        if (clearInventory)
            GameManager.Instance.ClearInventory();

        /*
        GameManager.Instance.hud.loadSceneName = loadSceneName;
        GameManager.Instance.hud.animator.SetTrigger("coverScreen");
        */

        SceneManager.LoadScene(loadSceneName);
        enabled = false;
    }
}