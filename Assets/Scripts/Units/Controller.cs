using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviourPun
{
    public ControllerType controllerType = ControllerType.Human;
    public string uid;
    UniversalPlayer uPlayer;
    public UniversalPlayer Owner {
        get => GetPlayer();
        private set => uPlayer = value;
    }



    public void SetControllerInfo(bool isBot, string uid) {
        controllerType = (isBot) ? ControllerType.Bot : ControllerType.Human;
        this.uid = uid;
        if (isBot) {
            Owner = PlayerManager.GetPlayerByID(uid);
        }
    }
    public void SetControllerInfo(string uid)
    {
        controllerType = ControllerType.Bot;
        this.uid = uid;
        Owner = PlayerManager.GetPlayerByID(uid);
    }
    public void SetControllerInfo(Player player)
    {
        controllerType = ControllerType.Human;
        this.uid = player.UserId;
    }

    private UniversalPlayer GetPlayer()
    {
        if (IsBot)
        {
            return uPlayer;
        }
        else {
            return PlayerManager.GetPlayerByID(photonView.Owner.UserId);
        }
    }

    public bool IsLocal
    {
        get { return (photonView.IsMine && controllerType == ControllerType.Human); }
    }
    public bool IsBot
    {
        get { return controllerType == ControllerType.Bot; }
    }
    public bool IsMine {
         get=> photonView.IsMine;
    }
    public bool Equals(string compareID) => this.uid == compareID;
    public bool Equals(Controller controller) => this.uid == controller.uid;
    public bool Equals(UniversalPlayer user) => this.uid == user.uid;



}

public enum ControllerType { 
    Human,Bot
}