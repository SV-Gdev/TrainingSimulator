using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System; // REQUIRED: Added for C# Action events

public class EnemyHealth : MonoBehaviour
{
    // =======================================================================
    // THE OBSERVER PATTERN EVENT (BROADCASTER)
    // Passes: (GameObject enemyObject) so the ScoreManager knows who died
    // =======================================================================
    public event Action<GameObject> OnEnemyDeath;

    // ==========================================
    // YOUR HEALTH VARIABLES
    // ==========================================
    public int currentHealth = 100;
    private bool isDead = false; // Prevents the death logic/event from firing multiple times

    private NavMeshAgent agent;
    private Rigidbody rb;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

    // ==========================================
    // FIXED: Changed parameter to 'float' to match weapon.damage
    // ==========================================
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        // Rounds the incoming float damage to an int before subtracting it
        currentHealth -= Mathf.RoundToInt(damage);

        // CHECK FOR DEATH CONDITION
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // =======================================================================
    // NEW: Centralized Death Logic that Shouts the Event
    // =======================================================================
    private void Die()
    {
        isDead = true;

        // Broadcast the death event safely to any listening script (like ScoreManager)
        OnEnemyDeath?.Invoke(gameObject);

        Debug.Log($"[EnemyHealth] {gameObject.name} has died! Death event broadcasted.");

        // If you have existing death handling code, put it right here:
        // gameObject.SetActive(false); 
    }

    // =======================================================================
    // NEW: Function to reset health called by your Respawn Helper
    // =======================================================================
    public void ResetHealth(float newHealth)
    {
        currentHealth = Mathf.RoundToInt(newHealth);
        isDead = false; // Reset the death tracking state so they can die again upon respawn
        Debug.Log($"[EnemyHealth] Character revived. Health reset to: {currentHealth}");
    }

    // =======================================================================
    // NEW: Call this from the Grenade script to handle the physical blast knockback
    // =======================================================================
    public void ApplyExplosionKnockback(Vector3 explosionPosition, float force, float radius, float upwardModifier)
    {
        if (agent != null)
        {
            // 1. Temporarily disable the NavMeshAgent so physics takes over
            agent.enabled = false;
        }

        if (rb != null)
        {
            // 2. Ensure the Rigidbody isn't kinematic so it can move freely
            rb.isKinematic = false;

            // 3. Apply the custom modular explosion force
            rb.AddExplosionForce(force, explosionPosition, radius, upwardModifier, ForceMode.Impulse);
        }

        // 4. Start a coroutine to turn the NavMeshAgent back on safely after flying
        StopAllCoroutines();
        StartCoroutine(RestoreNavigationRoutine());
    }

    private IEnumerator RestoreNavigationRoutine()
    {
        // Wait briefly while the enemy is airborne/sliding in the blast wave
        yield return new WaitForSeconds(0.8f);

        // Wait until the enemy settles and stops moving fast
        if (rb != null)
        {
            while (rb.velocity.magnitude > 0.2f)
            {
                yield return null;
            }

            // Re-lock physics
            rb.isKinematic = true;
        }

        // Re-enable navigation so the enemy can chase you again!
        if (agent != null)
        {
            agent.enabled = true;
        }
    }
}