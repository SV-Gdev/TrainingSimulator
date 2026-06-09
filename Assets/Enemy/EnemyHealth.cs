using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System; 

public class EnemyHealth : MonoBehaviour
{
   
    public event Action<GameObject> OnEnemyDeath;

   
    public int currentHealth = 100;
    private bool isDead = false;
    private NavMeshAgent agent;
    private Rigidbody rb;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
    }

   
    public void TakeDamage(float damage)
    {
        if (isDead) return;

       
        currentHealth -= Mathf.RoundToInt(damage);

        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    
    private void Die()
    {
        isDead = true;

        
        OnEnemyDeath?.Invoke(gameObject);

       
        
    }

   
    public void ResetHealth(float newHealth)
    {
        currentHealth = Mathf.RoundToInt(newHealth);
        isDead = false; 
       
    }

   
    public void ApplyExplosionKnockback(Vector3 explosionPosition, float force, float radius, float upwardModifier)
    {
        if (agent != null)
        {
           
            agent.enabled = false;
        }

        if (rb != null)
        {
           
            rb.isKinematic = false;

           
            rb.AddExplosionForce(force, explosionPosition, radius, upwardModifier, ForceMode.Impulse);
        }

        
        StopAllCoroutines();
        StartCoroutine(RestoreNavigationRoutine());
    }

    private IEnumerator RestoreNavigationRoutine()
    {
       
        yield return new WaitForSeconds(0.8f);

        
        if (rb != null)
        {
            while (rb.velocity.magnitude > 0.2f)
            {
                yield return null;
            }

            
            rb.isKinematic = true;
        }

       
        if (agent != null)
        {
            agent.enabled = true;
        }
    }
}