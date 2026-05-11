using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float Health;
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float respawnDelay = 2f;
    [SerializeField] int scoreValue = 10;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }
    public void TakeDamage(float damage)
    {
        if (Health > 0)
        {
            Health -= damage;
            Debug.Log("hit");
            if (Health <= 0)
            {
                EnemyDeath();
            }
        }
    }
    void EnemyDeath()
    {
        Debug.Log("Death");
        ScoreManager.Instance.AddScore(scoreValue);
        RespawnHelper.Instance.Respawn(
             gameObject,
             spawnPosition,
             spawnRotation,
              respawnDelay,
             maxHealth,
             this
         );
        gameObject.SetActive(false);
    }
    public void ResetHealth(float value)
    {
        Health = value;
    }
}