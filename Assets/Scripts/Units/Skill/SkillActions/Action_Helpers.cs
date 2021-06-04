using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_SetParameter : SkillAction
{

    public override float Activate()
    {
        parent.SetParam(paramType, paramValue);
        return 0f;
    }
}
public class Action_SetAngle : SkillAction
{

    public override float Activate()
    {
        parent.SetParam(SkillParams.EulerAngle, paramValue);
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, (float)paramValue));
        return 0f;
    }
}

public class Action_WaitForSeconds : SkillAction
{
    public override float Activate()
    {
        float duration = GetParam<float>(SkillParams.Duration);
        return duration;
    }
}