using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] float timer = 2f;
    float countdown;

    [Header("Explosion Metrics")]
    public float radius = 8f;
    public int maxDamage = 100;

    [Header("Pushback Settings")]
    [Tooltip("The power of the explosion pushback. Try values between 1000 - 2500 for noticeable physics!")]
    public float pushbackForce = 1500f;

    [Tooltip("Applies upward lift sending objects airborne.")]
    public float upwardModifier = 1.5f;

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
            GameObject spawnedParticle = Instantiate(ExplodingParticle, transform.position, Quaternion.identity);
            Destroy(spawnedParticle, 2f);
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius);

        HashSet<Rigidbody> processedRigidbodies = new HashSet<Rigidbody>();
        HashSet<GameObject> processedHitObjects = new HashSet<GameObject>();

        foreach (Collider nearbyObject in colliders)
        {
            float distance = Vector3.Distance(transform.position, nearbyObject.transform.position);
            float falloffFactor = Mathf.Clamp01(1f - (distance / radius));

            // 1. APPLY PHYSICS TO RIGIDBODIES
            Rigidbody rb = nearbyObject.GetComponentInParent<Rigidbody>();

            if (rb != null && !processedRigidbodies.Contains(rb))
            {
                processedRigidbodies.Add(rb);

                if (rb.isKinematic)
                {
                    rb.isKinematic = false;
                }

                float finalPushForce = pushbackForce * falloffFactor;

                rb.AddExplosionForce(
                    finalPushForce,
                    transform.position,
                    radius,
                    upwardModifier,
                    pushbackMode
                );
            }

            // 2. DAMAGE AND KNOCKBACK VIA EnemyHealthManager Singleton
            if (EnemyHealthManager.Instance != null && !processedHitObjects.Contains(nearbyObject.gameObject))
            {
                processedHitObjects.Add(nearbyObject.gameObject);

                int finalDamage = Mathf.RoundToInt(maxDamage * falloffFactor);
                float finalPushForce = pushbackForce * falloffFactor;

                EnemyHealthManager.Instance.TakeDamage(nearbyObject.gameObject, finalDamage);
                EnemyHealthManager.Instance.ApplyExplosionKnockback(transform.position, finalPushForce, radius, upwardModifier, nearbyObject.gameObject);
            }

            // 3. Alert the enemy AI when damaged by explosion
            EnemyStateMachine stateMachine = nearbyObject.GetComponentInParent<EnemyStateMachine>();
            if (stateMachine != null)
            {
                stateMachine.AlertAndChasePlayer();
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