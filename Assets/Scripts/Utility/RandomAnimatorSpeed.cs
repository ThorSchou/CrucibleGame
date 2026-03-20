using UnityEngine;

// Randomises animator speed on start — useful for enemies so they
// don't all animate in perfect sync.
public class RandomAnimatorSpeed : MonoBehaviour
{
    [SerializeField] private float low = .3f;
    [SerializeField] private float high = 1.5f;

    void Start()
    {
        GetComponent<Animator>().speed = Random.Range(low, high);
    }
}