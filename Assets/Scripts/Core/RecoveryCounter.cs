using UnityEngine;

public class RecoveryCounter : MonoBehaviour
{
    public float recoveryTime = 1f;
    [System.NonSerialized] public float counter;
    [System.NonSerialized] public bool recovering = false;

    void Start()
    {
        // Initialise to recoveryTime so the object is NOT recovering on spawn
        counter = recoveryTime;
    }

    void Update()
    {
        if (counter <= recoveryTime)
        {
            counter += Time.deltaTime;
            recovering = true;
        }
        else
        {
            recovering = false;
        }
    }
}