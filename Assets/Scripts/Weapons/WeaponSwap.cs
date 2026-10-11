using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour, IDataPersistence
{
   
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
    [SerializeField] private TMPro.TextMeshProUGUI weaponNameText;     

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
       
        if (Input.GetMouseButtonDown(0))
        {
            if (equippedWeapons.Count > 0 && currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
            {
                GameObject activeWeapon = equippedWeapons[currentWeaponIndex];
                WeaponProperties props = activeWeapon != null ? activeWeapon.GetComponent<WeaponProperties>() : null;

                if (props != null && props.category == WeaponCategory.Throwable)
                {
                    if (anim != null)
                    {
                        anim.SetTrigger("Toss");
                    }
                    return;
                }
            }
        }

        
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

    
    public void EquipWeaponByName(string weaponName)
    {
        if (string.IsNullOrEmpty(weaponName)) return;

        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            GameObject w = equippedWeapons[i];
            if (w != null && w.name.Replace("(Clone)", "").Trim() == weaponName)
            {
                currentWeaponIndex = i;
                UpdateWeapon();
                Debug.Log($"[Weapon Persistence] Re-equipped saved weapon: {weaponName}");
                return;
            }
        }

        Debug.LogWarning($"[Weapon Persistence] Saved weapon '{weaponName}' not found in equippedWeapons.");
    }

    void UpdateWeapon()
    {
        if (anim == null) return;

        equippedWeapons.RemoveAll(item => item == null);
        bool hasWeapons = equippedWeapons.Count > 0;
        bool holdingNonGun = false;

        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i] != null)
            {
                bool isCurrent = (i == currentWeaponIndex);
                equippedWeapons[i].SetActive(isCurrent);

                if (isCurrent)
                {
                    WeaponProperties props = equippedWeapons[i].GetComponent<WeaponProperties>();

                    if (props == null || props.category == WeaponCategory.Throwable || props.category == WeaponCategory.Melee)
                    {
                        holdingNonGun = true;
                    }
                }
            }
        }

        int shootingLayerIndex = anim.GetLayerIndex("Shooting");
        if (shootingLayerIndex != -1)
        {
            float targetWeight = (hasWeapons && !holdingNonGun) ? 1f : 0f;
            anim.SetLayerWeight(shootingLayerIndex, targetWeight);
        }

        anim.SetBool("IsArmed", hasWeapons && !holdingNonGun);

        if (hasWeapons)
        {
            currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, equippedWeapons.Count - 1);
            GameObject activeObj = equippedWeapons[currentWeaponIndex];

            
            ItemPickable pickup = activeObj != null ? activeObj.GetComponent<ItemPickable>() : null;
            ItemSO itemSO = (pickup != null) ? pickup.itemScriptableObject : null;

           
            string displayName = (itemSO != null && !string.IsNullOrEmpty(itemSO.itemName))
                ? itemSO.itemName
                : (activeObj != null ? activeObj.name.Replace("(Clone)", "").Trim() : "");

           
            if (weaponNameText != null)
            {
                weaponNameText.text = displayName;
            }

            if (playerAmmoPanel != null) playerAmmoPanel.SetActive(true);

            if (holdingNonGun)
            {
                if (playerWeaponManager != null) playerWeaponManager.SetCurrentWeapon(null);
                if (actionState != null) actionState.SetWeapon(null);
            }
            else
            {
                WeaponProperties activeProperties = activeObj != null ? activeObj.GetComponent<WeaponProperties>() : null;
                if (playerWeaponManager != null) playerWeaponManager.SetCurrentWeapon(activeProperties);
                if (actionState != null && playerWeaponManager != null) actionState.SetWeapon(playerWeaponManager);
            }
        }
        else
        {
            if (playerWeaponManager != null) playerWeaponManager.SetCurrentWeapon(null);
            if (actionState != null) actionState.SetWeapon(null);

            if (weaponNameText != null) weaponNameText.text = "";
        }

        OnArmedStateChanged?.Invoke(hasWeapons && !holdingNonGun);
        OnWeaponInventoryChanged?.Invoke();
    }

    public void AddWeaponToInventory(GameObject weaponPrefab)
    {
        if (weaponPrefab == null) return;

        if (weaponHolder == null)
        {
            Debug.LogWarning("[WeaponSwap] 'Weapon Holder' is not assigned in the Inspector, so weapons cannot be added to the player's hand.");
            return;
        }

        WeaponProperties incomingProps = weaponPrefab.GetComponent<WeaponProperties>();
        bool isThrowable = incomingProps != null && incomingProps.category == WeaponCategory.Throwable;

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
        if (pickupScript != null) pickupScript.enabled = false;

        equippedWeapons.Add(newWeapon);
        Debug.Log($"[WeaponSwap] Added: {weaponPrefab.name}");

        if (equippedWeapons.Count == 1)
        {
            currentWeaponIndex = 0;
        }

        UpdateWeapon();
    }

    public void ConsumeCurrentThrowable()
    {
        if (equippedWeapons.Count == 0) return;

        if (currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
        {
            GameObject activeWeapon = equippedWeapons[currentWeaponIndex];
            WeaponProperties props = activeWeapon != null ? activeWeapon.GetComponent<WeaponProperties>() : null;

            if (props != null && props.category == WeaponCategory.Throwable)
            {
                equippedWeapons.RemoveAt(currentWeaponIndex);

                if (activeWeapon != null)
                {
                    Destroy(activeWeapon);
                }

                currentWeaponIndex = 0;
                UpdateWeapon();

               
                InventoryManager invManager = FindObjectOfType<InventoryManager>();
                if (invManager != null)
                {
                    invManager.UpdateInventoryUI();
                }
            }
        }
    }


    public void LaunchGrenade()
    {
        if (equippedWeapons.Count == 0) return;
        if (currentWeaponIndex < 0 || currentWeaponIndex >= equippedWeapons.Count) return;

        GameObject activeWeapon = equippedWeapons[currentWeaponIndex];
        WeaponProperties props = activeWeapon != null ? activeWeapon.GetComponent<WeaponProperties>() : null;

        if (props == null || props.category != WeaponCategory.Throwable) return;
        if (props.throwablePrefab == null)
        {
            Debug.LogError("[WeaponSwap] Throwable Prefab is missing on WeaponProperties!");
            return;
        }

        Transform camTrans = Camera.main != null ? Camera.main.transform : transform;

        Vector3 spawnPos = (props.BarrelPos != null)
            ? props.BarrelPos.position
            : camTrans.position + (camTrans.forward * 0.8f);


        GameObject go = Instantiate(props.throwablePrefab, spawnPos, camTrans.rotation);


        Collider grenadeCollider = go.GetComponent<Collider>();
        if (grenadeCollider != null)
        {
            grenadeCollider.enabled = true;
            grenadeCollider.isTrigger = false;

            Collider playerCollider = GetComponentInParent<Collider>();
            if (playerCollider != null)
            {
                Physics.IgnoreCollision(grenadeCollider, playerCollider);
            }
        }


        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            float fwdForce = props.throwForce > 0 ? props.throwForce : 15f;
            float upForce = props.throwUpwardForce > 0 ? props.throwUpwardForce : 5f;

            Vector3 forceDir = (camTrans.forward * fwdForce) + (camTrans.up * upForce);
            rb.AddForce(forceDir, ForceMode.Impulse);
        }


        Grenade grenadeScript = go.GetComponent<Grenade>();
        if (grenadeScript != null)
        {
            grenadeScript.isThrown = true;
        }
        else
        {
            Debug.LogWarning("[WeaponSwap] Grenade script is missing on the throwable prefab!");
        }


        ConsumeCurrentThrowable();
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
            Debug.Log($"[Weapon Persistence] Saved weapon profile: {data.equippedWeaponName}");
        }
    }

    public void SaveData(ref GameData data)
    {
        data.equippedWeaponName = "";

        if (equippedWeapons.Count > 0 && currentWeaponIndex >= 0 && currentWeaponIndex < equippedWeapons.Count)
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