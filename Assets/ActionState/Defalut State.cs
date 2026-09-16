using UnityEngine;

public class DefaultState : ActionBase
{
    public override void EnterState(ActionState actions)
    {
        // Setup code if needed when entering Default state
    }

    public override void UpdateState(ActionState actions)
    {
        // Check for Reloading ONLY. 
        // WeaponManager.cs handles all shooting logic automatically!
        if (Input.GetKeyDown(KeyCode.R) && CanReload(actions))
        {
            actions.SwitchState(actions.Reload);
        }
    }

    bool CanReload(ActionState action)
    {
        if (action.Ammo == null) return false;
        if (action.Ammo.currentAmmo == action.Ammo.clipSize) return false;
        if (action.Ammo.extraAmmo == 0) return false;
        return true;
    }
}