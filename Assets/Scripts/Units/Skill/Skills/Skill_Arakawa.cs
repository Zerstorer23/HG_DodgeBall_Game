using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Arakawa : ISkill
{
    public delegate void setFunc(ActionSet mySkill);
    public static setFunc skillSet;



    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);

        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = 1f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Modifier, paramValue = 0.33f });
        mySkill.Enqueue(new Action_Player_MovespeedBuff());// 
        mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        Instantiate(mySkill, PREFAB_BULLET_ARAKAWA2, 1f);
        Instantiate(mySkill, PREFAB_BULLET_ARAKAWA, 2f);
        return mySkill;
    }

    private void Instantiate(ActionSet mySkill, string prefabName, float duration) {
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.PrefabName, paramValue = prefabName });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.ReactionType, paramValue = ReactionType.None });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = duration });
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_DoDeathAfter());
        mySkill.Enqueue(new Action_WaitForSeconds());
    }
 
    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 4.8f;
        skm.maxStack = 1;

    }


}
