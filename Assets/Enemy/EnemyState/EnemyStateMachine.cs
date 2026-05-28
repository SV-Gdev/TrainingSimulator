using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.UI.GridLayoutGroup;

public class EnemyStateMachine : MonoBehaviour
{
    [Header("Owner References")]
    public NavMeshAgent agent;
    public Animator animator;
    public Transform player;

    [Header("Waypoints")]
    public Transform[] waypointTransforms;

    [Header("Modular Attack Visual FX")]
    [Tooltip("Drag the hit/slash/punch particle prefab here from your Project assets.")]
    public GameObject punchParticlePrefab;

    [Tooltip("Drag the enemy's hand/fist bone transform here to spawn the effect right at the fist. If left empty, it spawms at the enemy's chest position.")]
    public Transform punchSpawnPoint;

    [Header("Settings")]
    public float patrolSpeed = 1.5f;
    public float chaseSpeed = 3.5f;
    public float chaseRange = 20f;
    public float loseRange = 8f;
    public float idleAtWaypoint = 2f;
    public float idleAfterPatrol = 5f;
    public float waypointStopDist = 1.5f;
    public float waypointTimeout = 15f;
    public float patrolDuration = 60f;
    public float attackRange = 2f;
    public int attackDamage = 10;

    // Added only this property to track state lock safely
    public bool IsCurrentlyAttacking { get; set; } = false;

    public EnemyIdleState IdleState { get; private set; }
    public EnemyPatrolState PatrolState { get; private set; }
    public EnemyChaseState ChaseState { get; private set; }
    public EnemyAttackState AttackState { get; private set; }

    EnemyBaseState currentState;
    private object owner;

    public static readonly int HashIdle = Animator.StringToHash("Idle");
    public static readonly int HashPatrol = Animator.StringToHash("Patrol");
    public static readonly int HashChase = Animator.StringToHash("Chase");
    public static readonly int HashAttack = Animator.StringToHash("Attack");

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
                Debug.LogError($"[StateMachine] {name}: No Animator found!");
        }

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        Debug.Log($"[StateMachine] Animator on: {animator.gameObject.name}");

        IdleState = new EnemyIdleState(this);
        PatrolState = new EnemyPatrolState(this);
        ChaseState = new EnemyChaseState(this);
        AttackState = new EnemyAttackState(this);
    }

    void Start()
    {
        agent.stoppingDistance = waypointStopDist;
        agent.autoBraking = true;
        SwitchState(IdleState, idleAfterPatrol);
    }

    void Update()
    {
        currentState?.Update();
    }

    public void SwitchState(EnemyBaseState newState, float context = 0f)
    {
        // Don't interrupt mid-punch sequence
        if (IsCurrentlyAttacking && newState != AttackState) return;

        currentState?.Exit();
        currentState = newState;
        currentState.Enter(context);
    }
    public void EnemyPunchDamageEvent()
    {
        // 1. Handle Visual Effects Modularly
        if (punchParticlePrefab != null)
        {
            // Fallback to transform position if no fist transform bone anchor is assigned
            Vector3 spawnPosition = punchSpawnPoint != null ? punchSpawnPoint.position : transform.position + Vector3.up;
            Quaternion spawnRotation = punchSpawnPoint != null ? punchSpawnPoint.rotation : transform.rotation;

            // Spawn the modular asset
            GameObject spawnedFX = Instantiate(punchParticlePrefab, spawnPosition, spawnRotation);

            // Clean it out of memory after 1.5 seconds automatically
            Destroy(spawnedFX, 1.5f);
        }

        // 2. Handle Damage Calculations (Only if actively tracking inside AttackState)
        if (IsCurrentlyAttacking && player != null)
        {
            float currentDist = Vector3.Distance(transform.position, player.position);

            if (currentDist <= attackRange + 0.5f)
            {
                PlayerHealth health = player.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(attackDamage);
                    Debug.Log($"[Animation Event] Strike landed! Player hit for {attackDamage} damage.");
                }
            }
        }
    }

    public bool PlayerInRange(float range) =>
        player != null &&
        Vector3.Distance(transform.position, player.position) <= range;

    public void SetAnimator(bool idle, bool patrol, bool chase)
    {
        if (idle)
            animator.Play("Idle", 0, 0f);
        else if (patrol)
            animator.Play("Patrol", 0, 0f);
        else if (chase)
            animator.Play("Chase", 0, 0f);
    }

    public void AlertAndChasePlayer()
    {
        if (currentState == ChaseState || currentState == AttackState) return;

        Debug.Log($"[StateMachine] {name} was hit! Forcing ChaseState.");
        SwitchState(ChaseState);
    }

    public Transform GetRandomWaypoint(Transform last)
    {
        if (waypointTransforms.Length == 0) return null;
        if (waypointTransforms.Length == 1) return waypointTransforms[0];

        Transform next;
        int attempts = 0;
        do
        {
            next = waypointTransforms[Random.Range(0, waypointTransforms.Length)];
            attempts++;
        }
        while (next == last && attempts < 10);

        return next;
    }
}