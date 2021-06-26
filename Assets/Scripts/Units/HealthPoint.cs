using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPoint : MonoBehaviourPun
 // , IPunObservable
{
    public UnitType unitType;
    public Controller controller;
    int maxLife = 1;
    public int currentLife;
    bool isDead = false;
    [SerializeField] bool invincibleFromBullets = false;
    public bool invincibleFromMapBullets = false;
    public bool dontKillByException = false;
    internal PhotonView pv;

    
    internal BuffManager buffManager;
    internal Unit_Player unitPlayer;
    internal Projectile_DamageDealer damageDealer;
    internal Component movement;
    public Team myTeam {
        get => controller.Owner.GetProperty("TEAM", Team.HOME);
    }
    public int associatedField = 0;

    public string killerUID=null;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        buffManager = GetComponent<BuffManager>();
        controller = GetComponent<Controller>();
        unitPlayer = GetComponent<Unit_Player>();
        damageDealer = GetComponent<Projectile_DamageDealer>();
        if (unitType == UnitType.Player)
        {
            movement = GetComponent<Unit_Movement>();
        }
        else {
            movement = GetComponent<Projectile_Movement>();
        }  
    }

    private void OnFieldFinish(EventObject obj)
    {
        if (!isDead && obj.intObj == associatedField)
        {
            Kill_Immediate();
        }
    }

    private void FixedUpdate()  
    {
        if (pv.IsMine && currentLife <= 0) {
            isDead = true;
            CheckUnderlings();
           // Debug.Log("Destroy called " + pv.ViewID + " / " + gameObject.name);
            PhotonNetwork.Destroy(pv);
        }
    }
    public void SetMaxLife(int life)
    {
        maxLife = life;
        currentLife = maxLife;
    }
    public int GetMaxLife()
    {
        return maxLife;
    }

    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);
        currentLife = maxLife;
        //    unitType = DetermineType();
        isDead = false;
        killerUID = null;
        if (unitType == UnitType.Projectile) currentLife = 1;
    }

    [PunRPC]
    public void SetInvincibleFromMapBullets(bool enable) {
        invincibleFromMapBullets = enable;
    }

    public void SetAssociatedField(int no) {
        associatedField = no;
    }

    private void OnDisable()
    {
        isDead = true;
        EventManager.StopListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);
    }

    internal void HealHP(int amount)
    {
        if (amount > 0 && currentLife >= maxLife) return;

        if (pv.IsMine)
        {
            pv.RPC("ChangeHP", RpcTarget.AllBuffered, amount);
        }
        
    }
    public bool IsInvincible() {
        return (invincibleFromBullets || buffManager.GetTrigger(BuffType.InvincibleFromBullets));    
    }
    int expectedlife;

   [PunRPC]
    internal void DoDamage(string attackerUserID, bool instaDeath)
    {
        if (isDead) return;
        CheckMirrorDamage(attackerUserID);
        if (IsInvincible()) return;
        if (pv.IsMine)
        {
            pv.RPC("ChangeHP", RpcTarget.AllBuffered, -1);
        }
        if (unitType == UnitType.Player)
        {
            expectedlife = currentLife - 1;
            if (pv.IsMine)
            {
                PhotonNetwork.Instantiate(ConstantStrings.PREFAB_HEAL_1, transform.position, Quaternion.identity, 0);
                if (controller.IsLocal) {

                    MainCamera.instance.DoShake();
                    #if UNITY_ANDROID && !UNITY_EDITOR
                                    Handheld.Vibrate();
                    #endif
                    unitPlayer.PlayHitAudio();
                }
            }
            NotifySourceOfDamage(attackerUserID, instaDeath);
        }
    }

    private void CheckMirrorDamage(string attackerUserID)
    {
        if (!controller.IsMine) return;
        if (IsMirrorDamage())
        {
            Unit_Player unit= GameFieldManager.gameFields[associatedField].playerSpawner.GetUnitByControllerID(attackerUserID);
            if (unit == null) return;
            if (controller.IsLocal) {
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = unit.controller.Owner.NickName + "님에게 피해 반사" });
                unit.pv.RPC("TriggerMessage", RpcTarget.AllBuffered, "피해가 반사되었습니다!");
            }
            buffManager.AddStat(BuffType.NumDamageReceivedWhileBuff,1);
            unit.pv.RPC("ChangeHP", RpcTarget.AllBuffered, -1);
//            damageDealer.DoPlayerCollision(unit.gameObject);
        }
    }

    private bool IsMirrorDamage()
    {
        return buffManager.GetTrigger(BuffType.MirrorDamage);
    }

    public void Kill_Immediate() {
        //Kill not called by RPC
        if (!pv.IsMine || isDead) return;
        isDead = true;
        if (wkRoutine != null) StopCoroutine(wkRoutine);
        CheckUnderlings();
        PhotonNetwork.Destroy(pv);
    }
    void CheckUnderlings() {
        if (unitType == UnitType.Player) {
            unitPlayer.KillUnderlings();
        }
    }

    void NotifySourceOfDamage(string attackerUserID , bool instaDeath)
    {
        UniversalPlayer p = PlayerManager.GetPlayerByID(attackerUserID);
        string attackerNickname = (p == null) ? "???" : p.NickName;
        bool targetIsDead = (expectedlife <= 0 || instaDeath);
        if (controller.IsMine)
        {
            targetIsDead = (currentLife <= 0 || instaDeath);
            if (attackerUserID == null)
            { //AttackedByMapObject
                if (controller.IsLocal)
                {
                    EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = "회피실패" });
                }
                if (targetIsDead)
                {
                    if (killerUID == null)
                    {
                        killerUID = "mapobj";
                        ChatManager.SendNotificationMessage(PhotonNetwork.NickName + "님이 사망했습니다.", "#FF0000");
                    }
                }
            }
            else if (controller.IsLocal)
            {
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = string.Format("{0}에게 피격", attackerNickname) });
            }

        }
        else if (PhotonNetwork.LocalPlayer.UserId == attackerUserID)
        {
            EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = string.Format("{0}를 타격..!", controller.Owner.NickName) });
            if (targetIsDead)
            {
                if (killerUID == null)
                {
                    killerUID = attackerUserID;
                    EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_KILLED_A_PLAYER, new EventObject() { stringObj = attackerUserID, hitHealthPoint = this });
                    ChatManager.SendNotificationMessage(attackerNickname + " 님이 " + controller.Owner.NickName + "님을 살해했습니다.", "#FF0000");
                }
            }
        }

    }

   // IEnumerator deathCoroutine;
    IEnumerator wkRoutine;
    private IEnumerator WaitAndKill(float delay)
    {
        yield return new WaitForSeconds(delay);
        Kill_Immediate();
    }

    public void DoDeathAfter(float delay)
    {
        wkRoutine = WaitAndKill(delay);
        StartCoroutine(wkRoutine);
    }

    [PunRPC]
    public void ChangeHP(int a)
    {
        currentLife += a;
    }
    public bool IsMapProjectile() {
        if (unitType != UnitType.Projectile) return false;
        if (damageDealer == null) return false;
        return damageDealer.isMapObject;
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //통신을 보내는 
        if (stream.IsWriting)
        {
            stream.SendNext(currentLife);
            Debug.Log("Sent life " + currentLife);
        }

        //클론이 통신을 받는 
        else
        {
            currentLife = (int)stream.ReceiveNext();
            Debug.Log("receive life " + currentLife);
        }
    }
}
public enum UnitType
{
    NONE, Player, Projectile
}