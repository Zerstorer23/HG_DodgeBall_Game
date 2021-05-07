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
    Unit_Player player;
    BuffManager buffManager;
    //Data
    public CharacterType myCharacter;

    delegate void voidFunc();
    voidFunc mySkillFunction;
    public float cooltime;
    public double lastActivatedTime;
    public bool skillInUse = false;


    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        unitMovement = GetComponent<Unit_Movement>();
        player = GetComponent<Unit_Player>();
        buffManager = GetComponent<BuffManager>();
    }
    private void CheckSkillActivation()
    {
        //if ()
        if (Input.GetAxis("Fire1") > 0 || Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.Joystick1Button7)
            || MenuManager.auto_drive
            )
        {
            if (GetRemainingTime() <= 0)
            {
                player.PlayShootAudio();
                pv.RPC("SetLastActivated", RpcTarget.All , true);
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
        SetLastActivated(false);
       // pv.RPC("SetLastActivated", RpcTarget.All, false);
        float skillCool = 1f;
        switch (myCharacter)
        {
            case CharacterType.NAGATO:
                mySkillFunction= DoSkillSet_Nagato;
                skillCool = 3f;
                break;
            case CharacterType.HARUHI:
                mySkillFunction= DoSkillSet_Haruhi;
                skillCool = 3.5f;
                break;
            case CharacterType.MIKURU:
                mySkillFunction = DoSkillSet_Mikuru;
                skillCool = 3.3f;
                break; 
            case CharacterType.KOIZUMI:
                mySkillFunction = DoSkillSet_Koizumi;
                skillCool = 5f;
                break;
            case CharacterType.KUYOU:
                mySkillFunction = DoSkillSet_Kuyou;
                skillCool = 3.25f;
                break;
            case CharacterType.ASAKURA:
                mySkillFunction = DoSkillSet_Asakura;
                skillCool = 4f;
                break;
            case CharacterType.KYOUKO:
                mySkillFunction = DoSkillSet_Kyouko;
                skillCool = 3f;
                break;
            case CharacterType.SASAKI:
                mySkillFunction = DoSkillSet_Sasaki;
                skillCool = 6f;
                break;
            case CharacterType.KIMIDORI:
                mySkillFunction = DoSkillSet_Kimidori;
                skillCool = 1.5f;
                break;
        }
        // pv.RPC("SetCooltime",RpcTarget.All, skillCool);
        SetCooltime(skillCool);
    }
    [PunRPC]
    public void SetCooltime(float a) {
        cooltime = a;
    }
    [PunRPC]
    public void SetLastActivated( bool startSkill)
    {
        skillInUse = startSkill;
        lastActivatedTime = PhotonNetwork.Time;
    }
    private void OnDisable()
    {
        skillInUse = false;
    }
    private void Update()
    {
        if (!pv.IsMine) return;
        CheckSkillActivation();
    }

    public float GetCoolTime() {
        return cooltime * buffManager.GetBuff(BuffType.Cooltime);
    }
    public double GetRemainingTime()
    {
        return (lastActivatedTime + GetCoolTime()) - PhotonNetwork.Time;
    }

    #region skills
    private void DoSkillSet_Nagato() {
        SkillSet mySkill = new SkillSet(gameObject, this);

        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_NAGATO);
        mySkill.SetParam(SkillParams.Duration, 0.25f);
        mySkill.SetParam(SkillParams.MoveSpeed, 25f);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
       // mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_SetProjectileStraight());
        StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Haruhi()
    {
        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_HARUHI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 1f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 1f });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = 0.25f });
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_SetProjectileScale());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_Projectile_ParentTransformAsPlayer());
        //   mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 4f }); //5.5
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 4f });
        mySkill.Enqueue(new Action_DoScaleTween());
        mySkill.Enqueue(new Action_DoDeathAfter());
       // mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = 1f });
        mySkill.Enqueue(new Action_Player_InvincibleBuff());

        StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Sasaki()
    {
        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_SASAKI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);

        int steps = 30;
        float dur = 2f;
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = dur });
        BuffData buff = new BuffData(BuffType.MoveSpeed, 0.2f, dur);
        mySkill.SetParam(SkillParams.BuffData, buff);
        mySkill.Enqueue(new Action_Player_InvincibleBuff());
        mySkill.Enqueue(new Action_Player_AddBuff());//
        for (int i = 0; i < steps; i++)
        {
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 1f });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 1f });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = dur });
            mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileScale());
            mySkill.Enqueue(new Action_SetProjectileStatic());
            //   mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 2.5f }); //5.5
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 2.5f });
            mySkill.Enqueue(new Action_DoScaleTween());
            mySkill.Enqueue(new Action_DoDeathAfter());
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = dur / steps });
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
        StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Mikuru()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 30f);
        mySkill.SetParam(SkillParams.Duration, 0.35f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_MIKURU);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        for (int n = 0; n < 2; n++)
        {
            mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
           // mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_WaitForSeconds());
        }

        Debug.Log("Activate skill");
        StartCoroutine(mySkill.Activate());
    }

    private void DoSkillSet_Koizumi()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.Duration, 1.5f); //1.5
       // mySkill.SetParam(SkillParams.Modifier, 0.75f);
        mySkill.SetParam(SkillParams.Color, "#c80000");
        mySkill.SetParam(SkillParams.Enable, true);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KOIZUMI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);

        BuffData buff = new BuffData(BuffType.MoveSpeed, 0.5f, 1.5f);
        mySkill.SetParam(SkillParams.BuffData, buff);

        mySkill.Enqueue(new Action_InstantiateBullet());
    //    mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_Projectile_ParentTransformAsPlayer());
        mySkill.Enqueue(new Action_DoDeathAfter());//
        mySkill.Enqueue(new Action_PlayerChangeSpriteColor());;
        mySkill.Enqueue(new Action_Player_AddBuff());//
        mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Color, paramValue = "#FFFFFF" });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Enable, paramValue = false });
        mySkill.Enqueue(new Action_PlayerChangeSpriteColor());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 1f});
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 1f});
        Debug.Log("Activate skill");
        StartCoroutine(mySkill.Activate());
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
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_Projectile_ParentTransformAsPlayer());
        mySkill.Enqueue(new Action_DoDeathAfter());
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_GunObject_SetAngle());
        mySkill.Enqueue(new Action_Projectile_ResetAngle());
        mySkill.Enqueue(new Action_PlayerDoGunAnimation());
        Debug.Log("Activate skill");
        StartCoroutine(mySkill.Activate());
    }

    private void DoSkillSet_Asakura()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 12f);
        mySkill.SetParam(SkillParams.Duration, 0.033f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Bounce);
        float angleOffset = unitMovement.GetAim();
        int numStep = 15;
        for (int i = 0; i < numStep; i++)
        {
            float angle = angleOffset + (360/ numStep) * i;
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3());
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.EulerAngle, paramValue = angle });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Quarternion, paramValue = Quaternion.Euler(0,0,angle) });
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
        Debug.Log("Activate skill");
        StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Kimidori()
    {
        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 6f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KIMIDORI);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Die);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_Projectile_ParentTransformAsPlayer());
        mySkill.Enqueue(new Action_SetProjectile_Orbit());
        Debug.Log("Activate skill");
        StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Kyouko()
    {
        
        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 16f);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KYOUKO);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        float angleOffset = unitMovement.GetAim();
        int stepSize = 4;
        for (int n = 0; n < 2; n++) {
            for (int i = 0; i < stepSize; i++)
            {
                float angle = angleOffset + (360f / stepSize) * i;
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3());
                mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.EulerAngle, paramValue = angle });
                mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Quarternion, paramValue = Quaternion.Euler(0, 0, angle) });
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileStraight());
            }
            mySkill.Enqueue(new Action_WaitForSeconds());
            for (int i = 0; i < stepSize; i++)
            {
                float angle = angleOffset +((360f / stepSize)/2)+ (360f / stepSize) * i;
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3());
                mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.EulerAngle, paramValue = angle });
                mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Quarternion, paramValue = Quaternion.Euler(0, 0, angle) });
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileStraight());
            }
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
      
        Debug.Log("Activate skill");
        StartCoroutine(mySkill.Activate());
    }
    #endregion
}
public enum CharacterType { 
   NONE, NAGATO,HARUHI,MIKURU,KOIZUMI,KUYOU,ASAKURA,KYOUKO,KIMIDORI,KYONMOUTO,SASAKI
}