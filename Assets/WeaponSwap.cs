using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour, IDataPersistence
{
    // --- Events ---
    public static event Action<bool> OnArmedStateChanged;
    public event Action OnWeaponInventoryChanged;

    [Header("Weapon Inventory Control")]
    public List<GameObject> equippedWeapons = new List<GameObject>();
    private int currentWeaponIndex = 0;

    [Header("Components & Holders")]
    [SerializeField] Animator anim;
    [SerializeField] private Transform weaponHolder;

    [Header("UI References")]
    [SerializeField] private GameObject playerAmmoPanel;
    [SerializeField] private TMPro.TextMeshProUGUI playerAmmoText;

    private WeaponManager playerWeaponManager;
    private ActionState actionState;

    void Start()
    {
        playerWeaponManager = GetComponent<WeaponManager>();
        actionState = GetComponent<ActionState>();

        ItemPickable.OnWeaponPickedUp += AddWeaponToInventory;

        UpdateWeapon();
    }

    void Update()
    {
        if (equippedWeapons.Count <= 1) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f) EquipNext();
        else if (scroll < 0f) EquipPrevious();
    }

    public void EquipNext()
    {
        currentWeaponIndex = (currentWeaponIndex + 1) % equippedWeapons.Count;
        UpdateWeapon();
    }

    public void EquipPrevious()
    {
        currentWeaponIndex--;
        if (currentWeaponIndex < 0) currentWeaponIndex = equippedWeapons.Count - 1;
        UpdateWeapon();
    }

    void UpdateWeapon()
    {
        if (anim == null) return;

        equippedWeapons.RemoveAll(item => item == null);
        bool hasWeapons = equippedWeapons.Count > 0;
        bool holdingGrenade = false;

        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i] != null)
            {
                bool isCurrent = (i == currentWeaponIndex);
                equippedWeapons[i].SetActive(isCurrent);

                if (isCurrent)
                {
                    WeaponProperties props = equippedWeapons[i].GetComponent<WeaponProperties>();
                    if (props != null && props.category == WeaponCategory.Throwable)
                    {
                        holdingGrenade = true;
                    }
                }
            }
        }

        int shootingLayerIndex = anim.GetLayerIndex("Shooting");
        if (shootingLayerIndex != -1)
        {
            float targetWeight = (hasWeapons && !holdingGrenade) ? 1f : 0f;
            anim.SetLayerWeight(shootingLayerIndex, targetWeight);
        }

        anim.SetBool("IsArmed", hasWeapons && !holdingGrenade);

        if (hasWeapons)
        {
            // Ensure index stays in bounds after removals
            currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, equippedWeapons.Count - 1);

            WeaponProperties activeProperties = equippedWeapons[currentWeaponIndex].GetComponent<WeaponProperties>();
            if (playerWeaponManager != null) playerWeaponManager.SetCurrentWeapon(activeProperties);
            if (actionState != null && playerWeaponManager != null) actionState.SetWeapon(playerWeaponManager);
        }
        else
        {
            if (playerWeaponManager != null) playerWeaponManager.SetCurrentWeapon(null);
            if (actionState != null) actionState.SetWeapon(null);

            // ==========================================
            // FIX: Clear the UI text if inventory is empty
            // ==========================================
            if (playerAmmoText != null)
            {
                playerAmmoText.text = "";
            }
        }

        OnArmedStateChanged?.Invoke(hasWeapons && !holdingGrenade);
        OnWeaponInventoryChanged?.Invoke();
    }

    public void AddWeaponToInventory(GameObject weaponPrefab)
    {
        if (weaponPrefab == null || weaponHolder == null) return;

        WeaponProperties incomingProps = weaponPrefab.GetComponent<WeaponProperties>();
        bool isThrowable = incomingProps != null && incomingProps.category == WeaponCategory.Throwable;

        // Only block duplicates if it's NOT a throwable (allows stacking/carrying multiple grenades)
        if (!isThrowable)
        {
            foreach (var w in equippedWeapons)
            {
                if (w != null && (w.name == weaponPrefab.name || w.name.StartsWith(weaponPrefab.name)))
                {
                    return;
                }
            }
        }

        GameObject newWeapon = Instantiate(weaponPrefab, weaponHolder);
        newWeapon.name = weaponPrefab.name;
        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = weaponPrefab.transform.localScale;

        WeaponProperties props = newWeapon.GetComponent<WeaponProperties>();
        if (props != null)
        {
            weaponAmmo playerAmmoScript = GetComponent<weaponAmmo>();
            if (playerAmmoScript != null)
            {
                props.Ammo = playerAmmoScript;
            }
        }

        AmmoUIManager uiManager = newWeapon.GetComponent<AmmoUIManager>();
        if (uiManager != null)
        {
            uiManager.ammoPanel = playerAmmoPanel;
            uiManager.ammoText = playerAmmoText;
        }

        Collider[] colliders = newWeapon.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;

        Rigidbody rb = newWeapon.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        ItemPickable pickupScript = newWeapon.GetComponent<ItemPickable>();
        if (pickupScript != null) Destroy(pickupScript);

        equippedWeapons.Add(newWeapon);
        Debug.Log($"[WeaponSwap] Added: {weaponPrefab.name}");

        if (equippedWeapons.Count == 1)
        {
            currentWeaponIndex = 0;
        }

        UpdateWeapon();
    }

    // ==========================================
    // DELETES GRENADE ON THROW
    // ==========================================
    public void ConsumeCurrentThrowable()
    {
        if (equippedWeapons.Count == 0) return;

        if (currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
        {
            GameObject activeWeapon = equippedWeapons[currentWeaponIndex];
            WeaponProperties props = activeWeapon != null ? activeWeapon.GetComponent<WeaponProperties>() : null;

            if (props != null && props.category == WeaponCategory.Throwable)
            {
                // 1. Remove from inventory list so it cannot be scrolled to again
                equippedWeapons.RemoveAt(currentWeaponIndex);

                // 2. Completely destroy the gameobject
                if (activeWeapon != null)
                {
                    Destroy(activeWeapon);
                }

                // 3. Reset index safely back to primary weapon or bounds
                currentWeaponIndex = 0;

                // 4. Update the visual weapon in hands and the UI
                UpdateWeapon();
            }
        }
    }

    public void ForceWeaponRefreshAfterThrow()
    {
        currentWeaponIndex = 0;
        UpdateWeapon();
    }

    public void LoadData(GameData data)
    {
        if (!string.IsNullOrEmpty(data.equippedWeaponName))
        {
            Debug.Log($"[Weapon Persistence] Loading saved weapon profile: {data.equippedWeaponName}");
        }
    }

    public void SaveData(ref GameData data)
    {
        if (equippedWeapons.Count > 0 && currentWeaponIndex < equippedWeapons.Count)
        {
            GameObject activeWeapon = equippedWeapons[currentWeaponIndex];
            if (activeWeapon != null)
            {
                data.equippedWeaponName = activeWeapon.name.Replace("(Clone)", "").Trim();
            }
        }
    }

    void OnDestroy()
    {
        ItemPickable.OnWeaponPickedUp -= AddWeaponToInventory;
    }
}