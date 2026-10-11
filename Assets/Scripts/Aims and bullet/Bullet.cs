using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float timeToDestroy = 3f;
    [HideInInspector] public WeaponManager weapon;

    void Start()
    {
        Destroy(gameObject, timeToDestroy);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"[Bullet] Crashed into: {collision.gameObject.name} | Tag: {collision.gameObject.tag}");

        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        
        if (EnemyHealthManager.Instance != null && weapon != null)
        {
            EnemyHealthManager.Instance.TakeDamage(collision.gameObject, weapon.damage);
        }

      
        EnemyStateMachine stateMachine = collision.gameObject.GetComponentInParent<EnemyStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.AlertAndChasePlayer();
        }

       
        Destroy(gameObject);
    }
}