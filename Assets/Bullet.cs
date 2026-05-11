using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float TimeToDestroy;
    [HideInInspector] public WeaponManager weapon;
   
    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, TimeToDestroy);
    }

    // Update is called once per frame
   

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<EnemyHealth>())
        {
            EnemyHealth enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
            enemyHealth.TakeDamage(weapon.damage);
        }
        Destroy(this.gameObject);
        if (collision.gameObject.GetComponentInParent<EnemyArmor1>())
        {
            EnemyArmor1 enemyArmor1 = collision.gameObject.GetComponentInParent<EnemyArmor1>();
            enemyArmor1.TakeDamage(weapon.damage);
        }
        Destroy(this.gameObject);

    }
}
