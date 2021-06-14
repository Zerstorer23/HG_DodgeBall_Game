using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



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
public class Action_Player_AddGravity : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.casterPV == null) return 0f;
        Vector3 direction = GetParam<Vector3>(SkillParams.Vector3, Vector3.zero);
        float weight = GetParam<float>(SkillParams.Modifier, 0f);
        parent.casterPV.RPC("AddMovementForce", RpcTarget.AllBuffered, direction,weight);
        return 0f;
    }
}
public class Action_Player_ResetGravity : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.casterPV == null) return 0f;
        parent.casterPV.RPC("AddMovementForce", RpcTarget.AllBuffered, Vector3.zero, 0f);
        return 0f;
    }
}