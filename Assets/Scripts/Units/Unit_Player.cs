using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Player : MonoBehaviourPun
{
    Animator animator;
    internal PhotonView pv;
    public CharacterType myCharacter;
    [SerializeField] SpriteRenderer myPortrait;
    internal HealthPoint health;
    internal SkillManager skillManager;
    public Transform gunTransform;
    public Unit_GunHandler gunHandler;

    [SerializeField]Animator gunAnimator;

    // Start is called before the first frame update
    internal int evasion = 0;
    internal int kills = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
        health = GetComponent<HealthPoint>();
        skillManager = GetComponent<SkillManager>();
    }

    void Start()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_SPAWNED, new EventObject() { stringObj = pv.Owner.UserId , gameObject = gameObject });
        StatisticsManager.RPC_AddToStat(StatTypes.KILL, pv.Owner.UserId, 0);
        StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 0);
        StatisticsManager.RPC_AddToStat(StatTypes.EVADE, pv.Owner.UserId, 0);
    }  // Update is called once per frame

    [PunRPC]
    public void SetInformation(int[] array) {
        myCharacter = (CharacterType)array[0];
        skillManager.SetSkill(myCharacter);
        myPortrait.sprite = EventManager.unitDictionary[myCharacter].portraitImage;
        health.SetMaxLife(array[1]);
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
        gunHandler.transform.rotation = Quaternion.Euler(0, 0, eulerAngle);
    }
    public void SetMyProjectile(GameObject obj) {
        obj.transform.SetParent(gunTransform);
        gunHandler.SetProjectileObject(obj);
    }

    private void OnDisable()
    {
        if (pv.IsMine)
        {
            MainCamera.FocusOnField(true);
            ChatManager.SendNotificationMessage(PhotonNetwork.NickName + "님이 사망했습니다.", "#FF0000");
        }
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_DIED, new EventObject() { stringObj = pv.Owner.UserId });
    }
    private void OnEnable()
    {
        evasion = 0;
        kills = 0;
        myPortrait.color = new Color(1, 1, 1);
        if (pv.IsMine)
        {
            MainCamera.SetFollow(gameObject.transform);
            MainCamera.FocusOnField(false);
        }
    }



    public void IncrementKill() {
        StatisticsManager.RPC_AddToStat(StatTypes.KILL,pv.Owner.UserId, 1);
        StatisticsManager.RPC_AddToStat(StatTypes.SCORE,pv.Owner.UserId, 16);
        kills++;
    }
    public void IncrementEvasion()
    {
        StatisticsManager.RPC_AddToStat(StatTypes.EVADE, pv.Owner.UserId, 1);
        StatisticsManager.RPC_AddToStat(StatTypes.SCORE, pv.Owner.UserId, 1);
        evasion++;
    }
 
}
