using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    float updateTimer;
    float lostTimer;
    const float Interval = 0.2f;
    const float LostGracePeriod = 1.5f;

    public EnemyChaseState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        // FIX: Reset timers every time the enemy starts chasing!
        // This ensures they don't instantly give up because of old memory.
        lostTimer = 0f;
        updateTimer = 0f;

        if (owner != null && owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
        {
            owner.agent.isStopped = false;
        }

        // Trigger the chase animation
        if (owner != null && owner.animator != null)
        {
            owner.animator.Play("Chase", 0, 0f);
        }
    }

    public override void Update()
    {
        float dist = Vector3.Distance(
            owner.transform.position,
            owner.player.position);

        if (dist <= owner.attackRange)
        {
            owner.SwitchState(owner.AttackState);
            return;
        }

        if (dist > owner.loseRange)
        {
            lostTimer += Time.deltaTime;
            if (lostTimer >= LostGracePeriod)
            {
                owner.SwitchState(owner.PatrolState);
                return;
            }
        }
        else
        {
            lostTimer = 0f;
        }

        updateTimer += Time.deltaTime;

        if (owner != null && owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
        {
            owner.agent.SetDestination(owner.player.position);
        }
    }

    public override void Exit()
    {
        if (owner != null && owner.agent != null && owner.agent.isActiveAndEnabled && owner.agent.isOnNavMesh)
        {
            owner.agent.stoppingDistance = owner.waypointStopDist;
            owner.agent.ResetPath();
            owner.agent.velocity = Vector3.zero;
        }
    }
}