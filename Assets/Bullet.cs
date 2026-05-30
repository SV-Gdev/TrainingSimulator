using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float TimeToDestroy;
    [HideInInspector] public WeaponManager weapon;

    void Start()
    {
        Destroy(this.gameObject, TimeToDestroy);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        EnemyHealth enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(weapon.damage);

            
            EnemyStateMachine stateMachine = collision.gameObject.GetComponentInParent<EnemyStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.AlertAndChasePlayer();
            }
        }

       
        EnemyArmor1 enemyArmor1 = collision.gameObject.GetComponentInParent<EnemyArmor1>();
        if (enemyArmor1 != null)
        {
            enemyArmor1.TakeDamage(weapon.damage);

          
            EnemyStateMachine stateMachine = collision.gameObject.GetComponentInParent<EnemyStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.AlertAndChasePlayer();
            }
        }

       
        Destroy(this.gameObject);
    }
}