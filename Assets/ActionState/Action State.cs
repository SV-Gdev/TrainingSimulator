using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionState : MonoBehaviour
{
    ActionBase currentState;

    public ReloadState Reload = new ReloadState();
    public DefaultState Default = new DefaultState();

    [HideInInspector] public WeaponManager currentWeapon;
    [HideInInspector] public weaponAmmo Ammo;
    [HideInInspector] public Animator anim;

    // =======================================================================
    // FIX: Declaring the missing UI panel variable!
    // Drag your white background "AmmoDisplay" object into this slot in the Inspector.
    // =======================================================================
    [Header("UI Reference")]
    public GameObject ammoUIPanel;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Auto-detect fallback if you forgot to drag it into the inspector slot
        if (ammoUIPanel == null)
        {
            ammoUIPanel = GameObject.Find("AmmoDisplay");
        }

        SwitchState(Default);
    }

    void Update()
    {
        // =======================================================================
        // UNARMED SAFETY SYSTEM:
        // If there is no active gun script or active weapon object, hide the UI!
        // =======================================================================
        if (currentWeapon == null || Ammo == null)
        {
            if (ammoUIPanel != null && ammoUIPanel.activeSelf)
            {
                ammoUIPanel.SetActive(false); // Shuts off the white box entirely!
            }
            return; // Exit out early since we are unarmed
        }

        // Otherwise, keep running your normal state systems if a weapon is equipped
        if (currentState != null)
        {
            currentState.UpdateState(this);
        }
    }

    public void SwitchState(ActionBase state)
    {
        currentState = state;
        currentState.EnterState(this);
    }

    public void WeaponReloaded()
    {
        if (Ammo != null)
        {
            Ammo.Reload();
        }
        SwitchState(Default);
    }

    public void SetWeapon(WeaponManager weapon)
    {
        currentWeapon = weapon;

        if (weapon != null)
        {
            Ammo = weapon.Ammo;

            // Instantly turn the UI panel ON when a valid gun is put into your hands!
            if (ammoUIPanel != null)
            {
                ammoUIPanel.SetActive(true);
            }
        }
        else
        {
            // Clean up our internal references if the weapon is removed
            Ammo = null;

            // Instantly turn the UI panel OFF when your hands are empty!
            if (ammoUIPanel != null)
            {
                ammoUIPanel.SetActive(false);
            }
        }
    }
}