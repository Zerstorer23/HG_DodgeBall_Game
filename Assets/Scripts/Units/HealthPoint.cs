using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPoint : MonoBehaviourPun
{
    public UnitType unitType;
    int maxLife = 1;
    public int currentLife;
    public bool isDead = false;
    public bool invincibleFromBullets = false;

    internal PhotonView pv;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
    }

    private void OnGameEnd(EventObject arg0)
    {
       // DoDeath(null);
    }

    public void SetMaxLife(int life) {
        maxLife = life;
        currentLife = maxLife;
    }
    public int GetMaxLife() {
        return maxLife;
    }
    private void OnEnable()
    {
        currentLife = maxLife;
    //    unitType = DetermineType();
        isDead = false;
        if (unitType == UnitType.Projectile) currentLife = 1;
    }
    
   private void OnDisable()
    {
        isDead = true;

      //  Debug.Log("disabled " + GetComponent<PhotonView>().ViewID);
    }
    /*   private void OnDestroy()
      {

          Debug.Log("destroyted " + GetComponent<PhotonView>().ViewID);
      }*/
/*    UnitType DetermineType() {
        if (GetComponent<Projectile>() != null) {
            return UnitType.Projectile;
        }
        if (GetComponent<Unit_Player>() != null) {
            return UnitType.Player;
        }
        return UnitType.NONE;
    }*/

    internal void DoDamage(Unit_Player attackedBy, bool instaDeath = false)
    {
        if (!pv.IsMine || invincibleFromBullets) return;
        pv.RPC("ChangeHP", RpcTarget.AllBuffered, -1);
        if (unitType == UnitType.Player) {
            Debug.Log("Instanitate heal");
            PhotonNetwork.Instantiate(ConstantStrings.PREFAB_HEAL_1, transform.position, Quaternion.identity, 0);
        }

        if (currentLife <= 0 || instaDeath) DoDeath(attackedBy);
    }
    public void DeathByException() {
        if (!pv.IsMine || isDead) return;
        isDead = true;
        PhotonNetwork.Destroy(pv);

    }
    private IEnumerator WaitAndKill(float delay)
    {
        yield return new WaitForSeconds(delay); 
        DeathByException();
    }
    [PunRPC]
    private void DoDeathAfter(float delay)
    {
        StartCoroutine(WaitAndKill(delay));
    }

    void DoDeath(Unit_Player attackedBy)
    {
        if (!pv.IsMine || isDead) return;
        isDead = true;
        if (attackedBy != null) {
            attackedBy.IncrementKill();
        }
        try
        {
            PhotonNetwork.Destroy(pv);
        }
        catch (Exception e)
        {
            Debug.Log(e.StackTrace);

        }
    }
    [PunRPC]
    public void ChangeHP(int a) {
        currentLife += a;
    }
}
public enum UnitType { 
 NONE,Player,Projectile
}