using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static ConstantStrings;
public class Skill_Tsuruya : SkillManager
{
    public override void LoadInformation()
    {
        cooltime = 4f;
        maxStack = 3;
    }

    public override void MySkillFunction()
    {
        ActionSet mySkill = new ActionSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_TSURUYA);
        mySkill.SetParam(SkillParams.Quarternion, Quaternion.identity);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        float radius = 15f; //5
        float timeStep = 0.25f; //0.25
        int numStep = 6; //10
        int shootAtOnce = 8;//10
        mySkill.SetParam(SkillParams.Duration, timeStep);
        BuffData buff = new BuffData(BuffType.MoveSpeed, -0.2f, timeStep * (numStep));
        mySkill.SetParam(SkillParams.BuffData, buff);
        mySkill.Enqueue(new Action_Player_AddBuff());
        for (int i = 0; i < numStep * shootAtOnce; i++)
        {

            float randAngle = Random.Range(0f, 360f);
            float randDistance = Random.Range(0f, radius);
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Distance, paramValue = randDistance });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.EulerAngle, paramValue = randAngle });
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            //  mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_SetProjectileStatic());
            if (i % shootAtOnce == 0)
            {
                mySkill.Enqueue(new Action_Player_InvincibleBuff());
                mySkill.Enqueue(new Action_WaitForSeconds());
            }

        }
        StartCoroutine(mySkill.Activate());
    }
}
