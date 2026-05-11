using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    float attackTimer;
    float attackCooldown = 1.0f;  
    bool isAttacking;

    public EnemyAttackState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        attackTimer = attackCooldown; 
        isAttacking = false;

        owner.agent.ResetPath();
        owner.agent.velocity = Vector3.zero;
        owner.agent.isStopped = true;

        owner.animator.Play("Attack", 0, 0f);
        Debug.Log("[Attack] Enter");
    }

    public override void Update()
    {
        float distToPlayer = Vector3.Distance(
            owner.transform.position,
            owner.player.position);

       
        if (distToPlayer > owner.attackRange)
        {
            owner.SwitchState(owner.ChaseState);
            return;
        }

        
        if (distToPlayer > owner.loseRange)
        {
            owner.SwitchState(owner.IdleState, owner.idleAfterPatrol);
            return;
        }

        
        Vector3 dir = (owner.player.position - owner.transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            owner.transform.rotation = Quaternion.Slerp(
                owner.transform.rotation,
                Quaternion.LookRotation(dir),
                Time.deltaTime * 10f);

        attackTimer += Time.deltaTime;
        if (attackTimer >= attackCooldown)
        {
            attackTimer = 0f;
            PerformAttack();
        }
    }

    public override void Exit()
    {
        isAttacking = false;
        owner.agent.isStopped = false;
    }

    void PerformAttack()
    {
        Debug.Log("[Attack] Hit player");

       
        PlayerHealth health = owner.player.GetComponent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(owner.attackDamage);
    }
}