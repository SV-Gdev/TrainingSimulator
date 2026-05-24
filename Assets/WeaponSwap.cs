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

        // Clean out dead tracking reference links
        equippedWeapons.RemoveAll(item => item == null);

        bool hasWeapons = equippedWeapons.Count > 0;
        bool holdingGrenade = false;

        // Loop through the weapon array list
        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i] != null)
            {
                bool isCurrent = (i == currentWeaponIndex);
                equippedWeapons[i].SetActive(isCurrent);

                // DETECT GRENADE: Force lowercase conversion to avoid matching typo bugs
                if (isCurrent && equippedWeapons[i].name.ToLower().Contains("grenade"))
                {
                    holdingGrenade = true;
                }
            }
        }

        // Drop the Shooting Layer weight to 0 instantly if holding a grenade
        int shootingLayerIndex = anim.GetLayerIndex("Shooting");
        if (shootingLayerIndex != -1)
        {
            float targetWeight = (hasWeapons && !holdingGrenade) ? 1f : 0f;
            anim.SetLayerWeight(shootingLayerIndex, targetWeight);
        }

        // Set your combat stance tracking parameters safely
        anim.SetBool("IsArmed", hasWeapons && !holdingGrenade);

        ActionState actionState = GetComponent<ActionState>();

        if (hasWeapons)
        {
            WeaponManager activeWeapon = equippedWeapons[currentWeaponIndex].GetComponent<WeaponManager>();
            if (actionState != null && activeWeapon != null)
            {
                actionState.SetWeapon(activeWeapon);
            }
        }
        else
        {
            if (actionState != null) actionState.SetWeapon(null);

            GameObject ammoBox = GameObject.Find("AmmoDisplay");
            if (ammoBox != null) ammoBox.SetActive(false);
        }

        // =======================================================================
        // TARGET DIRECT CALL: Refreshes your InventoryManager UI tracking slots!
        // =======================================================================
        InventoryManager inventory = GetComponent<InventoryManager>();
        if (inventory == null) inventory = FindObjectOfType<InventoryManager>();

        if (inventory != null)
        {
            inventory.UpdateInventoryUI();
        }
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

    // Call this inside ActionState to seamlessly update inventory after a throw event completes
    public void ForceWeaponRefreshAfterThrow()
    {
        // Reset our slot tracking indicators back down to 0 safely
        currentWeaponIndex = 0;
        UpdateWeapon();
    }
}