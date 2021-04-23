using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
using static ConstantStrings;

public class SkillManager : MonoBehaviourPun
{
    public PhotonView pv;
    Unit_Movement unitMovement;

    //Data
    public CharacterType myCharacter;

    delegate void voidFunc();
    voidFunc mySkillFunction;
    public float cooltime;
    public double lastActivatedTime;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        unitMovement = GetComponent<Unit_Movement>();
    }
    private void CheckSkillActivation()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (GetRemainingTime() <= 0)
            {
                pv.RPC("SetLastActivated", RpcTarget.AllBuffered);
                mySkillFunction();
            }
        }
    }
    public void SetSkill(CharacterType type) {
        myCharacter = type;
        ParseSkill();
        if (pv.IsMine)
        {
            GameSession.GetInst().skillPanelUI.SetSkillInfo(this);
        }
    }
    void ParseSkill()
    {
        
        pv.RPC("SetLastActivated", RpcTarget.AllBuffered);
        float skillCool = 1f;
        switch (myCharacter)
        {
            case CharacterType.NAGATO:
                mySkillFunction= DoSkillSet_Nagato;
                skillCool = 3f;
                break;
            case CharacterType.HARUHI:
                mySkillFunction= DoSkillSet_Haruhi;
                skillCool = 4f;
                break;
            case CharacterType.MIKURU:
                mySkillFunction = DoSkillSet_Mikuru;
                skillCool = 2f;
                break; 
            case CharacterType.KOIZUMI:
                mySkillFunction = DoSkillSet_Koizumi;
                skillCool = 4f;
                break;
            case CharacterType.KUYOU:
                mySkillFunction = DoSkillSet_Kuyou;
                skillCool = 4f;
                break;
            case CharacterType.ASAKURA:
                mySkillFunction = DoSkillSet_Asakura;
                skillCool = 4f;
                break;
        }
        Debug.Log("Set skill " + myCharacter);
        pv.RPC("SetCooltime",RpcTarget.AllBuffered, skillCool);
    }
    [PunRPC]
    public void SetCooltime(float a) {
        cooltime = a;
    }
    [PunRPC]
    public void SetLastActivated()
    {
        lastActivatedTime = PhotonNetwork.Time;
    }

    private void Update()
    {
        if (!pv.IsMine) return;
        CheckSkillActivation();
    }


    public float GetCoolTime() {
        return cooltime;
    }
    public double GetRemainingTime()
    {
        return (lastActivatedTime + cooltime) - PhotonNetwork.Time;
    }

    #region skills
    private void DoSkillSet_Nagato() {
        SkillSet mySkill = new SkillSet(gameObject, this);

        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_NAGATO);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.MoveSpeed, 9f);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_NoDeathOnCollision());
        mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        GameSession.GetInst().StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Haruhi()
    {
        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.Height, 1f);
        mySkill.SetParam(SkillParams.Width, 1f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_HARUHI);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.Enqueue(new Action_InstantiateBullet());
        mySkill.Enqueue(new Action_NoDeathOnCollision());
        // parent.pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Static, (int)ReactionType.None);
        mySkill.Enqueue(new Action_SetProjectileScale());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        //  obj.GetComponent<PhotonView>().RPC("SetGradualScale", RpcTarget.AllBuffered, duration, projSpeed);
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 3f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 3f });
        mySkill.Enqueue(new Action_DoScaleTween());
        mySkill.Enqueue(new Action_DoDeathAfter());
        StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Mikuru()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 24f);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_MIKURU);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        Debug.Log("Activate skill");
        GameSession.GetInst().StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Koizumi()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.Duration, 1.5f);
        mySkill.SetParam(SkillParams.Modifier, 0.75f);
        mySkill.SetParam(SkillParams.Color, "#c80000");
        mySkill.SetParam(SkillParams.Enable, true);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KOIZUMI);
        mySkill.Enqueue(new Action_InstantiateBullet());
        mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_Projectile_ParentTransformAsPlayer());
        mySkill.Enqueue(new Action_DoDeathAfter());
        mySkill.Enqueue(new Action_PlayerChangeSpriteColor());;
        mySkill.Enqueue(new Action_PlayerIncreaseMovespeed());
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Color, paramValue = "#FFFFFF" });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Enable, paramValue = false });
        mySkill.Enqueue(new Action_PlayerChangeSpriteColor());
        mySkill.Enqueue(new Action_PlayerDecreaseMovespeed());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 1f});
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 1f});
        Debug.Log("Activate skill");
        GameSession.GetInst().StartCoroutine(mySkill.Activate());
    }

    private void DoSkillSet_Kuyou()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KUYOU);
        mySkill.SetParam(SkillParams.AnimationTag, "DoBatswing");
        mySkill.SetParam(SkillParams.Quarternion, Quaternion.identity);
        mySkill.SetParam(SkillParams.Vector3, gameObject.transform.position);
        mySkill.SetParam(SkillParams.Duration, 0.25f);
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_Projectile_ParentTransformAsPlayer());
        mySkill.Enqueue(new Action_DoDeathAfter());
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_GunObject_SetAngle());
        mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_Projectile_ResetAngle());
        mySkill.Enqueue(new Action_PlayerDoGunAnimation());
        Debug.Log("Activate skill");
        GameSession.GetInst().StartCoroutine(mySkill.Activate());
    }

    private void DoSkillSet_Asakura()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 12f);
        mySkill.SetParam(SkillParams.Duration, 0.1f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        for (int i = 0; i < 12; i++)
        {
            float angle =30f* i;
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3());
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.EulerAngle, paramValue = angle });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Quarternion, paramValue = Quaternion.Euler(0,0,angle) });
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
        Debug.Log("Activate skill");
        GameSession.GetInst().StartCoroutine(mySkill.Activate());
    }
    #endregion
}
public enum CharacterType { 
   NONE, NAGATO,HARUHI,MIKURU,KOIZUMI,KUYOU,ASAKURA
}