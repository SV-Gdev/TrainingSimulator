using UnityEngine;
using TMPro; // Make sure to use TextMeshPro for crisp UI text

public class ActiveWeaponUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI weaponNameText; // Drag your UI Text here

    [Header("Player Reference")]
    [SerializeField] private WeaponSwap weaponSwap; // Drag your Player or WeaponSwap object here

    void Update()
    {
        // 1. Safety Check: If references are missing, do nothing to prevent errors
        if (weaponSwap == null || weaponNameText == null) return;

        // 2. Check if the player is actually carrying any weapons
        if (weaponSwap.equippedWeapons.Count > 0)
        {
            // Get the index of the active weapon
            // Since currentWeaponIndex is private in your WeaponSwap script, 
            // we loop through to find whichever weapon GameObject is currently active.
            GameObject activeWeapon = null;
            foreach (GameObject weapon in weaponSwap.equippedWeapons)
            {
                if (weapon != null && weapon.activeInHierarchy)
                {
                    activeWeapon = weapon;
                    break;
                }
            }

            // 3. Update the UI text if an active weapon was found
            if (activeWeapon != null)
            {
                string rawName = activeWeapon.name;

                // Clean up the text so it doesn't display "(Clone)" on your screen
                if (rawName.Contains("(Clone)"))
                {
                    rawName = rawName.Replace("(Clone)", "").Trim();
                }

                weaponNameText.text = rawName;
            }
            else
            {
                // If a gun is registered but none are turned on, show empty hands
                weaponNameText.text = "Holstered";
            }
        }
        else
        {
            // 4. If the inventory list is completely empty, clear the text display
            weaponNameText.text = "Unarmed";
        }
    }
}