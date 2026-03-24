using UnityEngine;

// Place empty GameObjects at each spawn location.
// Set the id to match the spawnPointId on the SceneLoadTrigger that leads here.
public class SpawnPoint : MonoBehaviour
{
    public string id;
    [SerializeField] private bool isDefault; // used when no spawnPointId is set

    void Start()
    {
        string target = GameManager.nextSpawnPointId;

        bool match = !string.IsNullOrEmpty(target) && target == id;
        bool fallback = string.IsNullOrEmpty(target) && isDefault;

        if (match || fallback)
        {
            NewPlayer.Instance.transform.position = transform.position;
            GameManager.nextSpawnPointId = null;
        }
    }
}
