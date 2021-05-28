using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testconnect : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {

        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        //PhotonNetwork.JoinLobby(TypedLobby.Default);
        Debug.Log("Connected to master");
        JoinRoom();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Try join room");
        JoinRoom();
    }
    public void JoinRoom()
    {

        ExitGames.Client.Photon.Hashtable hash = UI_MapOptions.GetInitOptions();
        RoomOptions roomOpts = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = (byte)18,
            PublishUserId = true,
            CustomRoomProperties = hash
        };
        PhotonNetwork.JoinOrCreateRoom("Default", roomOpts, TypedLobby.Default);

    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Connected");
    }

}
