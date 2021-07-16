using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Taniguchi : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        float aim = skm.unitMovement.GetAim();
        Vector3 position = skm.unitMovement.transform.position;
        float range = 40f;
        int steps = 15;
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_TANIGUCHI);
        mySkill.SetParam(SkillParams.MoveSpeed, 0f);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.Quarternion, Quaternion.identity);
        Vector3 stepVector = GetAngledVector(aim, (range / steps));
        for (int i = 0; i < steps; i++) {
            position += stepVector;
            mySkill.Enqueue(new Action_SetParameter(SkillParams.Vector3,position));


            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileStatic());
            mySkill.Enqueue(new Action_SetParameter(SkillParams.Duration, 1f));
            mySkill.Enqueue(new Action_DoDeathAfter());
            mySkill.Enqueue(new Action_SetParameter(SkillParams.Duration, 0.1f));
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
        return mySkill;
    }

    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.5f;
        skm.maxStack = 3;
    }

  
}
