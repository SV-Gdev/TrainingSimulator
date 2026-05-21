using UnityEngine;

public class ReloadState : ActionBase
{
    public override void EnterState(ActionState actions)
    {
        // 1. Force the Shooting Layer (Layer 1) weight to 1 so the animation is visible
        actions.anim.SetLayerWeight(1, 1f);

        // 2. Fire the trigger to make AnyState transition to Reloading
        actions.anim.SetTrigger("Reload");

        Debug.Log("[State Machine] Entered Reload Animation State");
    }

    public override void UpdateState(ActionState actions)
    {
        // Waiting for Animation Event to call WeaponReloaded()
    }
}