using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Action_SetProjectileStatic : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        int reaction = GetParam<int>(SkillParams.ReactionType);
        parent.pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Static, reaction, 0f);
        return 0f;
    }
}
public class Action_SetProjectileStraight : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        float direction = GetParam<float>(SkillParams.EulerAngle);
        float moveSpeed = GetParam<float>(SkillParams.MoveSpeed);
        int reaction = GetParam<int>(SkillParams.ReactionType);
        parent.pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Straight, reaction, direction);
        parent.pv.RPC("SetMoveInformation", RpcTarget.AllBuffered, moveSpeed, 0f, 0f);
        return 0f;
    }
}
public class Action_SetProjectile_Orbit : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        float direction = GetParam<float>(SkillParams.EulerAngle);
        float moveSpeed = GetParam<float>(SkillParams.MoveSpeed);
        int reaction = GetParam<int>(SkillParams.ReactionType);
        parent.pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.OrbitAround, reaction, direction);
        parent.pv.RPC("SetMoveInformation", RpcTarget.AllBuffered, moveSpeed, 0f, 0f);
        return 0f;
    }
}
public class Action_SetProjectile_Homing_Target : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        PhotonView targetPV = GetParam<PhotonView>(SkillParams.PhotonView);
        parent.pv.RPC("SetHomingTarget", RpcTarget.AllBuffered, targetPV.ViewID);
        return 0f;
    }
}
public class Action_SetProjectile_Homing_Information : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        ReactionType reaction = GetParam<ReactionType>(SkillParams.ReactionType);
        float range = GetParam<float>(SkillParams.Distance);
        float speed = GetParam<float>(SkillParams.RotateSpeed);
        parent.pv.RPC("SetHomingInformation", RpcTarget.AllBuffered, (int)reaction,range,speed);
        return 0f;
    }
}
public class Action_SetProjectile_Homing_Enable : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        bool enable = GetParam<bool>(SkillParams.Enable);
        parent.pv.RPC("EnableHoming",RpcTarget.AllBuffered,enable);
        return 0f;
    }
}
public class Action_SetProjectileCurves : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {

        if (parent.pv == null) return 0f;
        float direction = GetParam<float>(SkillParams.EulerAngle);
        float moveSpeed = GetParam<float>(SkillParams.MoveSpeed);
        float rotateSpeed = GetParam<float>(SkillParams.RotateSpeed);
        float rotateAngle = GetParam<float>(SkillParams.RotateAngle);
        int reaction = GetParam<int>(SkillParams.ReactionType);
        parent.pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Curves, reaction, direction);
        parent.pv.RPC("SetMoveInformation", RpcTarget.AllBuffered, moveSpeed, rotateSpeed, rotateAngle);
        return 0f;
    }
}