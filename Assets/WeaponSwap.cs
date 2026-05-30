using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwap : MonoBehaviour, IDataPersistence
{

   
    public event Action OnWeaponInventoryChanged;

    public List<GameObject> equippedWeapons = new List<GameObject>();
    private int currentWeaponIndex = 0;

    [SerializeField] Animator anim;
    [SerializeField] private Transform weaponHolder;

    void Start()
    {

        ItemPickable.OnWeaponPickedUp += AddWeaponToInventory;

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


        equippedWeapons.RemoveAll(item => item == null);

        bool hasWeapons = equippedWeapons.Count > 0;
        bool holdingGrenade = false;


        for (int i = 0; i < equippedWeapons.Count; i++)
        {
            if (equippedWeapons[i] != null)
            {
                bool isCurrent = (i == currentWeaponIndex);
                equippedWeapons[i].SetActive(isCurrent);


                if (isCurrent && equippedWeapons[i].name.ToLower().Contains("grenade"))
                {
                    holdingGrenade = true;
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


        OnWeaponInventoryChanged?.Invoke();
    }

    public void AddWeaponToInventory(GameObject weaponPrefab)
    {

        GameObject newWeapon = Instantiate(weaponPrefab, weaponHolder, false);

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = Vector3.one;


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