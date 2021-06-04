using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Koizumi : ISkill
{
    public override ActionSet GetSkillActionSet(SkillManager skm)
    {
        ActionSet mySkill = new ActionSet( skm);
        mySkill.SetParam(SkillParams.Duration, 1.5f); //1.5
                                                      // mySkill.SetParam(SkillParams.Modifier, 0.75f);
        mySkill.SetParam(SkillParams.Color, "#c80000");
        mySkill.SetParam(SkillParams.Enable, true);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KOIZUMI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);

        BuffData buff = new BuffData(BuffType.MoveSpeed, 0.5f, 1.5f);
        mySkill.SetParam(SkillParams.BuffData, buff);

        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_DoDeathAfter());//
        mySkill.Enqueue(new Action_PlayerChangeSpriteColor()); ;
        mySkill.Enqueue(new Action_Player_AddBuff());//
        mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Color, paramValue = "#FFFFFF" });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Enable, paramValue = false });
        mySkill.Enqueue(new Action_PlayerChangeSpriteColor());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 1f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 1f });
        return mySkill;
    }


    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 5f;
    }

}
