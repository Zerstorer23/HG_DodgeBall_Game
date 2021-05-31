using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
/// <summary>
/// A PhotonView identifies an object across the network (viewID) and configures how the controlling client updates remote instances.
/// </summary>
///     public override float Activate()
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

public class Action_Projectile_ToggleDamage : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        bool enable = (bool)GetParam(SkillParams.Enable);
        parent.pv.RPC("ToggleDamage", RpcTarget.AllBuffered, enable);
        return 0f;
    }
}
public class Action_SetProjectile_InvincibleFromMapBullets : SkillAction
{
    // Start is called before the first frame update
    public override float Activate()
    {
        if (parent.pv == null) return 0f;
        bool enable = (bool)GetParam(SkillParams.Enable);
        parent.pv.RPC("SetInvincibleFromMapBullets", RpcTarget.AllBuffered, enable);
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
