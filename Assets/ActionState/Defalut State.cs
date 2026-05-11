using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class DefaultState : ActionBase
{
    public override void EnterState(ActionState actions)
    {
        
    }
    public override void UpdateState(ActionState actions)
    {
        if (Input.GetKeyDown(KeyCode.R)&& CanReload(actions))
        {
            actions.SwitchState(actions.Reload);
        }
    }
    bool CanReload(ActionState action)
    {
        if(action.Ammo.currentammo==action.Ammo.clipsize) return false;
        else if (action.Ammo.ExtraAmmo == 0) return false;
        else return true;
    }
}
