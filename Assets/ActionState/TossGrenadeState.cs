using UnityEngine;

public class TossGrenadeState : ActionBase
{
    public override void EnterState(ActionState actions)
    {
        // Reset the trigger flag first to clear out any old button presses
        actions.anim.ResetTrigger("Toss");

        // Fire the trigger on the Base Layer!
        actions.anim.SetTrigger("Toss");

        Debug.Log("[State Machine] Entered TossGrenadeState and fired 'Toss' trigger successfully.");
    }

    public override void UpdateState(ActionState actions)
    {
        // Handled cleanly by your Animation Event mid-swing!
    }
}