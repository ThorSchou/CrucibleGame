using UnityEngine;
using UnityEngine.SceneManagement;

// Place on the arena entrance door AND the hub exit door.
// Toggle isArenaEntrance on the door that starts a new round.
public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] private string loadSceneName;
    [SerializeField] private string spawnPointId; // which SpawnPoint to place the player at in the next scene
    [SerializeField] private bool isArenaEntrance; // true on the door going INTO the arena
    [SerializeField] private bool clearInventory;
    [SerializeField] private bool resetRun; // true on the door that exits back to main menu

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject != NewPlayer.Instance.gameObject) return;

        if (isArenaEntrance)
            RoundManager.Instance.StartNewRound();

        if (clearInventory)
            GameManager.Instance.ClearInventory();

        if (resetRun)
        {
            RoundManager.Instance.ResetRounds();
            GameManager.Instance.ResetRun();
        }

        GameManager.nextSpawnPointId = spawnPointId;
        SceneManager.LoadScene(loadSceneName);
        enabled = false;
    }
}