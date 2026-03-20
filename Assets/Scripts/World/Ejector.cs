using UnityEngine;

// Attach to any collectable that should fly out of a broken box or dead enemy.
// Temporarily disables the collect trigger so the player doesn't instantly grab it.
public class Ejector : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bounceSound;
    [SerializeField] private BoxCollider2D collectableTrigger;
    [SerializeField] private Vector2 launchPower = new Vector2(300, 300);

    public bool launchOnStart;

    private Rigidbody2D rb;
    private float counter;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        if (launchOnStart)
        {
            Launch();
            if (collectableTrigger != null) collectableTrigger.enabled = false;
        }
        else
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            GetComponent<Collider2D>().enabled = false;
            if (collectableTrigger != null) collectableTrigger.enabled = true;
        }
    }

    void Update()
    {
        if (collectableTrigger == null) return;
        if (counter > 0.5f)
            collectableTrigger.enabled = true;
        else
            counter += Time.deltaTime;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (launchOnStart && collectableTrigger != null && collectableTrigger.enabled)
            audioSource.PlayOneShot(bounceSound, rb.linearVelocity.magnitude / 10 * audioSource.volume);
    }

    public void Launch()
    {
        rb.AddForce(new Vector2(
            launchPower.x * Random.Range(-1f, 1f),
            launchPower.y * Random.Range(1f, 1.5f)));
    }
}