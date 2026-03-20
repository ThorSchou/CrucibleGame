using System.Numerics;
using UnityEngine;

// Instantiates one or more objects at this transform's position.
// Used by EnemyBase and Breakable to spawn coins/items on death.
public class Instantiator : MonoBehaviour
{
    [SerializeField] private GameObject[] objects;
    [SerializeField] private int amount; // If > 0, spawns this many copies of objects[0]

    public void InstantiateObjects()
    {
        if (objects.Length == 0) return;

        int count = amount > 0 ? amount : objects.Length;

        for (int i = 0; i < count; i++)
        {
            GameObject obj = amount > 0
                ? Instantiate(objects[0], transform.position, UnityEngine.Quaternion.identity, null)
                : Instantiate(objects[i], transform.position, UnityEngine.Quaternion.identity, null);

            // If the spawned object has an Ejector, launch it immediately
            if (obj.TryGetComponent<Ejector>(out var ejector))
                ejector.launchOnStart = true;
        }
    }
}