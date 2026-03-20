using UnityEngine;
using UnityEngine.InputSystem;

public class ShopTrigger : MonoBehaviour
{
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E pressed, opening shop");
            ShopMenu.Instance.Open();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Something entered trigger: " + other.gameObject.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player in range!");
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}