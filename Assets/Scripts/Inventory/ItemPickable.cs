using System; // REQUIRED for C# Action events
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickable : MonoBehaviour
{
    // =======================================================================
    // THE OBSERVER PATTERN EVENT (BROADCASTER)
    // Passes: (GameObject weaponPrefab) so the listener knows what weapon to add
    // =======================================================================
    public static event Action<GameObject> OnWeaponPickedUp;

    public ItemSO itemScriptableObject;

    // Detects when the player steps into the weapon's pickup radius
    private void OnTriggerEnter(Collider other)
    {
        // 1. Safety Check: Make sure it's the Player touching the gun, and that our data sheet exists
        if (other.CompareTag("Player") && itemScriptableObject != null)
        {
            // 2. Verify that we actually assigned an equipped prefab inside our ScriptableObject
            if (itemScriptableObject.equippedWeaponPrefab != null)
            {
                // =======================================================================
                // EVENT BROADCAST: Shout out to the world that this weapon was collected!
                // =======================================================================
                OnWeaponPickedUp?.Invoke(itemScriptableObject.equippedWeaponPrefab);

                Debug.Log($"[Event Shouted] Weapon '{itemScriptableObject.name}' picked up. Notifying observers.");

                // 3. Destroy the ground pickup object variant so it disappears from the floor
                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"[Pickup Error] There is no Equipped Weapon Prefab assigned inside the ScriptableObject: {itemScriptableObject.name}!");
            }
        }
    }
}