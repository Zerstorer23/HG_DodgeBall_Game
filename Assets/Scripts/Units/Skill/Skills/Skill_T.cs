using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_T : ISkill
{
    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3f;
        skm.maxStack = 3;
    }
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        float searchRange = 10f;
        float rotateSpeed = 180f;

        mySkill.SetParam(SkillParams.MoveSpeed, 15f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_T);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Bounce);
        mySkill.SetParam(SkillParams.Enable, false);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_SetProjectileStraight());

        //---Homing info
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.RotateSpeed, paramValue = rotateSpeed });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Distance, paramValue = searchRange });
        mySkill.Enqueue(new Action_SetProjectile_Homing_Information());
        return mySkill;
    }




}
