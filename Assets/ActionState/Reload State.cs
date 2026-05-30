using UnityEngine;

public class ReloadState : ActionBase
{
    public override void EnterState(ActionState actions)
    {
        
        actions.anim.SetLayerWeight(1, 1f);

       
        actions.anim.SetTrigger("Reload");

        Debug.Log("[State Machine] Entered Reload Animation State");
    }

    public override void UpdateState(ActionState actions)
    {
        
    }
}