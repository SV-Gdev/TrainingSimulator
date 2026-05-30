using UnityEngine;
using TMPro; 

public class ActiveWeaponUI : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TextMeshProUGUI weaponNameText;

    [Header("Player Reference")]
    [SerializeField] private WeaponSwap weaponSwap; 

    void Update()
    {
       
        if (weaponSwap == null || weaponNameText == null) return;

       
        if (weaponSwap.equippedWeapons.Count > 0)
        {
            
            GameObject activeWeapon = null;
            foreach (GameObject weapon in weaponSwap.equippedWeapons)
            {
                if (weapon != null && weapon.activeInHierarchy)
                {
                    activeWeapon = weapon;
                    break;
                }
            }

            
            if (activeWeapon != null)
            {
                string rawName = activeWeapon.name;

               
                if (rawName.Contains("(Clone)"))
                {
                    rawName = rawName.Replace("(Clone)", "").Trim();
                }

                weaponNameText.text = rawName;
            }
            else
            {
                
                weaponNameText.text = "Holstered";
            }
        }
        else
        {
            
            weaponNameText.text = "Unarmed";
        }
    }
}