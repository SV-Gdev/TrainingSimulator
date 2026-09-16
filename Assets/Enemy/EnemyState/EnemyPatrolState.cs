using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    float patrolTimer;
    float waypointTimer;
    float stuckTimer;
    Vector3 lastPosition;
    Transform currentWaypoint;
    bool arriving;

    const float StuckInterval = 1f;
    const float StuckThreshold = 0.05f;

    public EnemyPatrolState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        patrolTimer = 0f;
        waypointTimer = 0f;
        stuckTimer = 0f;
        arriving = false;
        currentWaypoint = null;
        lastPosition = owner.transform.position;

        // FIX 1: Safety check before modifying agent properties (Fixes "Resume" error)
        if (owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
        {
            owner.agent.isStopped = false;
            owner.agent.speed = owner.patrolSpeed;
            owner.agent.stoppingDistance = owner.waypointStopDist;
        }

        owner.animator.Play("Patrol", 0, 0f);

        GoToNextWaypoint();
    }

    public override void Update()
    {
        if (arriving) return;

        if (owner.PlayerInRange(owner.chaseRange))
        {
            arriving = true;
            owner.SwitchState(owner.ChaseState);
            return;
        }

        patrolTimer += Time.deltaTime;
        waypointTimer += Time.deltaTime;
        stuckTimer += Time.deltaTime;

        if (patrolTimer >= owner.patrolDuration)
        {
            arriving = true;
            owner.SwitchState(owner.IdleState, owner.idleAfterPatrol);
            return;
        }

        if (stuckTimer >= StuckInterval)
        {
            float d = currentWaypoint != null
                ? Vector3.Distance(owner.transform.position, currentWaypoint.position)
                : -1f;
        }

        // Safety check before checking path status
        if (owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
        {
            if (owner.agent.pathPending) return;
        }

        if (currentWaypoint != null)
        {
            float distToWaypoint = Vector3.Distance(
                owner.transform.position,
                currentWaypoint.position
            );

            if (distToWaypoint <= owner.waypointStopDist + 0.3f)
            {
                arriving = true;

                // FIX 2: Safety check before resetting path (Fixes "ResetPath" error)
                if (owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
                {
                    owner.agent.ResetPath();
                    owner.agent.velocity = Vector3.zero;
                }

                owner.SwitchState(owner.IdleState, owner.idleAtWaypoint);
                return;
            }
        }

        if (waypointTimer >= owner.waypointTimeout)
        {
            GoToNextWaypoint();
            return;
        }

        if (stuckTimer >= StuckInterval)
        {
            float moved = Vector3.Distance(owner.transform.position, lastPosition);
            if (moved < StuckThreshold)
            {
                GoToNextWaypoint();
            }
            lastPosition = owner.transform.position;
            stuckTimer = 0f;
        }
    }

    public override void Exit()
    {
        if (owner != null && owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
        {
            owner.agent.ResetPath();
        }
    }

    void GoToNextWaypoint()
    {
        Transform next = owner.GetRandomWaypoint(currentWaypoint);
        if (next == null) return;

        currentWaypoint = next;

        // FIX 3: Safety check before setting destination (Fixes "SetDestination" error)
        if (owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
        {
            owner.agent.SetDestination(currentWaypoint.position);
        }

        waypointTimer = 0f;
        stuckTimer = 0f;
        lastPosition = owner.transform.position;
    }
}