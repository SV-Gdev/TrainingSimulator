using UnityEngine;
using TMPro;

public class AmmoUIManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Drag the TextMeshPro text component here")]
    public TextMeshProUGUI ammoText; 

    [Tooltip("Drag the parent panel/background of the ammo box here")]
    public GameObject ammoPanel; 

    private void OnEnable()
    {
        
        WeaponManager.OnAmmoChanged += UpdateAmmoDisplay;

       
        WeaponSwap.OnArmedStateChanged += ToggleUIVisibility;
    }

    private void OnDisable()
    {
        
        WeaponManager.OnAmmoChanged -= UpdateAmmoDisplay;
        WeaponSwap.OnArmedStateChanged -= ToggleUIVisibility;
    }

   
    private void UpdateAmmoDisplay(int currentAmmoAmount, int reserveAmmoAmount)
    {
       
        if (ammoText != null)
        {
           
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