using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class UI_MapOptions : MonoBehaviourPun
{
    //*****GAME SETTING***//
    public static readonly int default_lives_index = 1;
    public static readonly MapDifficulty default_difficult = MapDifficulty.None;
   // public int playerLives;
   // public MapDifficulty mapDifficulty;
    public bool gameStarted = false;
    PhotonView pv;


    [SerializeField] GameObject forceStart;
    [SerializeField] Text mapDiffText;
    [SerializeField] Text playerDiffText;
    [SerializeField] Dropdown mapDiffDropdown;
    [SerializeField] Dropdown livesDropdown;
    [SerializeField] Dropdown gamemodeDropdown;

   public MapDifficulty mapDiff;
   public int livesIndex =0;

    public static int[] lives = new int[] { 1, 3, 5 };

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        forceStart.SetActive(false);
        mapDiff = default_difficult;
        livesIndex = default_lives_index;
    }

    public void OnDropdown_MapDifficulty()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int index = mapDiffDropdown.value;
            pv.RPC("SetMapDifficulty", RpcTarget.AllBuffered, index);
            ChatManager.SendNotificationMessage(string.Format("난이도가 {0}로 변경되었습니다.", mapDiff));
        }
    }
    public void OnDropdown_PlayerDifficulty()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int index = livesDropdown.value;
            pv.RPC("SetPlayerLives", RpcTarget.AllBuffered, index);
            ChatManager.SendNotificationMessage(string.Format("라이프가 {0}로 변경되었습니다.", lives[livesIndex]));
        }
    }
    public void OnDropdown_GameMode()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int index = gamemodeDropdown.value;
            pv.RPC("SetGameMode", RpcTarget.AllBuffered, index);
            ChatManager.SendNotificationMessage(string.Format("게임모드가 {0}로 변경되었습니다.", (GameMode)index));
        }
    }
    public void UpdateSettingsUI()
    {
        bool isMaster = PhotonNetwork.LocalPlayer.IsMasterClient;

        mapDiffDropdown.interactable = isMaster;
        livesDropdown.interactable = isMaster;
        gamemodeDropdown.interactable = isMaster;
        forceStart.SetActive(isMaster);
        mapDiffDropdown.SetValueWithoutNotify((int)mapDiff);
        livesDropdown.SetValueWithoutNotify((int)livesIndex);
        gamemodeDropdown.SetValueWithoutNotify((int)GameSession.gameMode);
    }
    public void OnClick_ChangeTeam()
    {
        if (UI_PlayerLobbyManager.localPlayerInfo == null) return;
        if (GameSession.gameMode != GameMode.TEAM) return;
        UI_PlayerLobbyManager.localPlayerInfo.pv.RPC("ToggleTeam", RpcTarget.AllBuffered);
    }
    [PunRPC]
    public void SetMapDifficulty(int diff) {
        mapDiff = (MapDifficulty)diff;
        UpdateSettingsUI();
    }

    [PunRPC]
    public void SetPlayerLives(int index)
    {
        livesIndex =index;
        UpdateSettingsUI();
    }
    [PunRPC]
    public void SetGameMode(int index)
    {
        GameSession.gameMode = (GameMode)index;
        EventManager.TriggerEvent(MyEvents.EVENT_GAMEMODE_CHANGED, new EventObject() { objData = GameSession.gameMode });
        UpdateSettingsUI();
    }
    public static ExitGames.Client.Photon.Hashtable GetInitOptions()
    {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(HASH_MAP_DIFF, default_difficult);
        hash.Add(HASH_PLAYER_LIVES, lives[default_lives_index]);
        hash.Add(HASH_VERSION_CODE, GameSession.GetVersionCode());
        hash.Add(HASH_GAME_MODE, GameMode.PVP);
        hash.Add(HASH_GAME_STARTED, false);
        hash.Add(HASH_GAME_AUTO, false);
        return hash;
    }
    public void PushRoomSettings()
    {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(HASH_MAP_DIFF, mapDiff);
        hash.Add(HASH_PLAYER_LIVES, lives[livesIndex]);
        hash.Add(HASH_VERSION_CODE, GameSession.GetVersionCode());
        hash.Add(HASH_GAME_STARTED, gameStarted);
        hash.Add(HASH_GAME_MODE, GameSession.gameMode);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }


}
