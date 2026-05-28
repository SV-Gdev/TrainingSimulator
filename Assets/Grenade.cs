using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] float timer = 2f;
    float countdown;

    [Header("Explosion Metrics")]
    public float radius = 10f; // Outer boundary of the blast sphere
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

        // Spawn explosion visual FX particle layers
        if (ExplodingParticle != null)
        {
            GameObject spawnedParticle = Instantiate(ExplodingParticle, transform.position, transform.rotation);
            Destroy(spawnedParticle, 1f);
        }

        // Scan the area for objects within the customizable blast radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider nearbyObject in colliders)
        {
            // Calculate distance to scale both damage and pushback based on your radius
            float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);

            // Falloff factor: 1 at epicenter, 0 at the absolute edge of the radius
            float falloffFactor = 1f - (distance / radius);
            falloffFactor = Mathf.Clamp01(falloffFactor);

            // ==========================================
            // 1. MODULAR PUSHBACK SYSTEM
            // ==========================================
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Scales pushback power relative to distance from explosion epicenter
                float finalPushForce = pushbackForce * falloffFactor;

                // Unity's native calculation handles direction, radius restrictions, and upward lifting vectors perfectly
                rb.AddExplosionForce(
                    finalPushForce,
                    transform.position,
                    radius,
                    upwardModifier,
                    pushbackMode
                );
            }

            // ==========================================
            // 2. DAMAGE CALCULATION
            // ==========================================
            EnemyHealth enemy = nearbyObject.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                int finalDamage = Mathf.RoundToInt(maxDamage * falloffFactor);
                enemy.TakeDamage(finalDamage);

                // Send the explosion details over to the enemy so they override their NavMesh!
                float finalPushForce = pushbackForce * falloffFactor;
                enemy.ApplyExplosionKnockback(transform.position, finalPushForce, radius, upwardModifier);
            }
        }

        // Remove the physical grenade from the scene
        Destroy(gameObject);
    }

    // Draw the blast circle in the Unity Editor scene tab for easier design debugging
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}