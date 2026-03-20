using UnityEngine;

// Unity's built-in 3D audio rolloff doesn't translate well to flat 2D games,
// so this manually scales volume based on distance to the player.
public class TwoDimensionalSound : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float randomPitchAdder = 0f;
    [SerializeField] private float range;
    [SerializeField] private float volume;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        audioSource.pitch += Random.Range(-randomPitchAdder / 3, randomPitchAdder);
    }

    void Update()
    {
        Vector3 distanceBetweenPlayer = transform.position - NewPlayer.Instance.transform.position;
        float magnitude = (range - distanceBetweenPlayer.magnitude) / range;
        audioSource.volume = Mathf.Clamp01(magnitude);
    }
}