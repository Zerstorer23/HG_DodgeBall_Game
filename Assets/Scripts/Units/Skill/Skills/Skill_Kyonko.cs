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

    public override void LoadInformation(SkillManager skm)
    {
        skm.cooltime = 3.2f;
        skm.ai_projectileSpeed = 25f;
        original = skm;
    }
    SkillManager original;
    void CheckYasumi(HealthPoint targetHP)
    {
        if (targetHP.unitPlayer.myCharacter == CharacterType.YASUMI)
        {
            if (original.pv.IsMine)
            {
                original.pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)BuffType.HideBuffs, 1f, -1d);
            }
        }
        else
        {
            BuffData buff = new BuffData(BuffType.HideBuffs, 1f, -1d);
            original.buffManager.RemoveBuff(buff);
        }
    }
    public bool IsNotForSteal(CharacterType targetChar)
    {
        return (targetChar == CharacterType.KYONKO
           || targetChar == CharacterType.KYON
           || targetChar == CharacterType.YASUMI
           );
    }
    public override void OnMyProjectileHit(EventObject eo)
    {
        if (eo.sourceDamageDealer.myHealth.controller.uid != original.controller.uid) return;
        HealthPoint targetHP = eo.hitHealthPoint;
        if (targetHP.unitType != UnitType.Player) return;
        if (IsNotForSteal(targetHP.unitPlayer.myCharacter)) return;

        obtainedSkill = targetHP.unitPlayer.skillManager.mySkill;
        original.maxStack = 1;
        obtainedSkill.LoadInformation(original);
        UI_SkillBox.SetSkillInfo(original, targetHP.unitPlayer.myCharacter);
        var player = original.gameObject.GetComponent<Unit_Movement>();
        if (player != null && player.autoDriver.machine != null) player.autoDriver.machine.DetermineAttackType(targetHP.unitPlayer.myCharacter);
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
