using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public static event Action<int, int> OnAmmoChanged;

    [Header("Systems")]
    public Animator animator;
    private WeaponClassManager weaponClass;
    [HideInInspector] public WeaponSwap weaponSwap;
    private AimManage aim;

    [Header("Weapons")]
    public List<WeaponProperties> weapons = new List<WeaponProperties>();

    [HideInInspector] public WeaponProperties currentWeapon;
    private float fireRateTimer;
    private int currentIndex = 0;

    
    public float damage => currentWeapon != null ? currentWeapon.damage : 0f;
    public weaponAmmo Ammo => currentWeapon != null ? currentWeapon.Ammo : null;
    public Transform leftHandTarget => currentWeapon != null ? currentWeapon.leftHandTarget : null;
    public Transform leftHandHint => currentWeapon != null ? currentWeapon.leftHandHint : null;

    void Start()
    {
        aim = GetComponent<AimManage>();
        weaponClass = GetComponent<WeaponClassManager>();
        weaponSwap = GetComponent<WeaponSwap>();

        if (animator == null)
        {
            animator = GetComponentInParent<Animator>();
        }
    }

    void Update()
    {
        if (currentWeapon == null) return;
        if (ShouldFire()) Fire();
    }

    public void SetCurrentWeapon(WeaponProperties weapon)
    {
        if (weapon == null)
        {
            currentWeapon = null;
            return;
        }

        currentWeapon = weapon;

       
        Rigidbody handRb = currentWeapon.GetComponent<Rigidbody>();
        if (handRb != null)
        {
            handRb.isKinematic = true;  
            handRb.useGravity = false;
        }

        if (currentWeapon.weaponData != null)
        {
            fireRateTimer = currentWeapon.weaponData.firerate;
        }

        if (weaponClass != null)
            weaponClass.SelectCurrentWeapon(this);

        TriggerAmmoUIUpdate();
       
    }

    public void TriggerAmmoUIUpdate()
    {
        if (Ammo != null)
            OnAmmoChanged?.Invoke(Ammo.currentAmmo, Ammo.extraAmmo);
    }

    bool ShouldFire()
    {
        if (currentWeapon == null) return false;

        
        if (currentWeapon.category == WeaponCategory.Throwable)
        {
            return Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (currentWeapon.weaponData == null) return false;

        fireRateTimer += Time.deltaTime;
        if (fireRateTimer < currentWeapon.weaponData.firerate) return false;
        if (Ammo != null && Ammo.currentAmmo == 0) return false;

        if (currentWeapon.SemiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
        if (!currentWeapon.SemiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
        return false;
    }

    void Fire()
    {
        fireRateTimer = 0;

        if (currentWeapon.audioSource != null && currentWeapon.GunShot != null)
            currentWeapon.audioSource.PlayOneShot(currentWeapon.GunShot);

        if (Ammo != null)
        {
            Ammo.UseAmmo(1);
            OnAmmoChanged?.Invoke(Ammo.currentAmmo, Ammo.extraAmmo);
        }

        if (currentWeapon.category == WeaponCategory.Firearm)
            HandleFirearm();
        else if (currentWeapon.category == WeaponCategory.Throwable)
            HandleThrowable();
    }

    void HandleFirearm()
    {
        if (currentWeapon.BarrelPos == null)
        {
            Transform[] allChildren = currentWeapon.GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                if (child.CompareTag("Barrel"))
                {
                    currentWeapon.BarrelPos = child;
                    Debug.Log("Successfully auto-detected barrel on: " + currentWeapon.name);
                    break;
                }
            }

            if (currentWeapon.BarrelPos == null)
            {
                Debug.LogError("[WeaponManager] BarrelPos not found on " + currentWeapon.name + ". Make sure the child barrel is tagged as 'Barrel'!");
                return;
            }
        }

        Vector3 exactAimDirection = currentWeapon.BarrelPos.forward;

        if (aim != null && aim.aimPos != null)
        {
            exactAimDirection = (aim.aimPos.position - currentWeapon.BarrelPos.position).normalized;
        }

        for (int i = 0; i < currentWeapon.weaponData.BulletperShot; i++)
        {
            GameObject go = Instantiate(
                currentWeapon.bulletPrefab,
                currentWeapon.BarrelPos.position,
                Quaternion.LookRotation(exactAimDirection));

            Bullet b = go.GetComponent<Bullet>();
            if (b != null) b.weapon = this;

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(exactAimDirection * currentWeapon.weaponData.BulletVeclocity, ForceMode.Impulse);
            }
        }
    }

    public bool IsGrenadeInHand()
    {
        return currentWeapon != null
            && currentWeapon.category == WeaponCategory.Throwable
            && currentWeapon.gameObject != null
            && currentWeapon.gameObject.activeInHierarchy;
    }

    void HandleThrowable()
    {
        
        if (!IsGrenadeInHand())
        {
            Debug.LogWarning("[WeaponManager] Grenade is not in hand! Cancelling animation.");
            return;
        }

        if (currentWeapon.throwablePrefab == null)
        {
            Debug.LogError("[WeaponManager] Throwable Prefab is missing!");
            return;
        }

        
        if (animator != null)
        {
            animator.SetTrigger("Toss");
        }
    }

    
    public void LaunchGrenade()
    {
        if (!IsGrenadeInHand()) return;
        if (currentWeapon.throwablePrefab == null) return;

        Transform camTrans = Camera.main != null ? Camera.main.transform : transform;

        Vector3 spawnPos = (currentWeapon.BarrelPos != null)
            ? currentWeapon.BarrelPos.position
            : camTrans.position + (camTrans.forward * 0.8f);

        
        GameObject go = Instantiate(currentWeapon.throwablePrefab, spawnPos, camTrans.rotation);

       
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

            float fwdForce = currentWeapon.throwForce > 0 ? currentWeapon.throwForce : 15f;
            float upForce = currentWeapon.throwUpwardForce > 0 ? currentWeapon.throwUpwardForce : 5f;

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
            Debug.LogWarning("Grenade script is missing on the throwable prefab!");
        }

       
        if (weaponSwap != null)
        {
            weaponSwap.ConsumeCurrentThrowable();
        }
    }
}