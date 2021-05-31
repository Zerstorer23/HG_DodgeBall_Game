using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Action_PlayerChangeSpriteColor : SkillAction
{
    public override float Activate()
    {
        string col = (string)GetParam(SkillParams.Color);
        parent.casterPV.RPC("ChangePortraitColor", RpcTarget.AllBuffered,col);
        return 0f;
    }
}
public class Action_PlayerDoGunAnimation : SkillAction
{
    public override float Activate()
    {
        string animTag = (string)GetParam(SkillParams.AnimationTag);
        parent.casterPV.RPC("TriggerGunAnimation", RpcTarget.AllBuffered, animTag);
        return 0f;
    }
}
public class Action_Player_SetColliderSize : SkillAction
{
    public override float Activate()
    {
        float size = (float)GetParam(SkillParams.Width);
        parent.casterPV.RPC("SetBodySize", RpcTarget.AllBuffered, size);
        return 0f;
    }
}
public class Action_GunObject_SetAngle : SkillAction
{
    public override float Activate()
    {
        float euler = (float)GetParam(SkillParams.EulerAngle);
        parent.casterPV.RPC("SetGunAngle", Photon.Pun.RpcTarget.AllBuffered, euler);
        return 0f;
    }
}