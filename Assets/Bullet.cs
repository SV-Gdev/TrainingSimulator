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
        // 1. Check if we hit an Enemy Base Health component
        EnemyHealth enemyHealth = collision.gameObject.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(weapon.damage);

            // ALERT LOGIC: Trigger the enemy state machine to chase
            EnemyStateMachine stateMachine = collision.gameObject.GetComponentInParent<EnemyStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.AlertAndChasePlayer();
            }
        }

        // 2. Check if we hit an armored variant component instead
        EnemyArmor1 enemyArmor1 = collision.gameObject.GetComponentInParent<EnemyArmor1>();
        if (enemyArmor1 != null)
        {
            enemyArmor1.TakeDamage(weapon.damage);

            // ALERT LOGIC: Trigger the armored enemy state machine to chase
            EnemyStateMachine stateMachine = collision.gameObject.GetComponentInParent<EnemyStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.AlertAndChasePlayer();
            }
        }

        // 3. Destroy bullet object cleanly on impact
        Destroy(this.gameObject);
    }
}