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
        // 1. Grab all necessary components from the parent (Player) or this weapon
        if (weaponClass == null) weaponClass = GetComponentInParent<WeaponClassManager>();
        if (weaponSwap == null) weaponSwap = GetComponentInParent<WeaponSwap>(); // NEW: Grabs WeaponSwap

        if (Ammo == null) Ammo = GetComponent<weaponAmmo>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // 2. Safety Check for WeaponClassManager to prevent the crash you saw earlier!
        if (weaponClass != null)
        {
            weaponClass.SelectCurrentWeapon(this);
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": Cannot find WeaponClassManager on the parent!");
        }

        // 3. Safety Check for WeaponSwap
        if (weaponSwap == null)
        {
            Debug.LogWarning(gameObject.name + ": Cannot find WeaponSwap script on the parent!");
        }
    }

    void Update()
    {
        if (ShouldFire()) Fire();
    }

    bool ShouldFire()
    {
        fireRateTimer += Time.deltaTime;

        if (fireRateTimer < weaponData.firerate) return false;
        if (Ammo != null && Ammo.currentAmmo == 0) return false; // Added Ammo null check just in case

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