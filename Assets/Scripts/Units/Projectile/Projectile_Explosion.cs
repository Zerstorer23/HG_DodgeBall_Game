using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static ConstantStrings;
public class Projectile_Explosion : MonoBehaviourPun
{
    Projectile_DamageDealer damageDealer;
    private void Awake()
    {
        damageDealer = GetComponent<Projectile_DamageDealer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);

    }
    void HandleCollision(GameObject go)
    {
        string tag = go.tag;
        HealthPoint otherHP;
        switch (tag)
        {
            case TAG_PLAYER:
                otherHP = go.GetComponent<HealthPoint>();
                if (!otherHP.pv.IsMine)
                {
                    DoExplosion();
                }
                break;
            case TAG_PROJECTILE:
                otherHP = go.GetComponent<HealthPoint>();
                if (!otherHP.IsMapProjectile() && !otherHP.pv.IsMine)
                {
                    DoExplosion();
                }
                break;
            case TAG_BOX_OBSTACLE:
            case TAG_WALL:
                DoExplosion();
                break;
        }
    }
    public float radius = 2f;
    void DoExplosion() {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position,radius, LayerMask.GetMask("Player", "Projectile"));
        for (int i = 0; i < collisions.Length; i++)
        {
            Collider2D c = collisions[i];
            HealthPoint healthPoint = c.gameObject.GetComponent<HealthPoint>();
            //if (healthPoint == null) return;
            switch (c.gameObject.tag)
            {
                case TAG_PLAYER:
                    damageDealer.GiveDamage(healthPoint);
                    if (photonView.IsMine) {

                        PhotonNetwork.Instantiate(PREFAB_EXPLOSION_1, transform.position, Quaternion.identity, 0);
                    }
                    //  damageDealer.DoPlayerCollision(c.gameObject);
                    break;
                case TAG_PROJECTILE:
                    damageDealer.GiveDamage(healthPoint);
                    PhotonNetwork.Instantiate(PREFAB_EXPLOSION_1, transform.position, Quaternion.identity, 0);
                    //   damageDealer.DoProjectileCollision(c.gameObject);
                    //healthPoint.KillImmm...
                    break;
            }

        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
