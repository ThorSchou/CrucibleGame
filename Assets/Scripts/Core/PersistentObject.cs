using UnityEngine;

public class PersistentObject : MonoBehaviour
{
    void Awake()
    {
        GameObject[] existing = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in existing)
        {
            if (obj != gameObject && obj.name == gameObject.name && obj.GetComponent<PersistentObject>() != null)
            {
                Destroy(gameObject);
                return;
            }
        }
        DontDestroyOnLoad(gameObject);
    }
}