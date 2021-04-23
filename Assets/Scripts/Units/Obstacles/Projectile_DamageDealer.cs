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
    public bool dieOnCollision = true;

    public List<BuffData> customBuffs = new List<BuffData>();
    private void Awake()
    {
        projectile = GetComponent<Projectile>();
        movement = GetComponent<Projectile_Movement>();
        myHealth = GetComponent<HealthPoint>();
        Debug.Assert(projectile != null, "Where is projectile");
    }
    private void OnEnable()
    {
        dieOnCollision = true;
    }
    private void OnDisable()
    {
        exclusionPlayerID = "";
    }

    [PunRPC]
    public void SetExclusionPlayer(string playerID) {
        exclusionPlayerID = playerID;
    }
    [PunRPC]
    public void DisableCollisionDeath()
    {
        dieOnCollision = false;
    }
    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {

      //  Debug.Log(gameObject.name+ "Trigger with " +collision.gameObject.name );
        string tag = collision.gameObject.tag;
        switch (tag)
        {
            case TAG_PLAYER:
                DoPlayerCollision(collision.gameObject);
                break;
            case TAG_PROJECTILE:
                DoProjectileCollision(collision.gameObject);
                break;
        }

    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
     ///  Debug.Log(gameObject.name + "Collision with " + collision.gameObject.name);
        switch (tag)
        {
            case TAG_PLAYER:
                //Kill him and me
                DoPlayerCollision(collision.gameObject);
                break;
            case TAG_PROJECTILE:
                DoProjectileCollision(collision.gameObject);
                break;
            case TAG_BOX_OBSTACLE:
                //Bounce
                ContactPoint2D contact = collision.GetContact(0);
                //   Debug.Log("Contact at" + contact.point);
                DoBounceCollision(contact, collision.gameObject.transform.position);
                break;

        }
    }

    private void DoProjectileCollision(GameObject targetObj)
    {
        if (!canKillBullet) return;
        GiveDamage(targetObj, projectile.player);
        ApplyBuff(targetObj);

    }

    void DoPlayerCollision(GameObject targetObj)
    {
        GiveDamage(targetObj, projectile.player);
        ApplyBuff(targetObj);
    }
    void ApplyBuff(GameObject targetObj) {
        if (customBuffs.Count <= 0) return;
        BuffManager targetManager = targetObj.GetComponent<BuffManager>();
        if (targetManager == null) return;
        if (targetManager.pv.Owner.UserId == exclusionPlayerID) return;
        foreach (BuffData buff in customBuffs) {
            targetManager.RPC_AddBuff(buff);
        }
    }


    void DoBounceCollision(ContactPoint2D contact, Vector3 collisionPoint)
    {
        if (movement == null) return;
        movement.Bounce2(contact, collisionPoint); 
    }

    private void GiveDamage(GameObject obj, Unit_Player attackedBy)
    {
        HealthPoint health = obj.GetComponent<HealthPoint>();
        if (health == null) return;
        if (health.unitType == UnitType.Player && health.pv.Owner.UserId == exclusionPlayerID) return;
        health.DoDamage(attackedBy);
        if (dieOnCollision)
        {
            myHealth.DoDamage(null);
        }

    }

  

}
