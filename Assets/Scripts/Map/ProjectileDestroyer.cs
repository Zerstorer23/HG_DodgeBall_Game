using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class ProjectileDestroyer : MonoBehaviourPun
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;
        if (tag == TAG_PLAYER || tag == TAG_PROJECTILE)
        {
            HealthPoint hp = collision.GetComponent<HealthPoint>();
            if (hp == null || hp.dontKillByException) return;
            hp.Kill_Immediate();
        }
    }
}
