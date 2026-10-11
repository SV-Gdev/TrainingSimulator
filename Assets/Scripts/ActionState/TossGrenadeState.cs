using UnityEngine;

public class TossGrenadeState : ActionBase
{
    public override void EnterState(ActionState actions)
    {

        actions.anim.ResetTrigger("Toss");


        actions.anim.SetTrigger("Toss");


    }

    public override void UpdateState(ActionState actions)
    {

    }
}