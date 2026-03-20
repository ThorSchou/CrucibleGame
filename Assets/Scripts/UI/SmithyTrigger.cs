using UnityEngine;
using UnityEngine.InputSystem;

public class SmithyTrigger : MonoBehaviour
{
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame
            && !SmithyUpgradeMenu.Instance.gameObject.activeSelf)
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
