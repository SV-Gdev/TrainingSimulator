using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    float timer = 2;
    float countdown;
    float radius = 10f; // Adjusted to a realistic game distance unit (10 meters)
    float force = 500;

    // ==========================================
    // NEW: Configuration for explosive damage
    // ==========================================
    public int maxDamage = 100;

    bool hasExploded;
    [HideInInspector] public bool isThrown = false;

    [SerializeField] GameObject ExplodingParticle;

    void Start()
    {
        countdown = timer;
    }

    void Update()
    {
        if (!isThrown) return;

        countdown -= Time.deltaTime;
        if (countdown <= 0 && !hasExploded)
        {
            Explode();
        }
    }

    public void Explode()
    {
        hasExploded = true;

        // Spawn explosion visual FX particle layers
        GameObject spawnedParticle = Instantiate(ExplodingParticle, transform.position, transform.rotation);
        Destroy(spawnedParticle, 1);

        // Scan the area for objects within the blast radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider nearbyObject in colliders)
        {
            // 1. APPLY PHYSICS BLAST FORCE
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(force, transform.position, radius);
            }

            // =======================================================================
            // 2. APPLY DAMAGE TO ENEMIES
            // Looks for your enemy script component and applies a falloff damage calculations
            // =======================================================================
            EnemyHealth enemy = nearbyObject.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                // Calculate distance to scale damage (closer = more damage)
                float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
                float damagePercentage = 1f - (distance / radius);
                int finalDamage = Mathf.RoundToInt(maxDamage * Mathf.Clamp01(damagePercentage));

                // Send the damage value to the enemy!
                enemy.TakeDamage(finalDamage);
            }
        }

        // Remove the physical grenade from the scene
        Destroy(gameObject);
    }
}