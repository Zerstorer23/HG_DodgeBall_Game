using Photon.Pun;
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
    public Controller controller;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        health = GetComponent<HealthPoint>();
        movement = GetComponent<Projectile_Movement>();
        controller = GetComponent<Controller>();
    }
    private void OnEnable()
    {
        if (pv.InstantiationData.Length < 3) return;
        fieldNo = (int)pv.InstantiationData[0];
        health.SetAssociatedField(fieldNo);
        movement.SetAssociatedField(fieldNo);
        string playerID = (string)pv.InstantiationData[1];
        bool followPlayer = (bool)pv.InstantiationData[2];
        player = GameFieldManager.gameFields[fieldNo].playerSpawner.GetUnitByControllerID(playerID);

        if (player != null)
        {
            controller.SetControllerInfo(player.controller.IsBot, playerID);
            Debug.LogWarning("ctrl info set " + playerID);
            if (followPlayer)
            {
                player.SetMyProjectile(gameObject);
            }
            player.AddProjectile(gameObject.GetInstanceID(), health);
        }
        else
        {
            controller.SetControllerInfo(PhotonNetwork.MasterClient);
            transform.SetParent(GameSession.GetBulletHome());
        }
    }
    private void OnDisable()
    {
        if (player != null)
        {
            player.RemoveProjectile(gameObject.GetInstanceID());
        }
    }

    [PunRPC]
    public void ResetRotation() {
        gameObject.transform.localRotation = Quaternion.identity;
    }

    [SerializeField] SpriteRenderer bodySprite;
    public void SetColor(Color color) {
        bodySprite.color = color;
    }


}
