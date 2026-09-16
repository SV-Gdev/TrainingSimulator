using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickable : MonoBehaviour
{
    public static event Action<GameObject> OnWeaponPickedUp;

    public ItemSO itemScriptableObject;

    public void EnableGroundPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public void DisablePhysicsInHand()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && itemScriptableObject != null)
        {
            if (itemScriptableObject.equippedWeaponPrefab != null)
            {
                OnWeaponPickedUp?.Invoke(itemScriptableObject.equippedWeaponPrefab);

                Debug.Log($"[Event Shouted] Weapon '{itemScriptableObject.name}' picked up. Notifying observers.");

                Destroy(gameObject);
            }
            else
            {
                Debug.LogError($"[Pickup Error] There is no Equipped Weapon Prefab assigned inside the ScriptableObject: {itemScriptableObject.name}!");
            }
        }
    }
}