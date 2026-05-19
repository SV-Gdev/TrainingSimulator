using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour
{
    public List<GameObject> equippedWeapons = new List<GameObject>();
    private int currentWeaponIndex = 0;

    [SerializeField] Animator anim; // Drag your player's Animator here
    [SerializeField] private Transform weaponHolder;

    void Start()
    {
        // Start fresh and ensure animations match our empty hands
        UpdateWeapon();
    }

    void Update()
    {
        if (equippedWeapons.Count <= 1) return; // No need to swap if 0 or 1 weapon

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            currentWeaponIndex = (currentWeaponIndex + 1) % equippedWeapons.Count;
            UpdateWeapon();
        }
        else if (scroll < 0f)
        {
            currentWeaponIndex--;
            if (currentWeaponIndex < 0) currentWeaponIndex = equippedWeapons.Count - 1;
            UpdateWeapon();
        }
    }

    void UpdateWeapon()
    {
        if (anim == null) return;

        // 1. SAFETY CHECK: Remove any null or destroyed weapons from the list automatically
        equippedWeapons.RemoveAll(item => item == null);

        // 2. Check if we actually have any valid weapons left
        bool hasWeapons = equippedWeapons.Count > 0;

        // 3. Safely loop through and activate only the current weapon index
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i] != null)
            {
                equippedWeapons[i].SetActive(i == currentWeaponIndex);
            }
        }

        // 4. Update Animator Layers
        // If hasWeapons is false, targetWeight becomes 0f, turning off the rifle layer!
        float targetWeight = hasWeapons ? 1f : 0f;
        anim.SetLayerWeight(1, targetWeight);
        anim.SetBool("IsArmed", hasWeapons);
    }

    public void AddWeaponToInventory(GameObject weaponPrefab)
    {
        // Pass 'false' as the 3rd argument to tell Unity: 
        // "Do NOT keep world position, snap directly to the parent's origin!"
        GameObject newWeapon = Instantiate(weaponPrefab, weaponHolder, false);

        // Hard-reset everything to clear out any remaining prefab offsets
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = Vector3.one;

        // Turn off physics so it doesn't drop to the floor
        Collider[] weaponColliders = newWeapon.GetComponentsInChildren<Collider>();
        foreach (Collider col in weaponColliders)
        {
            col.enabled = false;
        }

        Rigidbody weaponRb = newWeapon.GetComponent<Rigidbody>();
        if (weaponRb != null)
        {
            weaponRb.isKinematic = true;
            weaponRb.useGravity = false;
        }

        newWeapon.SetActive(false);
        equippedWeapons.Add(newWeapon);

        if (equippedWeapons.Count == 1)
        {
            currentWeaponIndex = 0;
        }

        UpdateWeapon();
    }
}