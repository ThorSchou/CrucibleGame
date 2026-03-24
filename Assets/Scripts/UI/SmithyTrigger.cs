using UnityEngine;
using UnityEngine.InputSystem;

public class SmithyTrigger : MonoBehaviour
{
    private bool playerInRange = false;

    void Start()
    {
        SmithyUpgradeMenu.Instance = FindFirstObjectByType<SmithyUpgradeMenu>(FindObjectsInactive.Include);
    }

    void Update()
    {
        if (SmithyUpgradeMenu.Instance == null) return;
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame
            && !SmithyUpgradeMenu.Instance.IsOpen)
        {
            SmithyUpgradeMenu.Instance.Open();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
