using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class ProjectileDestroyer : MonoBehaviourPun
{
    // Start is called before the first frame update
    private void OnCollisionEnter2D(Collision2D collision)
    {

        Debug.Log("Destoryer Collision " + collision.gameObject.layer);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        string tag = collision.gameObject.tag;
        switch (tag)
        {
            case TAG_PROJECTILE:
                collision.gameObject.GetComponent<HealthPoint>().DeathByException();
                break;
        }
    }
}
