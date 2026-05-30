using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] float timer = 2f;
    float countdown;

    [Header("Explosion Metrics")]
    public float radius = 10f; 
    public int maxDamage = 100;

    [Header("Modular Pushback Settings")]
    [Tooltip("The raw power of the explosion pushback.")]
    public float pushbackForce = 500f;

    [Tooltip("Applies an upward lift vector to objects, sending them airborne. 0 = flat push away.")]
    public float upwardModifier = 1f;

    [Tooltip("Impulse applies the entire force instantly, ignoring mass. Force applies it gradually based on object weight.")]
    public ForceMode pushbackMode = ForceMode.Impulse;

    bool hasExploded;
    [HideInInspector] public bool isThrown = false;

    [Header("Visual Effects")]
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

        
        if (ExplodingParticle != null)
        {
            GameObject spawnedParticle = Instantiate(ExplodingParticle, transform.position, transform.rotation);
            Destroy(spawnedParticle, 1f);
        }

        
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider nearbyObject in colliders)
        {
            
            float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);

            
            float falloffFactor = 1f - (distance / radius);
            falloffFactor = Mathf.Clamp01(falloffFactor);

            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                
                float finalPushForce = pushbackForce * falloffFactor;

                
                rb.AddExplosionForce(
                    finalPushForce,
                    transform.position,
                    radius,
                    upwardModifier,
                    pushbackMode
                );
            }

            
            EnemyHealth enemy = nearbyObject.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int finalDamage = Mathf.RoundToInt(maxDamage * falloffFactor);
                enemy.TakeDamage(finalDamage);

                
                float finalPushForce = pushbackForce * falloffFactor;
                enemy.ApplyExplosionKnockback(transform.position, finalPushForce, radius, upwardModifier);
            }
        }

        
        Destroy(gameObject);
    }

   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}