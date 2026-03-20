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
        EventSystem.current.SetSelectedGameObject(firstSelected);
        lastSelected = firstSelected;
    }

    private void Update()
    {
        // Detect if player switched to keyboard/gamepad
        Keyboard keyboard = Keyboard.current;
        Gamepad gamepad = Gamepad.current;

        bool keyboardInput = keyboard != null && (
            keyboard.wKey.isPressed ||
            keyboard.sKey.isPressed ||
            keyboard.aKey.isPressed ||
            keyboard.dKey.isPressed ||
            keyboard.upArrowKey.isPressed ||
            keyboard.downArrowKey.isPressed ||
            keyboard.leftArrowKey.isPressed ||
            keyboard.rightArrowKey.isPressed ||
            keyboard.enterKey.wasPressedThisFrame ||
            keyboard.spaceKey.wasPressedThisFrame
        );

        bool gamepadInput = gamepad != null && (
            gamepad.leftStick.ReadValue().magnitude > 0.1f ||
            gamepad.dpad.ReadValue().magnitude > 0.1f ||
            gamepad.buttonSouth.wasPressedThisFrame
        );

        if (keyboardInput || gamepadInput)
        {
            if (usingMouse)
            {
                usingMouse = false;
                if (EventSystem.current.currentSelectedGameObject == null)
                    EventSystem.current.SetSelectedGameObject(lastSelected);
            }
        }

        // Detect mouse movement
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.delta.ReadValue().magnitude > 0.1f)
        {
            usingMouse = true;
        }

        // Track last selected object
        if (EventSystem.current.currentSelectedGameObject != null)
            lastSelected = EventSystem.current.currentSelectedGameObject;
    }
}