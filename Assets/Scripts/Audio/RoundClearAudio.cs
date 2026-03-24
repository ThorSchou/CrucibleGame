using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RoundClearAudio : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        RoundManager.Instance.onAllEnemiesDead.AddListener(Play);
    }

    void OnDestroy()
    {
        if (RoundManager.Instance != null)
            RoundManager.Instance.onAllEnemiesDead.RemoveListener(Play);
    }

    void Play()
    {
        audioSource.Play();
    }
}
