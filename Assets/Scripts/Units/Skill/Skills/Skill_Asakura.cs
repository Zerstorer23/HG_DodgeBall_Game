using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Asakura : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.MoveSpeed, 22f);
        mySkill.SetParam(SkillParams.RotateAngle, 60f);
        mySkill.SetParam(SkillParams.RotateSpeed, 150f);
        mySkill.SetParam(SkillParams.Duration, 0.033f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);

        float angleOffset = skm.unitMovement.GetAim();
        int numStep = 15;
        for (int i = 0; i < numStep; i++)
        {
            float angle = angleOffset + (360 / numStep) * i;
            mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            //  mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_SetProjectileCurves());
            //  mySkill.Enqueue(new Action_Player_InvincibleBuff());//
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 4f;
    }


}
