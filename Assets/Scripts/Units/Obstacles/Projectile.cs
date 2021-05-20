﻿using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviourPun
{
    internal Unit_Player player;
    public PhotonView pv;
    int fieldNo = 0;
    HealthPoint health;
    Projectile_Movement movement;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        health = GetComponent<HealthPoint>();
        movement = GetComponent<Projectile_Movement>();
    }
    private void OnEnable()
    {
        if (pv.InstantiationData.Length < 3) return;
        fieldNo = (int)pv.InstantiationData[0];
        health.SetAssociatedField(fieldNo);
        movement.SetAssociatedField(fieldNo);
        string playerID = (string)pv.InstantiationData[1];
        bool followPlayer = (bool)pv.InstantiationData[2];
        player = GameFieldManager.gameFields[fieldNo].playerSpawner.GetPlayerByOwnerID(playerID);
        if (followPlayer && player != null)
        {
            player.SetMyProjectile(gameObject);
        }
        else {
            transform.SetParent(GameSession.GetBulletHome());
        }
    }

    [PunRPC]
    public void ResetRotation() {
        gameObject.transform.localRotation = Quaternion.identity;
    }



}
