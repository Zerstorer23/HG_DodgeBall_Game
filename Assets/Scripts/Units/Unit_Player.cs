﻿using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Unit_Player : MonoBehaviourPun
{
    Animator animator;
    [SerializeField] internal AudioClip hitAudio, shootAudio;
    public PhotonView pv;
    public CharacterType myCharacter;
    [SerializeField] SpriteRenderer myPortrait;
    internal HealthPoint health;
    internal SkillManager skillManager;
    internal Unit_Movement movement;
    internal BuffManager buffManager;
    public Transform gunTransform;
    [SerializeField]Animator gunAnimator;
    [SerializeField]EnemyIndicator enemyIndicator;
    public GameObject driverIndicator;

    List<GameObject> myUnderlings = new List<GameObject>();

    public Team myTeam = Team.HOME;
    public int fieldNo = 0;

    internal GameObject charBody;
    [SerializeField] CharacterBodyManager characterBodymanager;

    CircleCollider2D circleCollider;
    // Start is called before the first frame update
    public int evasion = 0;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
        health = GetComponent<HealthPoint>();
        skillManager = GetComponent<SkillManager>();
        movement = GetComponent<Unit_Movement>();
        buffManager = GetComponent<BuffManager>();
        circleCollider = GetComponent<CircleCollider2D>();

    }
    private void OnDisable()
    {
        if (pv.IsMine)
        {
           
            EventManager.StopListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            EventManager.StopListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, IncrementKill);
            GameFieldManager.ChangeToSpectator();
        }
        circleCollider.radius = 0.33f;
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_DIED, new EventObject() { stringObj = pv.Owner.UserId, intObj = fieldNo });
    }
    private void OnEnable()
    {
        evasion = 0;
        myPortrait.color = new Color(1, 1, 1);
        myUnderlings = new List<GameObject>();
        myTeam = (Team)pv.Owner.CustomProperties["TEAM"];
        ParseInstantiationData();
        if (pv.IsMine)
        {
          //  movement.autoDriver = NetworkPosition.GetInst().autoDriver;
          //  NetworkPosition.ConnectPlayer(this);
            EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            EventManager.StartListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, IncrementKill);
            MainCamera.SetFollow(GameSession.GetInst().networkPos);
            MainCamera.FocusOnField(false);
            ChatManager.SetInputFieldVisibility(false);
            UI_StatDisplay.SetPlayer(this);
        }
        GameFieldManager.gameFields[fieldNo].playerSpawner.RegisterPlayer(pv.Owner.UserId,this);
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_SPAWNED, new EventObject() { stringObj = pv.Owner.UserId, goData = gameObject, intObj = fieldNo });
    }
    void ParseInstantiationData() {
        myCharacter = (CharacterType)pv.InstantiationData[0];
        skillManager.SetSkill(myCharacter);
        myPortrait.sprite = GameSession.unitDictionary[myCharacter].portraitImage;
        CheckCustomCharacter();
        int maxLife = (int)pv.InstantiationData[1];
        if (GameSession.gameMode == GameMode.TEAM)
        {
            maxLife += GetTeamBalancedLife((Team)pv.Owner.CustomProperties["TEAM"], maxLife);
        }
        health.SetMaxLife(maxLife);
        fieldNo = (int)pv.InstantiationData[2];
        if (fieldNo < GameFieldManager.gameFields.Count) {
            movement.SetMapSpec(GameFieldManager.gameFields[fieldNo].mapSpec);
        }
        health.SetAssociatedField(fieldNo);
    }
    void CheckCustomCharacter() {
        if (!(myCharacter == CharacterType.YASUMI
            || myCharacter == CharacterType.TSURUYA)
            ) {
            charBody = myPortrait.gameObject;
            characterBodymanager.gameObject.SetActive(false);
            charBody.SetActive(true);
            return;
        }
        charBody = characterBodymanager.gameObject;
        myPortrait.gameObject.SetActive(false);
        charBody.SetActive(true);
        characterBodymanager.SetCharacterSkin(myCharacter);




    }



    private void OnSuddenDeath(EventObject obj)
    {
        if (pv.IsMine) {
            enemyIndicator.SetTargetAsNearestEnemy();
        }
    }

    void Start()
    {
        StatisticsManager.RPC_AddToStat(StatTypes.KILL, pv.Owner.UserId, 0);
        StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 0);
        StatisticsManager.RPC_AddToStat(StatTypes.EVADE, pv.Owner.UserId, 0);


    }  // Update is called once per frame

    private int GetTeamBalancedLife(Team myTeam, int maxLife) {
        int numMyTeam = ConnectedPlayerManager.GetNumberInTeam(myTeam);
        int otherTeam = ConnectedPlayerManager.GetNumberInTeam((myTeam == Team.HOME) ? Team.AWAY : Team.HOME);
        Debug.Log("My team :" + myTeam + " = " + numMyTeam + " / " + otherTeam);
        int underdogged = (otherTeam - numMyTeam) * maxLife;
        if (underdogged <= 0) return 0; //같거나 우리팀이 더 많음
        Debug.Log("Add nmodifier :" + underdogged / numMyTeam);
        return underdogged / numMyTeam; //차이 /우리팀수 
    }

    [PunRPC]
    public void ChangePortraitColor(string hexColor)
    {
        myPortrait.color = ConstantStrings.GetColorByHex(hexColor);
    }
    [PunRPC]
    public void TriggerGunAnimation(string tag) {
        gunAnimator.SetTrigger(tag);
    }
    [PunRPC]
    public void SetGunAngle(float eulerAngle)
    {
        gunAnimator.transform.rotation = Quaternion.Euler(0, 0, eulerAngle);
    }
    [PunRPC]
    public void SetBodySize(float radius)
    {
       circleCollider.radius = radius;
    }
    [PunRPC]
    public void TriggerMessage(string msg)
    {
        if (!pv.IsMine) return;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = msg });
    }


    public void SetMyProjectile(GameObject obj) {
        myUnderlings.Add(obj);
        obj.transform.SetParent(gunTransform,false);
        obj.transform.localPosition = Vector3.zero;

    }
    public void PlayHitAudio()
    {
        if (!pv.IsMine) return;
        AudioManager.PlayAudioOneShot(hitAudio);
    }
    public void PlayShootAudio()
    {
        if (!pv.IsMine) return;
        AudioManager.PlayAudioOneShot(shootAudio);

    }

    public void IncrementKill(EventObject eo)
    {
        if (eo.stringObj == pv.Owner.UserId) {
            Debug.Log("Increment kill start");
            StatisticsManager.RPC_AddToStat(StatTypes.KILL, pv.Owner.UserId, 1);
            StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 16);
            StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_KILLS, 1);
            Debug.Log("Increment kill finish");

        }

    }
    public void IncrementEvasion()
    {
        if (pv.IsMine)
        {
            Debug.Log("Evaded!");
            StatisticsManager.RPC_AddToStat(StatTypes.EVADE, pv.Owner.UserId, 1);
            StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 1);
            StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_EVADES, 1);
            pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)BuffType.Cooltime, 0.2f, 5d);
            evasion++;
            //(int bType, float mod, double _duration)
        }
    }

    public void KillUnderlings() {
        for (int i = 0; i < myUnderlings.Count; i++) {
            if (myUnderlings[i] == null) continue;
            if (!myUnderlings[i].activeInHierarchy) continue;
            myUnderlings[i].GetComponent<HealthPoint>().Kill_Immediate();        
        }
    
    }
 
}
