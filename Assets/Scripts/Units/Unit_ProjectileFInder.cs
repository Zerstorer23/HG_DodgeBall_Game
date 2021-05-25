using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_ProjectileFinder : MonoBehaviour
{
    [SerializeField] Unit_Player player;

    private void OnEnable()
    {
        gameObject.SetActive(player.pv.IsMine);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        HealthPoint proj = collision.gameObject.GetComponent<HealthPoint>();
        if (proj != null) {
            if (proj.damageDealer == null) return;
            if (proj.damageDealer.isMapObject) {
                player.IncrementEvasion();
            }
        }
    }
}
