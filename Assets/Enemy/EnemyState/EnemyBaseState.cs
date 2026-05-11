public abstract class EnemyBaseState
{
    // Every state holds a reference back to the owner
    protected EnemyStateMachine owner;

    public EnemyBaseState(EnemyStateMachine owner)
    {
        this.owner = owner;
    }

    public abstract void Enter(float context);
    public abstract void Update();
    public abstract void Exit();
}   