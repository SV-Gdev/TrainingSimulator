using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkState : MovementBase
{
    public override void EnterState(PlayerMovement movement)
    {
        movement.anim.SetBool("Walking",true);
        Debug.Log("moving");
    }
    public override void UpdateState(PlayerMovement movement)
    {

    }
}
