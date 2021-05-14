using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviourPun
{
    internal Unit_Player player;
    public PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

    }
    [PunRPC]
    public void SetParentPlayer(string owner)
    {
        Unit_Player parent = GameSession.GetInst().charSpawner.GetPlayerByOwnerID(owner);
        if (parent != null)
        {
            parent.SetMyProjectile(gameObject);
        }
    }
    [PunRPC]
    public void ResetRotation() {
        gameObject.transform.localRotation = Quaternion.identity;
    }

    [PunRPC]
    public void SetParentTransform()
    {
        transform.SetParent(GameSession.GetInst().Home_Bullets);
    }

    [PunRPC]
    public void SetOwnerPlayer(string ownerID)
    {
        player = GameSession.GetPlayerByID(ownerID);
    }

}
