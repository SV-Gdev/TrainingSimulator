using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickable : MonoBehaviour
{
    public ItemSO itemScriptableObject;

    // Detects when the player steps into the weapon's pickup radius
    private void OnTriggerEnter(Collider other)
    {
        // 1. Safety Check: Make sure it's the Player touching the gun, and that our data sheet exists
        if (other.CompareTag("Player") && itemScriptableObject != null)
        {
            // 2. Try to find the WeaponSwap script component on the Player object
            WeaponSwap playerInventory = other.GetComponent<WeaponSwap>();

            if (playerInventory != null)
            {
                // 3. Verify that we actually assigned an equipped prefab inside our ScriptableObject
                if (itemScriptableObject.equippedWeaponPrefab != null)
                {
                    // Pass the fully functional weapon prefab to your inventory manager
                    playerInventory.AddWeaponToInventory(itemScriptableObject.equippedWeaponPrefab);

                    // 4. Destroy the ground pickup object variant so it disappears from the floor
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogError($"[Pickup Error] There is no Equipped Weapon Prefab assigned inside the ScriptableObject: {itemScriptableObject.name}!");
                }
            }
        }
    }
}