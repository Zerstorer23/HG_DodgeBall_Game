using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kyonko : ISkill
{
    /*    delegate void voidFunc();
        voidFunc DoSkill;
    */

    ISkill obtainedSkill = null;
    public override ActionSet GetSkillActionSet(SkillManager skillManager) {
        if (obtainedSkill == null)
        {
            return GetKyonkoSet(skillManager);
        }
        else {
            return obtainedSkill.GetSkillActionSet(skillManager);
        }
    }

    public override void LoadInformation(SkillManager skillManager)
    {
        skillManager.cooltime = 3.2f;
        original = skillManager;
    }
    SkillManager original;

/*    public override void OnMyProjectileHit(EventObject eo)
    {
        HealthPoint targetHP = eo.hitHealthPoint;
        if (targetHP.unitType != UnitType.Player) return;
        if (targetHP.unitPlayer.myCharacter == CharacterType.KYONKO|| targetHP.unitPlayer.myCharacter == CharacterType.KYONKO) return;
        Debug.Log("Changed skill ");
        obtainedSkill = targetHP.unitPlayer.skillManager.mySkill;
        original.maxStack = 1;
        obtainedSkill.LoadInformation(original);
    }*/
    public override void OnPlayerKilledPlayer(EventObject eo)
    {
        HealthPoint targetHP = eo.hitHealthPoint;
        if (targetHP.unitType != UnitType.Player) return;
        if (targetHP.unitPlayer.myCharacter == CharacterType.KYONKO || targetHP.unitPlayer.myCharacter == CharacterType.KYONKO) return;
        Debug.Log("Changed skill ");
        obtainedSkill = targetHP.unitPlayer.skillManager.mySkill;
        original.maxStack = 1;
        obtainedSkill.LoadInformation(original);
    }
    public ActionSet GetKyonkoSet(SkillManager skm) {
        ActionSet mySkill = new ActionSet(skm);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KYONKO);
        mySkill.SetParam(SkillParams.MoveSpeed, 25f);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.Duration, 0.17f);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
       // mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        mySkill.Enqueue(new Action_DoDeathAfter());
        return mySkill;
    }

}
