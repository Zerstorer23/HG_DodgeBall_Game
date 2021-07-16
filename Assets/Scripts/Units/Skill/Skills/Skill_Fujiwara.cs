using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Fujiwara : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_FUJIWARA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Die);
        mySkill.SetParam(SkillParams.Distance, 0.5f);
        int num = 50;
        float time = 2f;
        float speed = 18f;
        mySkill.SetParam(SkillParams.MoveSpeed, speed);

        for (int i = 0; i < num; i++) {
            float random = Random.Range(-15f, 15f);
            mySkill.Enqueue(new Action_Player_GetAim());
            mySkill.Enqueue(new Action_ModifyAngle(random));
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_SetParameter(SkillParams.Duration, time));
            mySkill.Enqueue(new Action_DoDeathAfter());
            mySkill.Enqueue(new Action_SetParameter(SkillParams.Duration,0.05f));
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
     
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 2f;
        skm.maxStack = 3;
    }
    /*
       public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.MoveSpeed, 165f);
        mySkill.SetParam(SkillParams.Distance, 5f);
        mySkill.SetParam(SkillParams.Duration, 0.35f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_MIKURU);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        int numBullets = 1;
        //      mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        for (int n = 0; n < numBullets; n++)
        {
            mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileStraight());
        }
        return mySkill;
    }
     */

}
