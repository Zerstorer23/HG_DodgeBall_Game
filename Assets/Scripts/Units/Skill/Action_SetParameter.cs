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
