using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DebugAutoConnect : MonoBehaviourPunCallbacks
{
    const string PRIMARY_ROOM = "PrimaryRoom";

    string default_name = "ㅇㅇ";

    private void Awake()
    {
        if(!PhotonNetwork.IsConnected)
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master!");
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Connected to Lobby");
        JoinRoom();
    }
    public void JoinRoom()
    {
        RoomOptions roomOpts = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 10,
            PublishUserId = true
        };
        PhotonNetwork.JoinOrCreateRoom(PRIMARY_ROOM, roomOpts, TypedLobby.Default);

    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Connected to Room");

    }
}
