using UnityEngine;

// Represents the player's currently equipped weapon.
// Right now the player only punches, so this is a lightweight placeholder.
// As the upgrade hub grows, swap weapon data here � attack logic stays in PlayerCombat,
// damage scaling stays in PlayerStats.

public class WeaponComponent : MonoBehaviour
{
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;

    public void EquipWeapon(WeaponData weapon)
    {
        if (weaponSpriteRenderer != null)
            weaponSpriteRenderer.sprite = weapon.sprite;
    }
}