using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Data")]
    [SerializeField] WeaponData weaponData;
    WeaponClassManager weaponClass;

    float fireRateTimer;
    [SerializeField] bool SemiAuto;

    [Header("Bullet Properties")]
    [SerializeField] GameObject bullet;
    [SerializeField] Transform BarrelPos;

    AimManage aim;
    public float damage = 20;

    [SerializeField] AudioClip GunShot;
    AudioSource audioSource;
    [HideInInspector]public  weaponAmmo Ammo;

    public Transform leftHandTarget, leftHandHint;
    // Start is called before the first frame update
    void Start()
    {
        
        aim = GetComponentInParent<AimManage>();
       
        fireRateTimer = weaponData.firerate; 

    }
    private void OnEnable()
    {
        if (weaponClass == null)
        {
            weaponClass = GetComponentInParent<WeaponClassManager>();
            Ammo = GetComponent<weaponAmmo>();
            audioSource = GetComponent<AudioSource>();
        }
        weaponClass.SelectCurrentWeapon(this);
    }

    // Update is called once per frame
    void Update()
    {
        if (ShouldFire()) Fire();
       
    }

    bool ShouldFire()
    {
        fireRateTimer += Time.deltaTime;
        if (fireRateTimer < weaponData.firerate) return false;
        if (Ammo.currentammo ==0 ) return false;
        if (SemiAuto && Input.GetKeyDown(KeyCode.Mouse0)) return true;
        if (!SemiAuto && Input.GetKey(KeyCode.Mouse0)) return true;
        return false;

    }
    
    void Fire()
    {
        fireRateTimer = 0;
        BarrelPos.LookAt(aim.aimPos);
       
        audioSource.PlayOneShot(GunShot);
        Ammo.currentammo--;
        for (int i = 0; i < weaponData.BulletperShot; i++)
        {
            GameObject currentBullet = Instantiate(bullet, BarrelPos.position, BarrelPos.rotation);

            Bullet bulletScript = currentBullet.GetComponent<Bullet>();
            bulletScript.weapon = this;

            Rigidbody rb = currentBullet.GetComponent<Rigidbody>();
            rb.AddForce(BarrelPos.forward * weaponData.BulletVeclocity, ForceMode.Impulse);

        }
    }
}
