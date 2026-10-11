using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using System;

public class EnemyHealthManager : MonoBehaviour
{
    // ==========================================
    // SINGLETON INSTANCE
    // ==========================================
    public static EnemyHealthManager Instance { get; private set; }

    // ==========================================
    // CUSTOM ENEMY DATA STRUCTURE FOR INSPECTOR
    // ==========================================
    [System.Serializable]
    public class EnemyTargetData
    {
        [Tooltip("Drag the Enemy GameObject here.")]
        public GameObject enemyGameObject;

        [Tooltip("Check this if this specific enemy has armor.")]
        public bool isArmor = false;

        [Header("Stats")]
        public float maxHealth = 100f;

        [Tooltip("Maximum armor value. ONLY valid when 'Is Armor' is checked above!")]
        public float maxArmor = 50f;

        [Tooltip("Points awarded when this specific enemy dies.")]
        public int scoreValue = 10;

        [Header("Live Runtime Stats (Updates in Play Mode)")]
        [Tooltip("Current health remaining.")]
        public float currentHealth;

        [Tooltip("Current armor remaining.")]
        public float currentArmor;

        // Hidden internal states that don't need inspector clutter
        [HideInInspector] public bool isDead;
        [HideInInspector] public NavMeshAgent agent;
        [HideInInspector] public Rigidbody rb;

        // ADDED: Cache the State Machine so we can alert the enemy
        [HideInInspector] public EnemyStateMachine stateMachine;
    }

    // ==========================================
    // EVENTS 
    // ==========================================
    public event Action OnDeath;
    public static event Action<int> OnEnemyKilled;
    public static event Action<GameObject> OnEnemyDiedObject;

    // ==========================================
    // INSPECTOR ARRAY
    // ==========================================
    [Header("Managed Enemies")]
    [Tooltip("Add as many enemies as you want to this array!")]
    [SerializeField] private List<EnemyTargetData> enemyList = new List<EnemyTargetData>();

    // Fast lookup dictionary for runtime performance
    private Dictionary<GameObject, EnemyTargetData> enemyLookup = new Dictionary<GameObject, EnemyTargetData>();

    void Awake()
    {
        // Setup Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeEnemies();
    }

    private void InitializeEnemies()
    {
        enemyLookup.Clear();

        foreach (var enemy in enemyList)
        {
            if (enemy.enemyGameObject == null) continue;

            enemy.currentHealth = enemy.maxHealth;
            enemy.currentArmor = enemy.isArmor ? Mathf.Max(0f, enemy.maxArmor) : 0f;
            enemy.isDead = false;

            enemy.agent = enemy.enemyGameObject.GetComponent<NavMeshAgent>();
            enemy.rb = enemy.enemyGameObject.GetComponent<Rigidbody>();

            // ADDED: Grab the state machine from the enemy
            enemy.stateMachine = enemy.enemyGameObject.GetComponent<EnemyStateMachine>();

            if (!enemyLookup.ContainsKey(enemy.enemyGameObject))
            {
                enemyLookup.Add(enemy.enemyGameObject, enemy);
            }
        }
    }

    // ==========================================
    // DAMAGE LOGIC
    // ==========================================

    public void TakeDamage(GameObject hitObject, float damage)
    {
        EnemyTargetData enemy = GetEnemyData(hitObject);
        if (enemy == null || enemy.isDead) return;

        ApplyDamage(enemy, damage);
    }

    public void TakeDamage(float damage)
    {
        EnemyTargetData enemy = GetEnemyData(gameObject);

        if (enemy == null)
        {
            enemy = enemyList.Find(e => !e.isDead);
        }

        if (enemy == null || enemy.isDead) return;

        ApplyDamage(enemy, damage);
    }

    private void ApplyDamage(EnemyTargetData enemy, float damage)
    {
        if (enemy.isArmor && enemy.currentArmor > 0)
        {
            enemy.currentArmor -= damage;

            if (enemy.currentArmor < 0)
            {
                float remainingDamage = Mathf.Abs(enemy.currentArmor);
                enemy.currentArmor = 0;
                enemy.currentHealth -= remainingDamage;
            }
        }
        else
        {
            enemy.currentHealth -= damage;
        }

        // ADDED: If the enemy took damage and is still alive, alert them!
        if (enemy.currentHealth > 0 && enemy.stateMachine != null)
        {
            enemy.stateMachine.AlertAndChasePlayer();
        }

        if (enemy.currentHealth <= 0)
        {
            Die(enemy);
        }
    }

    private void Die(EnemyTargetData enemy)
    {
        enemy.isDead = true;

        OnDeath?.Invoke();
        OnEnemyKilled?.Invoke(enemy.scoreValue);
        OnEnemyDiedObject?.Invoke(enemy.enemyGameObject);

        enemy.enemyGameObject.SetActive(false);
    }

    // ==========================================
    // KNOCKBACK LOGIC
    // ==========================================
    public void ApplyExplosionKnockback(Vector3 explosionPosition, float force, float radius, float upwardModifier, GameObject hitObject = null)
    {
        EnemyTargetData enemy = (hitObject != null) ? GetEnemyData(hitObject) : GetEnemyData(gameObject);
        if (enemy == null || enemy.isDead) return;

        if (enemy.agent != null)
        {
            enemy.agent.enabled = false;
        }

        if (enemy.rb != null)
        {
            enemy.rb.isKinematic = false;
            enemy.rb.AddExplosionForce(force, explosionPosition, radius, upwardModifier, ForceMode.Impulse);
        }

        StopAllCoroutines();
        StartCoroutine(RestoreNavigationRoutine(enemy));
    }

    private IEnumerator RestoreNavigationRoutine(EnemyTargetData enemy)
    {
        // Wait for the initial knockback phase
        yield return new WaitForSeconds(0.8f);

        if (enemy.rb != null)
        {
            // Wait until the physics movement settles down
            while (enemy.rb.velocity.magnitude > 0.2f)
            {
                yield return null;
            }

            // Freeze physics forces and make kinematic again
            enemy.rb.velocity = Vector3.zero;
            enemy.rb.angularVelocity = Vector3.zero;
            enemy.rb.isKinematic = true;
        }

        if (enemy.agent != null && !enemy.isDead)
        {
            // 1. Enable the NavMeshAgent
            enemy.agent.enabled = true;

            // 2. Find the nearest valid NavMesh location (searches within a 5-unit radius)
            if (NavMesh.SamplePosition(enemy.enemyGameObject.transform.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
            {
                // Safely snap/warp the enemy back onto the valid NavMesh surface
                enemy.agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning($"[EnemyHealthManager] {enemy.enemyGameObject.name} landed too far off the NavMesh!");
            }
        }
    }

    // ==========================================
    // HELPER LOOKUP
    // ==========================================
    private EnemyTargetData GetEnemyData(GameObject target)
    {
        if (target == null) return null;

        if (enemyLookup.TryGetValue(target, out EnemyTargetData data))
        {
            return data;
        }

        Transform curr = target.transform;
        while (curr != null)
        {
            if (enemyLookup.TryGetValue(curr.gameObject, out data))
            {
                return data;
            }
            curr = curr.parent;
        }

        return null;
    }
}