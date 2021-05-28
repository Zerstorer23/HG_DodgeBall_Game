using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static BulletManager;
using static ConstantStrings;
using Random = UnityEngine.Random;

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
    public int maxStack;
    public float cooltime;
    public bool skillInUse = false;

    public float remainingStackTime;
    public int currStack;

    double lastActivated = 0d;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        unitMovement = GetComponent<Unit_Movement>();
        player = GetComponent<Unit_Player>();
        buffManager = GetComponent<BuffManager>();
    }



    private void CheckSkillActivation()
    {
        if (PhotonNetwork.Time < lastActivated + 0.4) return;
        if (InputHelper.skillKeyFired() || 
            (MenuManager.auto_drive && CheckAutoDrive())   
            )
        {
            if (currStack > 0)
            {
                lastActivated = PhotonNetwork.Time;
                player.PlayShootAudio();
                pv.RPC("SetSkillInUse", RpcTarget.All, true);
                pv.RPC("ChangeStack", RpcTarget.AllBuffered, -1);
                mySkillFunction();
            }
        }
    }
    bool CheckAutoDrive() {
        return unitMovement.autoDriver.targetEnemy != null;
    }

    private void OnEnable()
    {
        ParseSkill();
        if (pv.IsMine)
        {
            GameSession.GetInst().skillPanelUI.SetSkillInfo(this);
        }
    }
    public void SetSkill(CharacterType type)
    {
        myCharacter = type;
    }
    void ParseSkill()
    {
        SetSkillInUse(false);
        // pv.RPC("SetLastActivated", RpcTarget.All, false);
        float skillCool = 1f;
        maxStack = 1;
        switch (myCharacter)
        {
            case CharacterType.NAGATO:
                mySkillFunction = DoSkillSet_Nagato;
                skillCool = 3f; // 3f
                maxStack = 5;
                break;
            case CharacterType.HARUHI:
                mySkillFunction = DoSkillSet_Haruhi;
                skillCool = 3.5f;
                break;
            case CharacterType.MIKURU:
                mySkillFunction = DoSkillSet_Mikuru;
                skillCool = 3.2f;
                break;
            case CharacterType.KOIZUMI:
            case CharacterType.KOIHIME:
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
                skillCool = 3.6f;
                break;
            case CharacterType.SASAKI:
                mySkillFunction = DoSkillSet_Sasaki;
                skillCool = 6f;
                break;
            case CharacterType.KIMIDORI:
                mySkillFunction = DoSkillSet_Kimidori;
                skillCool = 0.7f;
                break;
            case CharacterType.TSURUYA:
                mySkillFunction = DoSkillSet_Tsuruya;
                skillCool = 3f;
                maxStack = 3;
                break;
            case CharacterType.YASUMI:
                mySkillFunction = DoSkillSet_Yasumi;
                skillCool = 5f;
                break;
        }
        SetCooltime(skillCool);
    }

    public void SetCooltime(float a)
    {
        cooltime = a;
        remainingStackTime = a;
    }
    [PunRPC]
    public void SetSkillInUse(bool startSkill)
    {
        skillInUse = startSkill;
    }
    [PunRPC]
    public void ChangeStack(int a)
    {
        currStack += a;
    }

    private void OnDisable()
    {
        skillInUse = false;
    }
    private void Update()
    {
        CheckSkillStack();
        if (!pv.IsMine) return;
        CheckSkillActivation();
    }

    private void CheckSkillStack()
    {
        if (currStack < maxStack && !skillInUse)
        {
            remainingStackTime -= Time.deltaTime * buffManager.GetBuff(BuffType.Cooltime);
        }
        if (remainingStackTime <= 0)
        {
            if (pv.IsMine)
            {
                pv.RPC("ChangeStack", RpcTarget.AllBuffered, 1);
            }
            remainingStackTime += cooltime;
        }

    }
    #region skills
    private void DoSkillSet_Nagato()
    {
        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_NAGATO);
        mySkill.SetParam(SkillParams.Duration, 0.4f);
        mySkill.SetParam(SkillParams.MoveSpeed, 25f);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.Enqueue(new Action_GetCurrentPlayerPosition());
        mySkill.Enqueue(new Action_InstantiateBulletAt());
        mySkill.Enqueue(new Action_Player_InvincibleBuff());
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
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = 0.6f });
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
        mySkill.Enqueue(new Action_SetProjectileScale());
        mySkill.Enqueue(new Action_SetProjectileStatic());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 6f }); //5.5
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 6f });
        mySkill.Enqueue(new Action_DoScaleTween());
        mySkill.Enqueue(new Action_DoDeathAfter());
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
        BuffData buff = new BuffData(BuffType.MoveSpeed, 0.3f, dur);
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
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 3f }); //5.5
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Height, paramValue = 3f });
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
        mySkill.SetParam(SkillParams.MoveSpeed, 165f);
        mySkill.SetParam(SkillParams.Distance, 5f);
        mySkill.SetParam(SkillParams.Duration, 0.35f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_MIKURU);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        int numBullets = 1;
        mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        for (int n = 0; n < numBullets; n++)
        {
            mySkill.Enqueue(new Action_GetCurrentPlayerPosition_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            // mySkill.Enqueue(new Action_SetProjectileExcludePlayer());
            mySkill.Enqueue(new Action_SetProjectileStraight());
            // mySkill.Enqueue(new Action_WaitForSeconds());
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
        mySkill.Enqueue(new Action_Player_InvincibleBuff());//
        mySkill.Enqueue(new Action_InstantiateBullet_FollowPlayer());
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
        mySkill.SetParam(SkillParams.MoveSpeed, 21f);
        mySkill.SetParam(SkillParams.RotateAngle, 60f);
        mySkill.SetParam(SkillParams.RotateSpeed, 150f);
        mySkill.SetParam(SkillParams.Duration, 0.033f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_ASAKURA);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.Bounce);

        float angleOffset = unitMovement.GetAim();
        int numStep = 15;
        for (int i = 0; i < numStep; i++)
        {
            float angle = angleOffset + (360 / numStep) * i;
            mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            //  mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_SetProjectileCurves());
            mySkill.Enqueue(new Action_Player_InvincibleBuff());//
            mySkill.Enqueue(new Action_WaitForSeconds());
        }
        StartCoroutine(mySkill.Activate());
    }
    private void DoSkillSet_Kimidori()
    {
        SkillSet mySkill = new SkillSet(gameObject, this);
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
    private void DoSkillSet_Kyouko()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.MoveSpeed, 16f);
        mySkill.SetParam(SkillParams.Duration, 0.5f);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_KYOUKO);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        mySkill.SetParam(SkillParams.RotateAngle, 90f);
        mySkill.SetParam(SkillParams.RotateSpeed, 270f);
        float angleOffset = unitMovement.GetAim();
        int stepSize = 4;
        for (int n = 0; n < 2; n++)
        {
            for (int i = 0; i < stepSize; i++)
            {
                float angle = angleOffset + (360f / stepSize) * i;
                mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileStraight());
            }
            mySkill.Enqueue(new Action_Player_InvincibleBuff());//
            mySkill.Enqueue(new Action_WaitForSeconds());
            for (int i = 0; i < stepSize; i++)
            {
                float angle = angleOffset + ((360f / stepSize) / 2) + (360f / stepSize) * i;
                mySkill.Enqueue(new Action_SetAngle() { paramValue = angle });
                mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
                mySkill.Enqueue(new Action_InstantiateBulletAt());
                mySkill.Enqueue(new Action_SetProjectileCurves());
            }
            mySkill.Enqueue(new Action_Player_InvincibleBuff());//
            mySkill.Enqueue(new Action_WaitForSeconds());
        }

        Debug.Log("Activate skill");
        StartCoroutine(mySkill.Activate());
    }

    private void DoSkillSet_Tsuruya()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.PrefabName, PREFAB_BULLET_TSURUYA);
        mySkill.SetParam(SkillParams.Quarternion, Quaternion.identity);
        mySkill.SetParam(SkillParams.ReactionType, ReactionType.None);
        float radius = 12f; //5
        float timeStep = 0.25f; //0.25
        int numStep = 4; //10
        int shootAtOnce = 5;//10
        mySkill.SetParam(SkillParams.Duration, timeStep);
        BuffData buff = new BuffData(BuffType.MoveSpeed, -0.5f, timeStep * (numStep));
        mySkill.SetParam(SkillParams.BuffData, buff);
        mySkill.Enqueue(new Action_Player_AddBuff());
        for (int i = 0; i < numStep * shootAtOnce; i++)
        {

            float randAngle = Random.Range(0f, 360f);
            float randDistance = Random.Range(0f, radius);
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Distance, paramValue = randDistance });
            mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.EulerAngle, paramValue = randAngle });
            mySkill.Enqueue(new Action_GetCurrentPlayerVector3_AngledOffset());
            mySkill.Enqueue(new Action_InstantiateBulletAt());
            //  mySkill.Enqueue(new Action_SetProjectileStraight());
            mySkill.Enqueue(new Action_SetProjectileStatic());
            if (i % shootAtOnce == 0)
            {
                mySkill.Enqueue(new Action_WaitForSeconds());
            }

        }
        StartCoroutine(mySkill.Activate());
    }

    private void DoSkillSet_Yasumi()
    {

        SkillSet mySkill = new SkillSet(gameObject, this);
        mySkill.SetParam(SkillParams.UserID, pv.Owner.UserId);
        mySkill.SetParam(SkillParams.Width, 2f); ;
        float duration = 8f;
        BuffData buff = new BuffData(BuffType.MirrorDamage, 0f, duration);
        BuffData invincible = new BuffData(BuffType.InvincibleFromBullets, 0f, duration);
        mySkill.SetParam(SkillParams.BuffData, buff);
        mySkill.SetParam(SkillParams.Duration, Time.fixedDeltaTime * 2);

        mySkill.Enqueue(new Action_Player_SetColliderSize());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Width, paramValue = 0.33f });
        mySkill.Enqueue(new Action_WaitForSeconds());
        mySkill.Enqueue(new Action_Player_SetColliderSize());
        mySkill.Enqueue(new Action_Player_AddBuff());
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.Duration, paramValue = duration });
        mySkill.Enqueue(new Action_SetParameter() { paramType = SkillParams.BuffData, paramValue = invincible });
        mySkill.Enqueue(new Action_Player_AddBuff());
        mySkill.Enqueue(new Action_WaitForSeconds());
        StartCoroutine(mySkill.Activate());
    }
    #endregion
}
public enum CharacterType
{
    NONE, NAGATO, HARUHI, MIKURU, KOIZUMI, KUYOU, ASAKURA, KYOUKO, KIMIDORI, KYONMOUTO, SASAKI, TSURUYA, KOIHIME, YASUMI
}