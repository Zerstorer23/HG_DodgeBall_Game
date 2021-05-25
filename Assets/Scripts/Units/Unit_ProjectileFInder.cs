using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_ProjectileFInder : MonoBehaviour
{
    [SerializeField] Unit_Player player;
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        HealthPoint proj = collision.gameObject.GetComponent<HealthPoint>();
        Debug.Log(proj.gameObject.name);
        if (proj != null) {
            bool valid;
            if (proj.damageDealer == null) return;
            if (proj.damageDealer.isMapObject) {
                valid = true;
            }else if (GameSession.gameMode == GameMode.TEAM)
            {
                valid = (proj.myTeam == player.myTeam);
            }
            else {
                valid = (!proj.pv.AmOwner);
            }
            if (valid) {
                player.IncrementEvasion();
            }

        }
    }
}
