using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    float timer;
    float idleDuration;
    float chaseCooldown = 1f; 
    float chaseCooldownTimer;

    public EnemyIdleState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        
        if (owner.agent.isActiveAndEnabled)
        {
            owner.agent.ResetPath();
            owner.agent.velocity = Vector3.zero;
            owner.agent.isStopped = true;
        }

        owner.SetAnimator(true, false, false); 
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