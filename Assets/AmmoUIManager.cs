using UnityEngine;
using TMPro;

public class AmmoUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the TextMeshPro text component here")]
    public TextMeshProUGUI ammoText; // CHANGED TO PUBLIC

    [Tooltip("Drag the parent panel/background of the ammo box here")]
    public GameObject ammoPanel; // CHANGED TO PUBLIC

    private void OnEnable()
    {
        // Listen to the Weapon for ammo counts
        WeaponManager.OnAmmoChanged += UpdateAmmoDisplay;

        // Listen to the Player for whether we should show the UI at all
        WeaponSwap.OnArmedStateChanged += ToggleUIVisibility;
    }

    private void OnDisable()
    {
        // FIXED: Changed '==' to '-=' to properly unsubscribe
        WeaponManager.OnAmmoChanged -= UpdateAmmoDisplay;
        WeaponSwap.OnArmedStateChanged -= ToggleUIVisibility;
    }

    // FIXED: Now accepts both current and reserve ammo amounts
    private void UpdateAmmoDisplay(int currentAmmoAmount, int reserveAmmoAmount)
    {
        // Safe check to prevent NullReferenceExceptions
        if (ammoText != null)
        {
            // Displays it beautifully as "30 / 90" while firing
            ammoText.text = currentAmmoAmount + " / " + reserveAmmoAmount;
        }
    }

    private void ToggleUIVisibility(bool isArmed)
    {
        // Turn the visual panel on or off
        if (ammoPanel != null)
        {
            ammoPanel.SetActive(isArmed);
        }
    }
}