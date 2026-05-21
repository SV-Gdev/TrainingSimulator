using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] WeaponData weaponData;

    // References to your managers
    WeaponClassManager weaponClass;
    [HideInInspector] public WeaponSwap weaponSwap; // NEW: Added WeaponSwap reference

    float fireRateTimer;
    [SerializeField] bool SemiAuto;

    [Header("Bullet Properties")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform BarrelPos;

    AimManage aim;
    public float damage = 20;

    [SerializeField] AudioClip GunShot;
    AudioSource audioSource;
    [HideInInspector] public weaponAmmo Ammo;

    public Transform leftHandTarget, leftHandHint;

    void Start()
    {
        aim = GetComponentInParent<AimManage>();
        fireRateTimer = weaponData.firerate;
    }

    private void OnEnable()
    {
        // 1. Grab components from the parent player hierarchy cleanly
        if (weaponClass == null) weaponClass = GetComponentInParent<WeaponClassManager>();
        if (weaponSwap == null) weaponSwap = GetComponentInParent<WeaponSwap>();
        if (Ammo == null) Ammo = GetComponent<weaponAmmo>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // 2. NEW AUTO-REFS FOR CLONES: Find scene objects that aren't on the prefab
        if (aim == null)
        {
            aim = GetComponentInParent<AimManage>();
            // If it's still null, look globally on your player character
            if (aim == null) aim = GameObject.FindAnyObjectByType<AimManage>();
        }

        // 3. Force weaponAmmo to find the UI text box right now
        if (Ammo != null && Ammo.ammoDisplay == null)
        {
            GameObject displayObj = GameObject.Find("AmmoDisplay");
            if (displayObj != null)
            {
                Ammo.ammoDisplay = displayObj.GetComponent<TMPro.TextMeshProUGUI>();
            }
        }

        // 4. Safety Check execution for WeaponClassManager
        if (weaponClass != null)
        {
            weaponClass.SelectCurrentWeapon(this);
        }

        // 5. Force a UI update now that all links are restored!
        if (Ammo != null)
        {
            // Call the public update directly to push the new numbers to your HUD
            Ammo.UpdateAmmoUI();
        }
    }

    void Update()
    {
        if (ShouldFire()) Fire();
    }

    bool ShouldFire()
    {
        // 1. NEW SAFETY GUARD: If this weapon is NOT active in the world hierarchy, stop immediately!
        if (!gameObject.activeInHierarchy) return false;

        // 2. NEW INVENTORY GUARD: Double-check with WeaponSwap to make sure we are allowed to shoot
        if (weaponSwap == null || weaponSwap.equippedWeapons.Count == 0) return false;

        fireRateTimer += Time.deltaTime;

        if (fireRateTimer < weaponData.firerate) return false;
        if (Ammo != null && Ammo.currentAmmo == 0) return false;

        if (SemiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
        if (!SemiAuto && Input.GetKey(KeyCode.Mouse0)) return true;

        return false;
    }

    void Fire()
    {
        fireRateTimer = 0;

        if (aim != null)
        {
            BarrelPos.LookAt(aim.aimPos);
        }

        if (audioSource != null && GunShot != null)
        {
            audioSource.PlayOneShot(GunShot);
        }

        if (Ammo != null)
        {
            Ammo.UseAmmo(1);
        }

        for (int i = 0; i < weaponData.BulletperShot; i++)
        {
            GameObject currentBullet = Instantiate(bullet, BarrelPos.position, BarrelPos.rotation);
            Bullet bulletScript = currentBullet.GetComponent<Bullet>();

            if (bulletScript != null)
            {
                bulletScript.weapon = this;
            }

            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(BarrelPos.forward * weaponData.BulletVeclocity, ForceMode.Impulse);
            }
        }
    }
}