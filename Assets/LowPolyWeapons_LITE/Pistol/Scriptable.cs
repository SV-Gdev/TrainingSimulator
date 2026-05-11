using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Weapon/Weapon Data")]
public class WeaponData : ScriptableObject
{
    public int firerate;
    public bool SemiAuto;
    public int BulletVeclocity;
    public float BulletperShot;
    public float Clipsize;
    public float ExtraAmmo;
    public float CurrentAmmo;
    public Transform bullet;
}