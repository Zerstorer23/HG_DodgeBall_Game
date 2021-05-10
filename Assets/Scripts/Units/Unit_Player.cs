using Photon.Pun;
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
    public Transform gunTransform;
    [SerializeField]Animator gunAnimator;
    [SerializeField]EnemyIndicator enemyIndicator;
    public GameObject driverIndicator;

    List<GameObject> myUnderlings = new List<GameObject>();

    public Team myTeam = Team.HOME;

    // Start is called before the first frame update
    public int evasion = 0;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
        health = GetComponent<HealthPoint>();
        skillManager = GetComponent<SkillManager>();
        movement = GetComponent<Unit_Movement>();

    }
    private void OnDisable()
    {
        if (pv.IsMine)
        {
            ChatManager.SetInputFieldVisibility(true);
            EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            MainCamera.FocusOnField(true);
        }
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_DIED, new EventObject() { stringObj = pv.Owner.UserId });
    }
    private void OnEnable()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_SPAWNED, new EventObject() { stringObj = pv.Owner.UserId, goData = gameObject });
        evasion = 0;
        myPortrait.color = new Color(1, 1, 1);
        myUnderlings = new List<GameObject>();
        myTeam = (Team)pv.Owner.CustomProperties["TEAM"];
        if (pv.IsMine)
        {
          //  movement.autoDriver = NetworkPosition.GetInst().autoDriver;
          //  NetworkPosition.ConnectPlayer(this);
            EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeath);
            MainCamera.SetFollow(GameSession.GetInst().networkPos);
            MainCamera.FocusOnField(false);
        }
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

    [PunRPC]
    public void SetInformation(int[] array) {
        myCharacter = (CharacterType)array[0];
        skillManager.SetSkill(myCharacter);
        myPortrait.sprite = GameSession.unitDictionary[myCharacter].portraitImage;
        int maxLife = array[1];
        if (GameSession.gameMode == GameMode.TEAM) {
            maxLife += GetTeamBalancedLife((Team)pv.Owner.CustomProperties["TEAM"]);
        }
        health.SetMaxLife(maxLife);
    }
    private int GetTeamBalancedLife(Team myTeam) {
        int numMyTeam = ConnectedPlayerManager.GetNumberInTeam(myTeam);
        int otherTeam = ConnectedPlayerManager.GetNumberInTeam((myTeam == Team.HOME) ? Team.AWAY : Team.HOME);
        Debug.Log("My team :" + myTeam + " = " + numMyTeam + " / " + otherTeam);
        int underdogged = otherTeam - numMyTeam;
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



    public void IncrementKill()
    {
        Debug.Log("Increment kill start");
        StatisticsManager.RPC_AddToStat(StatTypes.KILL,pv.Owner.UserId, 1);
        StatisticsManager.RPC_AddToStat(StatTypes.SCORE,pv.Owner.UserId, 16);
        StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_KILLS, 1);
        Debug.Log("Increment kill finish");
    }
    public void IncrementEvasion()
    {
        if (pv.IsMine)
        {
            StatisticsManager.RPC_AddToStat(StatTypes.EVADE, pv.Owner.UserId, 1);
            StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 1);
            StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_EVADES, 1);
        }
        evasion++;
    }

    public void KillUnderlings() {
        for (int i = 0; i < myUnderlings.Count; i++) {
            if (myUnderlings[i] == null) continue;
            if (!myUnderlings[i].activeInHierarchy) continue;
            myUnderlings[i].GetComponent<HealthPoint>().Kill_Immediate();        
        }
    
    }
 
}
