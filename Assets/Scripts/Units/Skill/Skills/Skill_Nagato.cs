using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static BulletManager;
using static ConstantStrings;
using Random = UnityEngine.Random;
public class Skill_Nagato : SkillManager
{
    public override void MySkillFunction()
    {
        ActionSet mySkill = new ActionSet(gameObject, this);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_NAGATO);
        mySkill.SetParam(SkillParams.Duration, 0.4f);
        mySkill.SetParam(SkillParams.MoveSpeed, 24f);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.Enable, false);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_Projectile_ToggleDamage());
        // mySkill.Enqueue(new Action_Player_InvincibleBuff());
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Enable, paramValue = true });
        mySkill.Enqueue(new Action_Projectile_ToggleDamage());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        StartCoroutine(mySkill.Activate());
    }

    public override void LoadInformation()
    {
        cooltime = 3.3f; // 3f
        maxStack = 5;
    }
}
