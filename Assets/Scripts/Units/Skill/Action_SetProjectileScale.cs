using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_SetProjectileScale : SkillAction
{

    public override float Activate()
    {
        parent.pv.RPC("SetScale", RpcTarget.AllBuffered,
            (float)GetParam(SkillParams.Width),
            (float)GetParam(SkillParams.Height));
        return 0;
    }
}
