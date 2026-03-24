using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MenuInputManager : MonoBehaviour
{
    [SerializeField] private GameObject firstSelected;

    private GameObject lastSelected;
    private bool usingMouse = false;

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        EventSystem.current.SetSelectedGameObject(firstSelected);
        lastSelected = firstSelected;
    }

    private void OnEnable()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected);
        lastSelected = firstSelected;
    }

    private void Update()
    {
        // Re-select last button if selection is lost
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            GameObject target = (lastSelected != null && lastSelected.activeInHierarchy) ? lastSelected : firstSelected;
            EventSystem.current.SetSelectedGameObject(target);
        }
        else
            lastSelected = EventSystem.current.currentSelectedGameObject;
    }
}