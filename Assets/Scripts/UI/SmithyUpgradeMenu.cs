using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SmithyUpgradeMenu : MonoBehaviour
{
    public static SmithyUpgradeMenu Instance;

    [Header("Weapon Slots")]
    [SerializeField] private Button[] weaponButtons;
    [SerializeField] private WeaponData[] weapons;

    [Header("Button Stats")]
    [SerializeField] private TextMeshProUGUI[] damageTexts;
    [SerializeField] private TextMeshProUGUI[] priceTexts;

    [Header("Selection Visuals")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;

    private int selectedIndex = 0;
    private bool leftWasDown;
    private bool rightWasDown;
    private bool upWasDown;
    private bool downWasDown;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < weaponButtons.Length; i++)
        {
            int index = i;
            weaponButtons[i].onClick.AddListener(() => BuyWeapon(weapons[index]));
        }
        RefreshButtonStats();
    }

    private void Start()
    {
        if (panel != null) panel.SetActive(false);

        for (int i = 0; i < weaponButtons.Length; i++)
        {
            int index = i;
            // Highlight on mouse hover
            UnityEngine.EventSystems.EventTrigger trigger =
                weaponButtons[i].gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => {
                selectedIndex = index;
                UpdateSelection();
            });
            trigger.triggers.Add(entry);
        }

        if (closeButton != null)
        {
            UnityEngine.EventSystems.EventTrigger trigger =
                closeButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
            entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) => {
                selectedIndex = weaponButtons.Length;
                UpdateSelection();
            });
            trigger.triggers.Add(entry);
        }
    }

    public void Open()
    {
        panel.SetActive(true);
        selectedIndex = 0;
        UpdateSelection();
        NewPlayer.Instance.Freeze(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Close()
    {
        panel.SetActive(false);
        NewPlayer.Instance.Freeze(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void RefreshButtonStats()
    {
        for (int i = 0; i < weapons.Length && i < weaponButtons.Length; i++)
        {
            if (weapons[i] == null) continue;
            if (i < damageTexts.Length && damageTexts[i] != null)
                damageTexts[i].text = "DMG: " + weapons[i].damage;
            if (i < priceTexts.Length && priceTexts[i] != null)
                priceTexts[i].text = weapons[i].price.ToString();
        }
    }

    private void Update()
    {
        if (!IsOpen) return;

        HandleNavigation();

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame ||
            Keyboard.current.enterKey.wasPressedThisFrame)
        {
            weaponButtons[selectedIndex].onClick.Invoke();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (selectedIndex < weaponButtons.Length)
                weaponButtons[selectedIndex].onClick.Invoke();
            else
                Close();
        }
    }

    private void HandleNavigation()
    {
        bool up = Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed;
        bool down = Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        bool left = Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;

        if (up && !upWasDown)
        {
            if (selectedIndex < weaponButtons.Length) // on weapon buttons
                selectedIndex = (selectedIndex - 1 + weaponButtons.Length) % weaponButtons.Length;
            UpdateSelection();
        }
        if (down && !downWasDown)
        {
            if (selectedIndex < weaponButtons.Length)
                selectedIndex = (selectedIndex + 1) % weaponButtons.Length;
            UpdateSelection();
        }
        if (left && !leftWasDown)
        {
            selectedIndex = weaponButtons.Length; // go to close button
            UpdateSelection();
        }
        if (right && !rightWasDown)
        {
            selectedIndex = 0; // go back to first weapon
            UpdateSelection();
        }

        leftWasDown = left;
        rightWasDown = right;
        upWasDown = up;
        downWasDown = down;
    }

    private void UpdateSelection()
    {
        if (normalSprite == null || selectedSprite == null) return;

        for (int i = 0; i < weaponButtons.Length; i++)
            weaponButtons[i].GetComponent<Image>().sprite = normalSprite;

        if (closeButton != null)
            closeButton.GetComponent<Image>().sprite = normalSprite;

        if (selectedIndex < weaponButtons.Length)
            weaponButtons[selectedIndex].GetComponent<Image>().sprite = selectedSprite;
        else if (closeButton != null)
            closeButton.GetComponent<Image>().sprite = selectedSprite;
    }

    private void BuyWeapon(WeaponData weapon)
    {
        if (weapon == null) return;
        if (GameManager.Instance.equippedWeapon == weapon) return;
        if (GameManager.Instance.SpendCoins(weapon.price))
            GameManager.Instance.EquipWeapon(weapon);
        else
            Debug.Log("Not enough coins!");
    }
}