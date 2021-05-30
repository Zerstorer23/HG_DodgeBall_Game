using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Projectile_DamageDealer : MonoBehaviourPun
{
    Projectile projectile;
    Projectile_Movement movement;
    HealthPoint myHealth;
    [SerializeField] string exclusionPlayerID;
    [SerializeField] bool canKillBullet = false;
    public bool isMapObject = false;
    public bool givesDamage = true;
    PhotonView pv;

    public List<BuffData> customBuffs = new List<BuffData>();
    // [SerializeField] bool nonRecyclableDamage = false;

    public Collider2D myCollider;
    bool hasCustomCollider = false;
    private void Awake()
    {
        FindCollider();
        projectile = GetComponent<Projectile>();
        movement = GetComponent<Projectile_Movement>();
        myHealth = GetComponent<HealthPoint>();
        pv = GetComponent<PhotonView>();
        Debug.Assert(projectile != null, "Where is projectile");
        hasCustomCollider = GetComponent<Projectile_CustomDamageDealer>() != null;
    }

    private void OnDisable()
    {
        exclusionPlayerID = "";
        myCollider.enabled = false;
    }
    private void OnEnable()
    {
        if (!isMapObject) {
            exclusionPlayerID = pv.Owner.UserId;
        }
        myCollider.enabled = true;
        attackedIDs = new Dictionary<string, double>();
    }
    public Dictionary<string,double> attackedIDs = new Dictionary<string, double>();
    void FindCollider()
    {
        myCollider = GetComponent<PolygonCollider2D>();
        if (myCollider == null)
        {
            myCollider = GetComponent<CircleCollider2D>();
        }
        if (myCollider == null)
        {
            myCollider = GetComponent<BoxCollider2D>();
        }
        if (myCollider == null)
        {
            myCollider = GetComponent<CapsuleCollider2D>();
        }
    }

    /*
        [PunRPC]
        public void SetExclusionPlayer(string playerID) {
            exclusionPlayerID = playerID;
        }*/

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {

        string tag = collision.gameObject.tag;
       // Debug.Log(gameObject.name + "Trigger with " + collision.gameObject.name+" / tag "+tag);
        switch (tag)
        {
            case TAG_PLAYER:
                if (!hasCustomCollider) {
                    DoPlayerCollision(collision.gameObject);
                }
                break;
            case TAG_PROJECTILE:
                    DoProjectileCollision(collision.gameObject);
          
                break;
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        ContactPoint2D contact;        
         // Debug.Log(gameObject.name + "Collision with " + collision.gameObject.name + " / tag " + tag);
        switch (tag)
        {
            case TAG_PLAYER:
                //Kill him and me
                if (!hasCustomCollider)
                {
                    DoPlayerCollision(collision.gameObject);
                    if (isMapObject) myHealth.Kill_Immediate();
                }
                break;
            case TAG_PROJECTILE:
                DoProjectileCollision(collision.gameObject);
                if (movement.reactionType == ReactionType.Bounce)
                {
                    contact = collision.GetContact(0);
                    DoBounceCollision(contact, collision.gameObject.transform.position);
                }
                else if (movement.reactionType == ReactionType.Die)
                {
                    Projectile_DamageDealer targetdd = collision.gameObject.GetComponent<Projectile_DamageDealer>();
                    if (targetdd.isMapObject && myHealth.invincibleFromMapBullets) break;
                    myHealth.Kill_Immediate();
                }
                break;
            case TAG_BOX_OBSTACLE:
                //Bounce
                contact = collision.GetContact(0);
                //   Debug.Log("Contact at" + contact.point);
                DoBounceCollision(contact, collision.gameObject.transform.position);
                break;

        }
    }
    bool CheckValidDamageEvaluation(HealthPoint otherHP) {
        if (isMapObject)
        {
            if (!otherHP.pv.IsMine) return false; // 맵오브젝트는 각자 알아서
        }
        else
        {
            if (!pv.IsMine) return false; // 개인투사체는 주인이처리
        }
        return true;
    }

    public void DoProjectileCollision(GameObject targetObj)
    {
        HealthPoint otherHP = targetObj.GetComponent<HealthPoint>();
        if (!CheckValidTeam(otherHP)) return;
        GiveDamage(otherHP);
    }

    public void DoPlayerCollision(GameObject targetObj)
    {
        HealthPoint otherHP = targetObj.GetComponent<HealthPoint>();
        if(!CheckValidDamageEvaluation(otherHP)) return ;
        if (!CheckManifoldDamage(otherHP)) return ;
        if (!CheckValidTeam(otherHP)) return ;
        ApplyBuff(otherHP);
        if (!givesDamage) return;
        GiveDamage(otherHP);
    }
    bool CheckValidTeam(HealthPoint otherHP) {
        if (GameSession.gameModeInfo.isCoop)
        {
            if (isMapObject) return true;//맵 ->아무거나 무조건 딜
            if (otherHP.unitType == UnitType.Projectile)
            {
                if (otherHP.damageDealer.isMapObject) return true; //아무거나 -> 맵 무조건 딜
            }
            return (otherHP.myTeam != myHealth.myTeam); //그외 팀구분
        }
        return true;
    
    
    }
    bool CheckManifoldDamage(HealthPoint otherHP) {
        string uid = otherHP.pv.Owner.UserId;
        double curr = PhotonNetwork.Time;
        if (attackedIDs.ContainsKey(uid))
        {
         /*   if (curr - attackedIDs[uid] >= 1.25d) {
                attackedIDs[uid] = curr;
                return true;
            }*/
            return false;
        }
        else {
            attackedIDs.Add(uid, curr);
            return true;
        }
    }

    void ApplyBuff(HealthPoint otherHP) {
        if (customBuffs.Count <= 0) return;
        BuffManager targetManager = otherHP.buffManager;
        if (targetManager == null || !targetManager.gameObject.activeInHierarchy) return;
        if (targetManager.pv.Owner.UserId == exclusionPlayerID) return;
        foreach (BuffData buff in customBuffs)
        {
            targetManager.pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)buff.buffType, buff.modifier, buff.duration);
        }
    }


    void DoBounceCollision(ContactPoint2D contact, Vector3 collisionPoint)
    {
        if (movement == null) return;
        movement.Bounce2(contact, collisionPoint); 
    }

    private void GiveDamage(HealthPoint otherHP)
    {
        if (otherHP.unitType == UnitType.Player)
        {

            if (otherHP.pv.Owner.UserId == exclusionPlayerID) return;
            string sourceID = (isMapObject) ? null : pv.Owner.UserId;
      //      Debug.Log("Damage player");
            otherHP.pv.RPC("DoDamage", RpcTarget.AllBuffered, sourceID, false);
            if (isMapObject) myHealth.Kill_Immediate();
        }
        else if(canKillBullet){
            if (otherHP.damageDealer.isMapObject )
            {
//                Debug.Log("Damage Map Proj");
                otherHP.Kill_Immediate();
            }
            if (movement.reactionType == ReactionType.Die)
            {
                if (otherHP.damageDealer.isMapObject && myHealth.invincibleFromMapBullets) return;
                myHealth.Kill_Immediate();
            }
        }
        return ;
    }



}
