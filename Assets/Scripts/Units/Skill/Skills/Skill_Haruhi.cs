using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static BulletManager;
using static ConstantStrings;
using Random = UnityEngine.Random;
public class Skill_Haruhi : SkillManager
{ 
    

    public override void MySkillFunction()
    {
        ActionSet mySkill = new ActionSet(gameObject, this);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_HARUHI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 1f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 1f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = 0.6f });
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectileScale());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 6f }); //5.5
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 6f });
        mySkill.Enqueue(new Action_DoScaleTween());
        mySkill.Enqueue(new Action_DoDeathAfter());
        mySkill.Enqueue(new Action_Player_InvincibleBuff());
        mySkill.Enqueue(new Action_WaitForSeconds());
        StartCoroutine(mySkill.Activate());
    }

    public override void LoadInformation()
    {
        cooltime = 3.3f;
    }
}
