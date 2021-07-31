using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Skill_Kyon : ISkill
{
    /*    delegate void voidFunc();
        voidFunc DoSkill;
    */

    ISkill obtainedSkill = null;
    public override ActionSet GetSkillActionSet(SkillManager skillManager)
    {
        if (obtainedSkill == null)
        {
            return GetKyonkoSet(skillManager);
        }
        else
        {
            return obtainedSkill.GetSkillActionSet(skillManager);
        }
    }

    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.2f;
        skm.ai_projectileSpeed = 25f;
        original = skm;
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
    public bool IsNotForSteal(CharacterType targetChar) {
        return (targetChar == CharacterType.KYONKO
           || targetChar == CharacterType.KYON
           || targetChar == CharacterType.YASUMI
           );
    }
    public override void OnPlayerKilledPlayer(EventObject eo)
    {
        HealthPoint targetHP = eo.hitHealthPoint;

        UniversalPlayer attacker = PlayerManager.GetPlayerByID(eo.stringObj);
        if (attacker == null) return;
        if (attacker.uid != original.controller.uid) return;
        if (targetHP.unitType != UnitType.Player) return;
        if (IsNotForSteal(targetHP.unitPlayer.myCharacter)) return;
        obtainedSkill = targetHP.unitPlayer.skillManager.mySkill;
        original.maxStack = 1;
        obtainedSkill.LoadInformation(original);
        UI_SkillBox.SetSkillInfo(original, targetHP.unitPlayer.myCharacter);
        var player = original.gameObject.GetComponent<Unit_Movement>();
        if (player != null && player.autoDriver.machine != null) player.autoDriver.machine.DetermineAttackType(targetHP.unitPlayer.myCharacter);
    }

    public ActionSet GetKyonkoSet(SkillManager skm)
    {
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
