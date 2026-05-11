using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadState : ActionBase
{
    public override void EnterState(ActionState actions)
    {
        actions.anim.SetTrigger("Reload");
        
    }
    public override void UpdateState(ActionState actions)
    {

    }
}
