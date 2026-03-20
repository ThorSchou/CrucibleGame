using UnityEngine;

// Lives on the player GameObject. Computes final stats from base values
// plus whatever upgrades the player has bought in the Upgrade hub.
// Add a new upgrade type here and a matching field in UpgradeData.
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private int baseMaxHealth = 6;
    [SerializeField] private float baseMoveSpeed = 7f;
    [SerializeField] private float baseJumpPower = 17f;
    [SerializeField] private int baseAttackDamage = 1;

    [Header("Amount Gained Per Upgrade")]
    [SerializeField] private int healthPerUpgrade = 1;
    [SerializeField] private float speedPerUpgrade = 0.5f;
    [SerializeField] private float jumpPerUpgrade = 1f;
    [SerializeField] private int damagePerUpgrade = 1;

    public const int MaxPossibleHealth = 20; // hard cap — 10 trifire slots (2 HP each)

    // +2 HP at rounds 3, 5, 7, 9... (every 2 rounds starting from round 3)
    private int RoundHealthBonus
    {
        get
        {
            int round = RoundManager.Instance.CurrentRound;
            if (round < 3) return 0;
            return ((round - 3) / 2 + 1) * 2; // round 3=+2, round 5=+4, round 7=+6...
        }
    }

    public int MaxHealth => Mathf.Min(baseMaxHealth + RoundHealthBonus + (GameManager.Instance.upgradeData.healthUpgrades * healthPerUpgrade), MaxPossibleHealth);
    public float MoveSpeed => baseMoveSpeed + (GameManager.Instance.upgradeData.speedUpgrades * speedPerUpgrade);
    public float JumpPower => baseJumpPower + (GameManager.Instance.upgradeData.jumpUpgrades * jumpPerUpgrade);
    public int AttackDamage => GameManager.Instance.equippedWeapon != null
    ? GameManager.Instance.equippedWeapon.damage
    : baseAttackDamage;
}