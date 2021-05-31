using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kimidori : SkillManager
{
    public override void LoadInformation()
    {
        cooltime = 2f;
    }

    public override void MySkillFunction()
    {
        ActionSet mySkill = new ActionSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 6f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KIMIDORI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Die);
        mySkill.SetParam(SkillParams.Enable, !GameSession.gameModeInfo.isCoop);

        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectile_Orbit());
        mySkill.Enqueue(new Action_SetProjectile_InvincibleFromMapBullets());
        StartCoroutine(mySkill.Activate());
    }

}
