using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class ShopMenu : MonoBehaviour
{
    public static ShopMenu Instance;

    [Header("Weapon Buttons")]
    [SerializeField] private Button axeButton;
    [SerializeField] private Button swordButton;
    [SerializeField] private Button maceButton;

    [Header("Armor Buttons")]
    [SerializeField] private Button armorUpgrade1Button;

    [Header("Weapon Data")]
    [SerializeField] private WeaponData axeData;
    [SerializeField] private WeaponData swordData;
    [SerializeField] private WeaponData maceData;

    [Header("Armor Prices")]
    [SerializeField] private int armorUpgradePrice = 60;

    [Header("Selection Visuals")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;

    private Button[] buttons;
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

        buttons = new Button[]
        {
            axeButton, swordButton, maceButton,
            armorUpgrade1Button
        };

        axeButton.onClick.AddListener(() => BuyWeapon(axeData));
        swordButton.onClick.AddListener(() => BuyWeapon(swordData));
        maceButton.onClick.AddListener(() => BuyWeapon(maceData));
        armorUpgrade1Button.onClick.AddListener(() => BuyArmor(1, armorUpgradePrice));
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
            buttons[selectedIndex].onClick.Invoke();
        }
    }

    private void HandleNavigation()
    {
        bool left = Keyboard.current.aKey.isPressed;
        bool right = Keyboard.current.dKey.isPressed;

        if (left && !leftWasDown)
        {
            selectedIndex = (selectedIndex - 1 + buttons.Length) % buttons.Length;
            UpdateSelection();
        }
        if (right && !rightWasDown)
        {
            selectedIndex = (selectedIndex + 1) % buttons.Length;
            UpdateSelection();
        }

        leftWasDown = left;
        rightWasDown = right;
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].GetComponent<Image>().sprite = normalSprite;

        buttons[selectedIndex].GetComponent<Image>().sprite = selectedSprite;
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

    private void BuyArmor(int requiredLevel, int price)
    {
        if (GameManager.Instance.upgradeData.armorLevel >= requiredLevel)
        {
            Debug.Log("Already purchased!");
            return;
        }
        if (GameManager.Instance.SpendCoins(price))
        {
            GameManager.Instance.UpgradeArmor();
            Debug.Log("Armor upgraded to level " + requiredLevel);
        }
        else
        {
            Debug.Log("Not enough coins!");
        }
    }
}