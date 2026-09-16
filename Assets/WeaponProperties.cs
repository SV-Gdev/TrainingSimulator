using UnityEngine;

public enum WeaponCategory
{
    Firearm,
    Throwable,
    Melee
}

public class WeaponProperties : MonoBehaviour
{
    [Header("Weapon Identity")]
    public WeaponCategory category = WeaponCategory.Firearm; // Changed to lowercase 'category'

    [Header("Weapon Data")]
    public WeaponData weaponData; // Changed from 'Object' to 'WeaponData' so C# recognizes .firerate
    public bool SemiAuto;
    public float damage = 20;
    public float firerate = 0.2f; // Added direct firerate field as well

    [Header("Firearm Properties")]
    public Transform BarrelPos;

    [Header("Throwable Properties")]
    public GameObject throwablePrefab;
    public float throwForce = 15f;
    public float throwUpwardForce = 5f;

    [Header("Shared Prefabs & Audio")]
    public GameObject bulletPrefab;
    public AudioClip GunShot;
    public AudioSource audioSource;

    [Header("IK Hands Positioning")]
    public Transform leftHandTarget;
    public Transform leftHandHint;

    [Header("Components")]
    public weaponAmmo Ammo;
}