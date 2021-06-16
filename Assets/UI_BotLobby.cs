using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_BotLobby : MonoBehaviourPunCallbacks
{
    public override void OnJoinedRoom()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        OnAddBot();
        OnAddBot();
        OnAddBot();
        OnAddBot();
        OnRemoveBot();
    }

    public void OnAddBot() {
        if (!PhotonNetwork.IsMasterClient) {
            return;
        }
        string newID = PlayerManager.PollBotID();
        photonView.RPC("AddBotPlayer", RpcTarget.AllBuffered, newID);
        Debug.LogWarning("RPC add bot");
    }
    public void OnRemoveBot() {
        UniversalPlayer[] bots = PlayerManager.GetBotPlayers();
        if (bots.Length > 0)
        {
            photonView.RPC("RemoveBotPlayer", RpcTarget.AllBuffered, bots[0].uid);
        }
    }

    [PunRPC]
    public void AddBotPlayer(string uid) {
        UniversalPlayer botPlayer = new UniversalPlayer(uid);
        PlayerManager.AddBotPlayer(botPlayer);
    }

    [PunRPC]
    public void RemoveBotPlayer(string uid) {
        PlayerManager.RemoveBotPlayer(uid);
    }
}
