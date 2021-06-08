using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    Dictionary<int, HealthPoint> myProjectiles = new Dictionary<int, HealthPoint>();

    public Team myTeam = Team.HOME;
    public int fieldNo = 0;
    internal GameObject charBody;
    [SerializeField] CharacterBodyManager characterBodymanager;

    CircleCollider2D circleCollider;
    // Start is called before the first frame update
    public int evasion = 0;
    [SerializeField] bool isBot = false;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
        health = GetComponent<HealthPoint>();
        movement = GetComponent<Unit_Movement>();
        buffManager = GetComponent<BuffManager>();
        circleCollider = GetComponent<CircleCollider2D>();
        skillManager = GetComponent<SkillManager>();
    }
    private void OnDisable()
    {
        if (pv.IsMine && !isBot)
        {
            EventManager.StopListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            EventManager.StopListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, IncrementKill);
            GameFieldManager.ChangeToSpectator();
        }
        circleCollider.radius = 0.33f;
        myProjectiles.Clear();
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_DIED, new EventObject() { stringObj = pv.Owner.UserId, intObj = fieldNo });
    }
    private void OnEnable()
    {
        evasion = 0;
        myPortrait.color = new Color(1, 1, 1);
        myUnderlings = new List<GameObject>();
        myTeam = (Team)pv.Owner.CustomProperties["TEAM"];
        ParseInstantiationData();
        if (pv.IsMine && !isBot)
        {
            EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            EventManager.StartListening(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, IncrementKill);
            MainCamera.SetFollow(GameSession.GetInst().networkPos);
            MainCamera.FocusOnField(false);
            ChatManager.SetInputFieldVisibility(false);
            UI_StatDisplay.SetPlayer(this);
        }
        if (!isBot) {
            GameFieldManager.gameFields[fieldNo].playerSpawner.RegisterPlayer(pv.Owner.UserId, this);
            EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_SPAWNED, new EventObject() { stringObj = pv.Owner.UserId, goData = gameObject, intObj = fieldNo });
        }
    }
    void ParseInstantiationData() {
        myCharacter = (CharacterType)pv.InstantiationData[0];
        myPortrait.sprite = ConfigsManager.unitDictionary[myCharacter].portraitImage;
        CheckCustomCharacter();
        int maxLife = (int)pv.InstantiationData[1];
        if (GameSession.gameModeInfo.isTeamGame)
        {
            maxLife += GetTeamBalancedLife((Team)pv.Owner.CustomProperties["TEAM"], maxLife);
        }
        health.SetMaxLife(maxLife);
        fieldNo = (int)pv.InstantiationData[2];
        isBot = (bool)pv.InstantiationData[3];
        if (fieldNo < GameFieldManager.gameFields.Count) {
            movement.SetMapSpec(GameFieldManager.gameFields[fieldNo].mapSpec);
        }
        health.SetAssociatedField(fieldNo);
    }


    public SpriteRenderer mainSprite;
    void CheckCustomCharacter()
    {
        if (!(myCharacter == CharacterType.YASUMI
            || myCharacter == CharacterType.TSURUYA)
            )
        {
            charBody = myPortrait.gameObject;
            characterBodymanager.gameObject.SetActive(false);
            charBody.SetActive(true);
            mainSprite = myPortrait;
            return;
        }
        charBody = characterBodymanager.gameObject;
        myPortrait.gameObject.SetActive(false);
        charBody.SetActive(true);
        characterBodymanager.SetCharacterSkin(myCharacter);
        mainSprite = characterBodymanager.mainSprite;
    }

    private void OnSuddenDeath(EventObject obj)
    {
        if (pv.IsMine && !isBot) {
            enemyIndicator.SetTargetAsNearestEnemy();
        }
    }
    [PunRPC]
    public void SetBodySize(float radius)
    {
        circleCollider.radius = radius;
    }
    [PunRPC]
    public void TriggerMessage(string msg)
    {
        if (!pv.IsMine || isBot) return;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = msg });
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
        int underdogged = (otherTeam - numMyTeam) * maxLife;
        if (underdogged <= 0) return 0; //같거나 우리팀이 더 많음
        return underdogged / numMyTeam; //차이 /우리팀수 
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
    public void SetMyProjectile(GameObject obj) {
        myUnderlings.Add(obj);
        obj.transform.SetParent(gunTransform,false);
        obj.transform.localPosition = Vector3.zero;

    }
    public bool IsBot() => isBot;
    public void PlayHitAudio()
    {
        if (!pv.IsMine || isBot) return;
        AudioManager.PlayAudioOneShot(hitAudio);
    }
    public void PlayShootAudio()
    {
        if (!pv.IsMine || isBot) return;
        AudioManager.PlayAudioOneShot(shootAudio);

    }
    public void AddProjectile(int id, HealthPoint proj) {

        if (myProjectiles.ContainsKey(id))
        {
            myProjectiles[id] = proj;
        }
        else {
            myProjectiles.Add(id, proj);
        }
    }
    public void RemoveProjectile(int id) {
        if (myProjectiles.ContainsKey(id)) {
            myProjectiles.Remove(id);
        }
    }
    public void IncrementKill(EventObject eo)
    {
        if (eo.stringObj == pv.Owner.UserId) {
            StatisticsManager.RPC_AddToStat(StatTypes.KILL, pv.Owner.UserId, 1);
            StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 16);
            StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_KILLS, 1);
        }

    }
    public void IncrementEvasion()
    {
        if (pv.IsMine && !isBot)
        {
            evasion++;
            StatisticsManager.RPC_AddToStat(StatTypes.EVADE, pv.Owner.UserId, 1);
            StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 1);
            StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_EVADES, 1);
            pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)BuffType.Cooltime, 0.2f, 5d);       //(int bType, float mod, double _duration)
        }
    }

    public void KillUnderlings() {
        for (int i = 0; i < myUnderlings.Count; i++) {
            if (myUnderlings[i] == null) continue;
            if (!myUnderlings[i].activeInHierarchy) continue;
            myUnderlings[i].GetComponent<HealthPoint>().Kill_Immediate();        
        }
    
    }

    internal bool FindAttackHistory(int tid)
    {
        foreach (var proj in myProjectiles.Values) {
            if (proj.damageDealer.duplicateDamageChecker.FindAttackHistory(tid)) return true;
        }
        return false;
    }

   
}
