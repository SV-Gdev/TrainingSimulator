using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour
{
    public List<GameObject> equippedWeapons = new List<GameObject>();
    private int currentWeaponIndex = 0;

    [SerializeField] Animator anim; // Drag your player's Animator here
    [SerializeField] private Transform weaponHolder; // Your hand bone slot

    void Start()
    {
        UpdateWeapon();
    }

    void Update()
    {
        if (equippedWeapons.Count <= 1) return;

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

        // Clean out any destroyed weapons
        equippedWeapons.RemoveAll(item => item == null);

        bool hasWeapons = equippedWeapons.Count > 0;

        // Loop through and activate the selected weapon index
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i] != null)
            {
                equippedWeapons[i].SetActive(i == currentWeaponIndex);
            }
        }

        // Update Animator Layers
        float targetWeight = hasWeapons ? 1f : 0f;
        anim.SetLayerWeight(1, targetWeight);
        anim.SetBool("IsArmed", hasWeapons);

        // =======================================================================
        // FIXED STATE CONTROLLER WIRING:
        // Uses your exact variables to cleanly handle armed vs unarmed states.
        // =======================================================================
        ActionState actionState = GetComponent<ActionState>();

        if (hasWeapons)
        {
            // Get the WeaponManager script component from the active weapon clone
            WeaponManager activeWeapon = equippedWeapons[currentWeaponIndex].GetComponent<WeaponManager>();

            // If both are found, link them together!
            if (actionState != null && activeWeapon != null)
            {
                actionState.SetWeapon(activeWeapon);
            }
        }
        else
        {
            // If the inventory list is completely empty, clean up the UI and states safely
            if (actionState != null)
            {
                actionState.SetWeapon(null); // Clears tracking references inside ActionState
            }

            // Find the white UI element container box and shut it down safely
            GameObject ammoBox = GameObject.Find("AmmoDisplay");
            if (ammoBox != null)
            {
                ammoBox.SetActive(false);
            }
        }
        // =======================================================================
    }

    public void AddWeaponToInventory(GameObject weaponPrefab)
    {
        // Spawns weapon safely inside your weaponHolder hand bone slot
        GameObject newWeapon = Instantiate(weaponPrefab, weaponHolder, false);

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = Vector3.one;

        // Turn off physics components on the clone
        Collider[] weaponColliders = newWeapon.GetComponentsInChildren<Collider>();
        foreach (Collider col in weaponColliders) col.enabled = false;

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