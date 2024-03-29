﻿using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using static ConstantStrings;
using ExitGames.Client.Photon;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public static int MAX_PLAYER_PER_ROOM = 18;
    [SerializeField] GameObject loadingChuu;
    [SerializeField] GameObject[] disableInLoading;
    const string PRIMARY_ROOM = "PrimaryRoom";
    [SerializeField] UI_PlayerLobbyManager lobbyManager;
    [SerializeField] UI_MapOptions mapOptions;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected) {
            DoLoading();
        }
    }

    private void DoLoading()
    {
        loadingChuu.SetActive(true);
        foreach (GameObject go in disableInLoading)
        {
            go.SetActive(false);
        }
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (PlayerManager.embarkCalled) return;
        JoinRoom();
    }
    public void OnClickLeaderBoard() {
        GooglePlayManager.ShowLeaderBoard();
    }

    public static void JoinRoom()
    {

        Hashtable hash = UI_MapOptions.GetInitOptions(); 
        RoomOptions roomOpts = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = (byte)MAX_PLAYER_PER_ROOM,
            PublishUserId = true,
            CustomRoomProperties = hash
        };
        PhotonNetwork.JoinOrCreateRoom(PRIMARY_ROOM, roomOpts, TypedLobby.Default);

    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.NickName = UI_ChangeName.default_name;
        PhotonNetwork.SendRate = 60; //60 / 60 on update
        PhotonNetwork.SerializationRate = 60; // 32 32 on fixed
        loadingChuu.SetActive(false);
        foreach (GameObject go in disableInLoading)
        {
            go.SetActive(true);
        }
        lobbyManager.ConnectedToRoom();
    }
    public static string GetLocalName()
    {
        return PhotonNetwork.LocalPlayer.NickName;
    }

    [SerializeField] Toggle autoToggle;
    public void OnAutoToggle()
    {
        GameSession.auto_drive_enabled = autoToggle.isOn;
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {

        lobbyManager.OnRoomPropertiesChanged();
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        lobbyManager.OnPlayerLeftRoom(newPlayer);
        mapOptions.UpdateSettingsUI(); //Player Leave room
    }
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("Master changed");
        if (PhotonNetwork.IsMasterClient) {
            GameSession.PushRoomASetting(HASH_ROOM_RANDOM_SEED, Random.Range(0, 133));
        }
        PlayerManager.LocalPlayer.SetCustomProperties("SEED", Random.Range(0, 133));
        mapOptions.UpdateSettingsUI(); //Player Leave room
    }


    public void OnClick_OpenSettings() {
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.Settings, boolObj = true });
    
    }
}


public enum MapDifficulty
{
    None = 0, BoxOnly,Easy, Standard, Hard
}
