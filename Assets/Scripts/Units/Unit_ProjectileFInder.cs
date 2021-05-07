using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_ProjectileFInder : MonoBehaviour
{
    [SerializeField] Unit_Player player;
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        Projectile proj = collision.gameObject.GetComponent<Projectile>();
        if (proj != null) {
            if (proj.pv.CreatorActorNr != PhotonNetwork.LocalPlayer.ActorNumber) {

                player.IncrementEvasion();
            }

        }
    }
}
