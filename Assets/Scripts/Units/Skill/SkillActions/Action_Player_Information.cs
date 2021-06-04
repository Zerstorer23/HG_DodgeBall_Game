using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Action_PlayerChangeSpriteColor : SkillAction
{
    public override float Activate()
    {
        string col = GetParam<string>(SkillParams.Color);
        parent.casterPV.RPC("ChangePortraitColor", RpcTarget.AllBuffered,col);
        return 0f;
    }
}
public class Action_PlayerDoGunAnimation : SkillAction
{
    public override float Activate()
    {
        string animTag = GetParam<string>(SkillParams.AnimationTag);
        parent.casterPV.RPC("TriggerGunAnimation", RpcTarget.AllBuffered, animTag);
        return 0f;
    }
}
public class Action_Player_SetColliderSize : SkillAction
{
    public override float Activate()
    {
        float size = GetParam<float>(SkillParams.Width);
        parent.casterPV.RPC("SetBodySize", RpcTarget.AllBuffered, size);
        return 0f;
    }
}
public class Action_Player_Suicide : SkillAction
{
    public override float Activate()
    {
        parent.castingPlayer.health.Kill_Immediate();
        return 0f;
    }
}
public class Action_GunObject_SetAngle : SkillAction
{
    public override float Activate()
    {
        float euler = GetParam<float>(SkillParams.EulerAngle);
        parent.casterPV.RPC("SetGunAngle", Photon.Pun.RpcTarget.AllBuffered, euler);
        return 0f;
    }
}