using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPoint : MonoBehaviourPun
{
    UnitType unitType;
    int maxLife = 1;
    public int currentLife;
    public bool isDead = false;
    internal PhotonView pv;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    public void SetMaxLife(int life) {
        maxLife = life;
        currentLife = maxLife;
    }
    private void OnEnable()
    {
        currentLife = maxLife;
        unitType = DetermineType();
        isDead = false;
        if (unitType == UnitType.Projectile) currentLife = 1;
    }
    UnitType DetermineType() {
        if (GetComponent<Projectile>() != null) {
            return UnitType.Projectile;
        }
        if (GetComponent<Unit_Player>() != null) {
            return UnitType.Player;
        }
        return UnitType.NONE;
    }

    internal void DoDamage(bool instaDeath = false)
    {
        currentLife--;
        if (currentLife <= 0 || instaDeath) DoDeath();
    }
   void DoDeath()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient || isDead) return;
        isDead = true;
        PhotonNetwork.Destroy(gameObject);
    }
}
public enum UnitType { 
 NONE,Player,Projectile
}