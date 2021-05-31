using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Yasumi : SkillManager
{
    public override void LoadInformation()
    {
        cooltime = 5f;
    }

    public override void MySkillFunction()
    {
        ActionSet mySkill = new ActionSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.Width, 2f); ;
        float duration = 8f;
        BuffData buff = new BuffData(BuffType.MirrorDamage, 0f, duration);
        BuffData invincible = new BuffData(BuffType.InvincibleFromBullets, 0f, duration);
        mySkill.SetParam(SkillParams.BuffData, buff);
        mySkill.SetParam(SkillParams.Duration, Time.fixedDeltaTime * 2);

        mySkill.Enqueue(new Action_Player_SetColliderSize());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 0.33f });
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_Player_SetColliderSize());
        mySkill.Enqueue(new Action_Player_AddBuff());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = duration });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.BuffData, paramValue = invincible });
        mySkill.Enqueue(new Action_Player_AddBuff());
        mySkill.Enqueue(new Action_WaitForSeconds());
        StartCoroutine(mySkill.Activate());
    }
}
