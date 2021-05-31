using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Mikuru : SkillManager
{
    public override void LoadInformation()
    {
        cooltime = 3.2f;
    }

    public override void MySkillFunction()
    {
        ActionSet mySkill = new ActionSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
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
            // mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            // mySkill.Enqueue(new Action_WaitForSeconds());
        }

        StartCoroutine(mySkill.Activate());
    }

}
