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
    string exclusionPlayerID;
    private void Awake()
    {
        projectile = GetComponent<Projectile>();
        movement = GetComponent<Projectile_Movement>();
        myHealth = GetComponent<HealthPoint>();
        Debug.Assert(projectile != null, "Where is projectile");
    }
    [PunRPC]
    public void SetExclusionPlayer(string playerID) {

        exclusionPlayerID = playerID;
    }

    // Start is called before the first frame update
    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        string tag = collision.gameObject.tag;
        switch (tag)
        {
            case TAG_PLAYER:
                //Kill him and me
                DoDamage(collision.gameObject);
                break;
            case TAG_BOX_OBSTACLE:
                //Bounce
                ContactPoint2D[] contacts = new ContactPoint2D[10];
                collision.GetContacts(contacts);
                movement.Bounce(contacts[0],collision.gameObject.transform.position);
                break;

        }

    }

    private void DoDamage(GameObject obj)
    {
        HealthPoint health = obj.GetComponent<HealthPoint>();
        if (health == null) return;
        if (health.pv.Owner.UserId == exclusionPlayerID) return;
        health.DoDamage();
        myHealth.DoDamage();

    }

  

}
