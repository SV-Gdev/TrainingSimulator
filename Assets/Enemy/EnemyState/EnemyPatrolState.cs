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

        owner.agent.isStopped = false;
        owner.agent.speed = owner.patrolSpeed;
        owner.agent.stoppingDistance = owner.waypointStopDist;

    
        owner.animator.Play("Patrol", 0, 0f);

        GoToNextWaypoint();
        Debug.Log($"[Patrol] ENTER heading to {currentWaypoint?.name}");
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
            Debug.Log("[Patrol] Patrol duration done — going idle");
            arriving = true;
            owner.SwitchState(owner.IdleState, owner.idleAfterPatrol);
            return;
        }

        
        if (stuckTimer >= StuckInterval)
        {
            float d = currentWaypoint != null
                ? Vector3.Distance(owner.transform.position, currentWaypoint.position)
                : -1f;

            Debug.Log($"[Patrol] distToWaypoint={d:F2} " +
                      $"stopDist={owner.waypointStopDist} " +
                      $"pathPending={owner.agent.pathPending} " +
                      $"hasPath={owner.agent.hasPath} " +
                      $"remainingDist={owner.agent.remainingDistance:F2} " +
                      $"velocity={owner.agent.velocity.magnitude:F2}");
        }

        if (owner.agent.pathPending) return;

        if (currentWaypoint != null)
        {
            float distToWaypoint = Vector3.Distance(
                owner.transform.position,
                currentWaypoint.position
            );

            if (distToWaypoint <= owner.waypointStopDist + 0.3f)
            {
                Debug.Log($"[Patrol] ARRIVED at {currentWaypoint.name} " +
                          $"dist={distToWaypoint:F2} — switching to Idle");
                arriving = true;
                owner.agent.ResetPath();
                owner.agent.velocity = Vector3.zero;
                owner.SwitchState(owner.IdleState, owner.idleAtWaypoint);
                return;
            }
        }

        if (waypointTimer >= owner.waypointTimeout)
        {
            Debug.Log($"[Patrol] Waypoint timeout — picking new waypoint");
            GoToNextWaypoint();
            return;
        }

        if (stuckTimer >= StuckInterval)
        {
            float moved = Vector3.Distance(owner.transform.position, lastPosition);
            if (moved < StuckThreshold)
            {
                Debug.Log($"[Patrol] STUCK detected — picking new waypoint");
                GoToNextWaypoint();
            }
            lastPosition = owner.transform.position;
            stuckTimer = 0f;
        }
    }

    public override void Exit()
    {
        Debug.Log("[Patrol] EXIT");
        arriving = false;
        owner.agent.ResetPath();
    }

    void GoToNextWaypoint()
    {
        Transform next = owner.GetRandomWaypoint(currentWaypoint);
        if (next == null) return;

        currentWaypoint = next;
        owner.agent.SetDestination(currentWaypoint.position);

        // ADD THIS
        Debug.Log($"[Patrol] Going to {currentWaypoint.name} " +
                  $"at {currentWaypoint.position} — " +
                  $"my position={owner.transform.position} — " +
                  $"dist={Vector3.Distance(owner.transform.position, currentWaypoint.position):F2} — " +
                  $"navmesh path status={owner.agent.pathStatus}");

        waypointTimer = 0f;
        stuckTimer = 0f;
        lastPosition = owner.transform.position;
    }
}