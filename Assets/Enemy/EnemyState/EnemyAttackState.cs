using UnityEngine;

public class EnemyAttackState : EnemyBaseState
{
    float attackTimer;
    float attackCooldown = 1.0f;
    bool isAttacking;

    public EnemyAttackState(EnemyStateMachine owner) : base(owner) { }

    public override void Enter(float context)
    {
        
        attackTimer = 0f;
        isAttacking = false;

        owner.agent.ResetPath();
        owner.agent.velocity = Vector3.zero;
        owner.agent.isStopped = true;

        
        owner.IsCurrentlyAttacking = true;

        owner.animator.Play("Attack", 0, 0f);
        Debug.Log("[Attack] Enter");
    }

    public override void Update()
    {
        float distToPlayer = Vector3.Distance(
            owner.transform.position,
            owner.player.position);

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

          
            owner.IsCurrentlyAttacking = false;

         
            if (distToPlayer > owner.loseRange)
            {
                owner.SwitchState(owner.PatrolState);
            }
            else if (distToPlayer > owner.attackRange)
            {
                owner.SwitchState(owner.ChaseState);
            }
        }
    }

    public override void Exit()
    {
        isAttacking = false;
        owner.IsCurrentlyAttacking = false;
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