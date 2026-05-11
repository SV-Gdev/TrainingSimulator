using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    float timer;
    float idleDuration;
    float chaseCooldown = 1f; // must be idle for this long before chase can trigger
    float chaseCooldownTimer;

    public EnemyIdleState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        timer = 0f;
        chaseCooldownTimer = 0f;
        idleDuration = context > 0f ? context : owner.idleAfterPatrol;

        owner.agent.ResetPath();
        owner.agent.velocity = Vector3.zero;
        owner.agent.isStopped = true;

        owner.animator.Play("Idle", 0, 0f);
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        chaseCooldownTimer += Time.deltaTime;

       
        if (chaseCooldownTimer >= chaseCooldown &&
            owner.PlayerInRange(owner.chaseRange))
        {
            owner.agent.isStopped = false;
            owner.SwitchState(owner.ChaseState);
            return;
        }

        if (timer >= idleDuration)
        {
            owner.agent.isStopped = false;
            owner.SwitchState(owner.PatrolState);
        }
    }

    public override void Exit()
    {
        owner.agent.isStopped = false;
    }
}