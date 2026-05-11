using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    float updateTimer;
    const float Interval = 0.2f;

    public EnemyChaseState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        updateTimer = 0f;
        owner.agent.isStopped = false;
        owner.agent.speed = owner.chaseSpeed;
        owner.agent.stoppingDistance = owner.attackRange; 
        owner.animator.Play("Chase", 0, 0f);
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
            owner.SwitchState(owner.IdleState, 1.5f);
            return;
        }

        
        updateTimer += Time.deltaTime;
        if (updateTimer >= Interval)
        {
            owner.agent.SetDestination(owner.player.position);
            updateTimer = 0f;
        }
    }

    public override void Exit()
    {
        owner.agent.stoppingDistance = owner.waypointStopDist;
        owner.agent.ResetPath();
        owner.agent.velocity = Vector3.zero;
    }
}