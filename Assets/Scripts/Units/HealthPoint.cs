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
    internal bool isHomeTeam = true;



    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        buffManager = GetComponent<BuffManager>();
        unitPlayer = GetComponent<Unit_Player>();
        damageDealer = GetComponent<Projectile_DamageDealer>();
    }
    private void Start()
    {

        isHomeTeam = unitPlayer.isHomeTeam;
    }

    private void OnGameFinish(EventObject obj)
    {
        if (!isDead)
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
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinish);
        currentLife = maxLife;
        //    unitType = DetermineType();
        isDead = false;
        if (unitType == UnitType.Projectile) currentLife = 1;
    }

    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinish);
        isDead = true;
    }

    internal void HealHP(int amount)
    {
        if (currentLife < maxLife)
        {
            if (pv.IsMine)
            {
                pv.RPC("ChangeHP", RpcTarget.AllBuffered, amount);
            }
        }
    }
    public bool IsInvincible() {
        return (invincibleFromBullets || buffManager.GetTrigger(BuffType.InvincibleFromBullets));    
    }
    int expectedlife;
   [PunRPC]
    internal void DoDamage(string attackerUserID, bool instaDeath)
    {
        if (isDead || IsInvincible()) return;
        expectedlife = currentLife - 1;

        if (pv.IsMine) {
            pv.RPC("ChangeHP", RpcTarget.AllBuffered, -1);
        }
//        ChangeHP(-1);

        if (unitType == UnitType.Player && pv.IsMine)
        {
            PhotonNetwork.Instantiate(ConstantStrings.PREFAB_HEAL_1, transform.position, Quaternion.identity, 0);
            MainCamera.instance.DoShake();
            unitPlayer.PlayHitAudio();
        }
        NotifySourceOfDamage(attackerUserID);
        if (expectedlife <= 0 || instaDeath)
        {
            NotifySourceOfDeath(attackerUserID);
        }
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

    void NotifySourceOfDamage(string attackerUserID)
    {
        if (unitType != UnitType.Player) return;
        if (pv.IsMine)
        {
            if (attackerUserID == null)
            {
                Debug.Log("Am attacked!");
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = "회피실패" });
                if (expectedlife <= 0)
                {
                    ChatManager.SendNotificationMessage(PhotonNetwork.NickName + "님이 사망했습니다.", "#FF0000");
                }
            }
            else
            {
                Player p = ConnectedPlayerManager.GetPlayerByID(attackerUserID);
                string attackerNickname = "???";
                if (p != null)
                {
                    attackerNickname = p.NickName;
                }
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = string.Format("{0}에게 피격", attackerNickname) });
                if (expectedlife <= 0)
                {
                    ChatManager.SendNotificationMessage(attackerNickname + " 님이 " + PhotonNetwork.NickName + "님을 살해했습니다.", "#FF0000");
                }

            }

        }
        else if (PhotonNetwork.LocalPlayer.UserId == attackerUserID)
        {
            EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = string.Format("{0}를 타격..!", pv.Owner.NickName) });
        }

    }
    private void NotifySourceOfDeath(string attackerUserID)
    {
        if (attackerUserID == null) return;
        if (PhotonNetwork.LocalPlayer.UserId == attackerUserID)
        {
            if (unitType == UnitType.Player)
            {
                Unit_Player player = PlayerSpawner.GetPlayers()[attackerUserID];
                if (player == null) return;
                player.IncrementKill();
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