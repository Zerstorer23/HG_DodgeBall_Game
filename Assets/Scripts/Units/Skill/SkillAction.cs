using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillAction
{
    protected SkillSet parent;

    public SkillParams paramType;
    public object paramValue;
    public void SetSkillSet(SkillSet p) {
        parent = p;
    }
    public virtual float Activate() {
        return 0f;
    }
    public object GetParam(SkillParams key) {
        return parent.GetParam(key);
    }
}
