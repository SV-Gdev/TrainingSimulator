using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionState : MonoBehaviour
{
    ActionBase currentState;

    public ReloadState Reload = new ReloadState();
    public DefaultState Default = new DefaultState();
    public TossGrenadeState TossGrenade = new TossGrenadeState();

    [HideInInspector] public WeaponManager currentWeapon;
    [HideInInspector] public weaponAmmo Ammo;
    [HideInInspector] public Animator anim;

    [Header("UI Reference")]
    public GameObject ammoUIPanel;

    [Header("Dynamic Throw Force Configuration")]
    public float throwForwardForce = 15f;
    public float throwUpwardForce = 5f;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (ammoUIPanel == null)
        {
            ammoUIPanel = GameObject.Find("AmmoDisplay");
        }

        SwitchState(Default);
    }

    void Update()
    {
        // Check if player has a weapon equipped and verify if it's a grenade item type
        if (currentWeapon != null && currentWeapon.gameObject.name.ToLower().Contains("grenade"))
        {
            if (Input.GetMouseButtonDown(0)) // Left Mouse Click
            {
                SwitchState(TossGrenade);
                return; // Exit early to bypass firearm shooting states
            }
        }

        if (currentWeapon == null || Ammo == null)
        {
            if (ammoUIPanel != null && ammoUIPanel.activeSelf)
            {
                ammoUIPanel.SetActive(false);
            }
            return;
        }

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

    // =======================================================================
    // REFACTORED ANIMATION EVENT: Detaches and throws the grenade you are holding!
    // =======================================================================
    public void LaunchGrenadeEvent()
    {
        WeaponSwap swapper = GetComponent<WeaponSwap>();
        if (swapper == null || currentWeapon == null) return;

        GameObject heldGrenade = currentWeapon.gameObject;
        Grenade grenadeScript = heldGrenade.GetComponent<Grenade>();

        // 1. Detach from your hand tracking bone structure
        heldGrenade.transform.SetParent(null);

        // 2. Reactivate physics simulations mid-air
        BoxCollider boxCol = heldGrenade.GetComponent<BoxCollider>();
        if (boxCol != null) boxCol.enabled = true;
        else
        {
            Collider generalCol = heldGrenade.GetComponent<Collider>();
            if (generalCol != null) generalCol.enabled = true;
        }

        // Ensure gravity is enabled and kinematics are disabled on launch
        Rigidbody rb = heldGrenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            Vector3 physicalTrajectory = (transform.forward * throwForwardForce) + (Vector3.up * throwUpwardForce);
            rb.AddForce(physicalTrajectory, ForceMode.Impulse);
        }

        // 3. Start the grenade explosion countdown timer ticking
        if (grenadeScript != null)
        {
            grenadeScript.isThrown = true;
        }

        // =======================================================================
        // CLEANUP: Removes tracking and cascades down to WeaponSwap UI refreshes!
        // =======================================================================
        swapper.equippedWeapons.Remove(heldGrenade);

        // Reset variables before triggering the weapon refresh 
        currentWeapon = null;
        Ammo = null;

        // This automatically recalibrates layout, loops, and updates UI via Step 2 alterations!
        swapper.ForceWeaponRefreshAfterThrow();
    }

    public void WeaponReloaded()
    {
        if (Ammo != null) Ammo.Reload();
        SwitchState(Default);
    }

    public void SetWeapon(WeaponManager weapon)
    {
        currentWeapon = weapon;

        if (weapon != null)
        {
            Ammo = weapon.Ammo;
            // Hide the ammo values UI box text layer if holding a grenade item type
            if (ammoUIPanel != null)
                ammoUIPanel.SetActive(!weapon.gameObject.name.Contains("grenade"));
        }
        else
        {
            Ammo = null;
            if (ammoUIPanel != null) ammoUIPanel.SetActive(false);
        }
    }
}