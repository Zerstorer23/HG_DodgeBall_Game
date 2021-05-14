using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
/// <summary>
/// A PhotonView identifies an object across the network (viewID) and configures how the controlling client updates remote instances.
/// </summary>
public class Action_SetProjectileStatic : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        int reaction = (int)GetParam(SkillParams.ReactionType);
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
        float direction = (float)GetParam(SkillParams.EulerAngle);
        float moveSpeed = (float)GetParam(SkillParams.MoveSpeed);
        int reaction = (int)GetParam(SkillParams.ReactionType);
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
        float direction = (float)GetParam(SkillParams.EulerAngle);
        float moveSpeed = (float)GetParam(SkillParams.MoveSpeed);
        int reaction = (int)GetParam(SkillParams.ReactionType);
        parent.pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.OrbitAround, reaction, direction);
        parent.pv.RPC("SetMoveInformation", RpcTarget.AllBuffered, moveSpeed, 0f, 0f);
        return 0f;
    }
}
public class Action_SetProjectileCurves : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {

        if (parent.pv == null) return 0f;
        float direction =(float)GetParam(SkillParams.EulerAngle);
        float moveSpeed = (float)GetParam(SkillParams.MoveSpeed);
        int reaction = (int)GetParam(SkillParams.ReactionType);
        parent.pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Curves, reaction, direction);
        parent.pv.RPC("SetMoveInformation", RpcTarget.AllBuffered, moveSpeed, 0f, 0f);
        return 0f;
    }
}
public class Action_DoScaleTween : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        float duration = (float)GetParam(SkillParams.Duration);
        float scale = (float)GetParam(SkillParams.Width);
        parent.pv.RPC("DoTweenScale", RpcTarget.AllBuffered, duration, scale);
        return 0f;
    }
}
public class Action_WaitForSeconds : SkillAction {
    public override float Activate()
    {
        float duration = (float)GetParam(SkillParams.Duration);
        return duration;
    }
}
public class Action_DoDeathAfter : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        float duration = (float)GetParam(SkillParams.Duration);
        parent.pv.GetComponent<HealthPoint>().DoDeathAfter(duration);
        return 0f;
    }
}
