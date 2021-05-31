using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Asakura : SkillManager
{
    public override void LoadInformation()
    {
        cooltime = 4f;
    }

    public override void MySkillFunction()
    {
        ActionSet mySkill = new ActionSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 22f);
        mySkill.SetParam(SkillParams.RotateAngle, 60f);
        mySkill.SetParam(SkillParams.RotateSpeed, 150f);
        mySkill.SetParam(SkillParams.Duration, 0.033f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);

        float angleOffset = unitMovement.GetAim();
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
        StartCoroutine(mySkill.Activate());
    }

}
