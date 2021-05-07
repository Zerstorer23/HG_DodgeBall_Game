using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Player_AddBuff : SkillAction
{
    public override float Activate()
    {
        BuffData buff = (BuffData)GetParam(SkillParams.BuffData);
        parent.caster.GetComponent<BuffManager>().pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)buff.buffType, buff.modifier, buff.duration);
        return 0f;
    }
}
public class Action_Player_InvincibleBuff : SkillAction
{
    public override float Activate()
    {
        if (GameSession.suddenDeathCalled) return 0f;
        float duration = (float)GetParam(SkillParams.Duration);
        parent.caster.GetComponent<BuffManager>().pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)BuffType.InvincibleFromBullets,0f,(double) duration);
        return 0f;
    }
}
public class Action_PlayerChangeSpriteColor : SkillAction
{
    public override float Activate()
    {
        string col = (string)GetParam(SkillParams.Color);
        parent.casterPV.RPC("ChangePortraitColor",Photon.Pun.RpcTarget.AllBuffered,col);
        return 0f;
    }
}
public class Action_PlayerDoGunAnimation : SkillAction
{
    public override float Activate()
    {
        string animTag = (string)GetParam(SkillParams.AnimationTag);
        parent.casterPV.RPC("TriggerGunAnimation", Photon.Pun.RpcTarget.AllBuffered, animTag);
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