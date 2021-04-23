using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_GunHandler : MonoBehaviour
{
    GameObject myBullet;
    [SerializeField] Unit_Player player;
    public void SetProjectileObject(GameObject obj) {
        myBullet = obj;
    }
    public void DestroyObject() {
        if (player.pv.IsMine) {
            PhotonNetwork.Destroy(myBullet);        
        }
    }

}
