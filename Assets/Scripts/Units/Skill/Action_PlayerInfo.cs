using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_PlayerIncreaseMovespeed : SkillAction
{
    public override float Activate()
    {
        float mod = (float)GetParam(SkillParams.Modifier);
        parent.caster.GetComponent<BuffManager>().speedModifier += mod;
        Debug.Log("Set move speed " + mod);
        return 0f;
    }
}
public class Action_PlayerDecreaseMovespeed : SkillAction
{
    public override float Activate()
    {
        float mod = (float)GetParam(SkillParams.Modifier);
        parent.caster.GetComponent<BuffManager>().speedModifier -= mod;
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