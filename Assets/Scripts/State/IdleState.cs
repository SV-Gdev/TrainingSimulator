

using UnityEngine;

public class IdleState : MovementBase
{
    public override void EnterState(PlayerMovement movement)
    {
        Debug.Log("Moved");
    }
    public override void UpdateState(PlayerMovement movement)
    {
        if (movement.direction.magnitude > 0.1f)
        {
            movement.SwitchState(movement.Walk);
            Debug.Log("Moved");
        }
    }
}
