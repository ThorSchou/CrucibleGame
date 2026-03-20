using UnityEngine;

public class StopMenuMusic : MonoBehaviour
{
    private void Start()
    {
        PersistentMusic music = FindFirstObjectByType<PersistentMusic>();
        if (music != null)
            Destroy(music.gameObject);
    }
}