using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using static ConstantStrings;

public class SkillManager : MonoBehaviourPun
{
    public PhotonView pv;
    internal Unit_Movement unitMovement;
    protected Unit_Player player;
    internal BuffManager buffManager;
    //Data
    public CharacterType myCharacter;
    Controller controller;

    public ISkill mySkill;
    public int maxStack = 1;
    public float cooltime;
    public bool skillInUse = false;

    public float remainingStackTime;
    public int currStack;

   internal double lastActivated = 0d;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        unitMovement = GetComponent<Unit_Movement>();
        player = GetComponent<Unit_Player>();
        buffManager = GetComponent<BuffManager>();
        controller = GetComponent<Controller>();

    }
    private void OnEnable()
    {
        myCharacter = (CharacterType)pv.InstantiationData[0];
        ParseSkill(myCharacter);
        InitSkill();
        if (pv.IsMine)
        {
            if(!controller.IsBot)UI_SkillBox.SetSkillInfo(this);
            EventManager.StartListening(MyEvents.EVENT_MY_PROJECTILE_HIT, OnProjectileHit);
            EventManager.StartListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, OnPlayerKilledPlayer);
            EventManager.StartListening(MyEvents.EVENT_MY_PROJECTILE_MISS, OnProjectileMiss);
        }
    }



    private void OnDisable()
    {
        skillInUse = false;
        if (pv.IsMine)
        {
            EventManager.StopListening(MyEvents.EVENT_MY_PROJECTILE_HIT, OnProjectileHit);
            EventManager.StopListening(MyEvents.EVENT_MY_PROJECTILE_MISS, OnProjectileMiss);
            EventManager.StopListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, OnProjectileHit);
        }
    }

    public void InitSkill()
    {
        skillInUse = false;
        remainingStackTime = cooltime; // a;
    }
    private void OnProjectileMiss(EventObject eo)
    {
        mySkill.OnMyProjectileMiss(eo);
    }
    private void OnPlayerKilledPlayer(EventObject eo)
    {
        mySkill.OnPlayerKilledPlayer(eo);
    }

    private void OnProjectileHit(EventObject eo)
    {
        mySkill.OnMyProjectileHit(eo);
    }



    private void CheckSkillActivation()
    {
        if (InputHelper.skillKeyFired() ||
            ((GameSession.IsAutoDriving() || controller.IsBot) && unitMovement.autoDriver.CanAttackTarget())
            )
        {
            if (buffManager.GetTrigger(BuffType.BlockSkill)) return;
            if (currStack > 0)
            {
                UI_TouchPanel.isTouching = false;
                lastActivated = PhotonNetwork.Time;
                player.PlayShootAudio();
                pv.RPC("SetSkillInUse", RpcTarget.AllBuffered, true);
                pv.RPC("ChangeStack", RpcTarget.AllBuffered, -1);
                MySkillFunction();
            }
        }
    }

    IEnumerator lastSkillRoutine;
    void MySkillFunction()
    {
        var actionSet = mySkill.GetSkillActionSet(this);
        if (actionSet.isMutualExclusive)
        {
            if (lastSkillRoutine != null)
            {
                StopCoroutine(lastSkillRoutine);
                player.KillUnderlings();
            }
        }
        lastSkillRoutine = actionSet.Activate();
        StartCoroutine(lastSkillRoutine);
    }
    /*public abstract ActionSet MySkillActions(SkillManager skillManager);
    public abstract void LoadInformation(SkillManager skillManager);*/


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
    public bool SkillIsReady()
    {
        return (currStack > 0);
    }
    public bool SkillInUse()
    {
        return (skillInUse);
    }
    public bool IsInvincible()
    {
        return (buffManager.GetTrigger(BuffType.InvincibleFromBullets));
    }

    private void Update()
    {
        CheckSkillStack();
        if (!pv.IsMine) return;
        CheckSkillActivation();
    }

    private void CheckSkillStack()
    {
        // Debug.Log(currStack + " " + maxStack + " " + skillInUse);
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
    public void ParseSkill(CharacterType character)
    {
        myCharacter = character;
        maxStack = 1;
        switch (myCharacter)
        {
            case CharacterType.NAGATO:
                mySkill = new Skill_Nagato();
                break;
            case CharacterType.HARUHI:
                mySkill = new Skill_Haruhi();
                break;
            case CharacterType.MIKURU:
                mySkill = new Skill_Mikuru();
                break;
            case CharacterType.KOIZUMI:
                mySkill = new Skill_Koizumi();
                break;
            case CharacterType.KOIHIME:
                mySkill = new Skill_Koihime();
                break;
            case CharacterType.KUYOU:
                mySkill = new Skill_Kuyou();
                break;
            case CharacterType.ASAKURA:
                mySkill = new Skill_Asakura();
                break;
            case CharacterType.KYOUKO:
                mySkill = new Skill_Kyouko();
                break;
            case CharacterType.KIMIDORI:
                mySkill = new Skill_Kimidori();
                break;
            case CharacterType.SASAKI:
                mySkill = new Skill_Sasaki();
                break;
            case CharacterType.TSURUYA:
                mySkill = new Skill_Tsuruya();
                break;
            case CharacterType.YASUMI:
                mySkill = new Skill_Yasumi();
                break;
            case CharacterType.KYONMOUTO:
                mySkill = new Skill_Kyonmouto();
                break;
            case CharacterType.KYONKO:
                mySkill = new Skill_Kyonko();
                break;
            case CharacterType.KYON:
                mySkill = new Skill_Kyon();
                break;
            case CharacterType.T:
                mySkill = new Skill_T();
                break;
            case CharacterType.MORI:
                mySkill = new Skill_Mori();
                break;
            case CharacterType.Taniguchi:
                mySkill = new Skill_Taniguchi();
                break;
        }
        mySkill.LoadInformation(this);
    }
}
public enum CharacterType
{
    NONE, NAGATO, HARUHI, MIKURU, KOIZUMI, KUYOU, ASAKURA, KYOUKO, KIMIDORI, KYONMOUTO, SASAKI, TSURUYA, KOIHIME, YASUMI
        , KYONKO, KYON, T, Taniguchi, MORI
}