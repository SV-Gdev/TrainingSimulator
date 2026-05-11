using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class EnemyArmor1 : MonoBehaviour
{
    [SerializeField] public float armor;
    [SerializeField] float maxArmor = 50f;
    [SerializeField] float health;
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float respawnDelay = 2f;
    [SerializeField] int scoreValue = 10;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    void Start()
    {
        armor = maxArmor;
        health = maxHealth;
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    public void TakeDamage(float damage)
    {
        if (armor > 0)
        {
            Debug.Log("HIT");
            armor -= damage;
            
            if (armor < 0)
            {
                float remainingDamage = Mathf.Abs(armor);
                armor = 0;
                health -= remainingDamage;
            }
        }
        else
        {
            Debug.Log("HIT");
            health -= damage;
           
        }
        if (health <= 0)
        {
            Debug.Log("Death");
            ScoreManager.Instance.AddScore(scoreValue);
            gameObject.SetActive(false);
            RS1.Instance.StartRespawn(gameObject, spawnPosition, spawnRotation, respawnDelay, maxHealth, maxArmor, this);
        }
    }

    public void ResetHealth(float health, float armor)
    {
        this.health = health;
        this.armor = armor;
    }
}