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

    private int selectedIndex = 0;
    private bool leftWasDown;
    private bool rightWasDown;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(false);

        for (int i = 0; i < weaponButtons.Length; i++)
        {
            int index = i;
            weaponButtons[i].onClick.AddListener(() => BuyWeapon(weapons[index]));
        }

        RefreshButtonStats();
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
    }

    private void HandleNavigation()
    {
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;

        if (left && !leftWasDown)
        {
            selectedIndex = (selectedIndex - 1 + weaponButtons.Length) % weaponButtons.Length;
            UpdateSelection();
        }
        if (right && !rightWasDown)
        {
            selectedIndex = (selectedIndex + 1) % weaponButtons.Length;
            UpdateSelection();
        }

        leftWasDown = left;
        rightWasDown = right;
    }

    private void UpdateSelection()
    {
        if (normalSprite == null || selectedSprite == null) return;

        for (int i = 0; i < weaponButtons.Length; i++)
            weaponButtons[i].GetComponent<Image>().sprite = normalSprite;

        weaponButtons[selectedIndex].GetComponent<Image>().sprite = selectedSprite;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        selectedIndex = 0;
        UpdateSelection();
        NewPlayer.Instance.Freeze(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        NewPlayer.Instance.Freeze(false);
    }

    private void BuyWeapon(WeaponData weapon)
    {
        if (weapon == null)
        {
            Debug.Log("No weapon data assigned!");
            return;
        }
        if (GameManager.Instance.equippedWeapon == weapon)
        {
            Debug.Log("Already equipped: " + weapon.weaponName);
            return;
        }
        if (GameManager.Instance.SpendCoins(weapon.price))
        {
            GameManager.Instance.EquipWeapon(weapon);
            Debug.Log("Equipped: " + weapon.weaponName);
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }
}
