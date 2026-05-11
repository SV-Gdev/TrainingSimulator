using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    float timer;
    float idleDuration;

    public EnemyIdleState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        timer = 0f;
        idleDuration = context > 0f ? context : owner.idleAfterPatrol;

        owner.agent.ResetPath();
        owner.agent.velocity = Vector3.zero; // Force physical stop
        owner.agent.isStopped = true;

        // Use CrossFade for a smooth 0.2s transition using the Hash
        // This is much more reliable than Play("String")
        owner.animator.CrossFade(EnemyStateMachine.HashIdle, 0.2f);

        Debug.Log($"[Idle] Enter: Animation should now be blending to Idle.");
    }

    public override void Update()
    {
        if (owner.PlayerInRange(owner.chaseRange))
        {
            owner.agent.isStopped = false;
            owner.SwitchState(owner.ChaseState);
            return;
        }

        timer += Time.deltaTime;
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