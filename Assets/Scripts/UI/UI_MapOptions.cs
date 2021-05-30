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
    public static int default_lives_index = 1;
    public static MapDifficulty default_difficult = MapDifficulty.Easy;
   // public int playerLives;
   // public MapDifficulty mapDifficulty;
    bool gameStarted = false;
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

    public void SetGameStarted(bool enable) {
        gameStarted = enable;
    }
    public bool GetGameStarted() => gameStarted;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        forceStart.SetActive(false);
    }
    private void OnEnable()
    {
        UpdateSettingsUI();
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
        int gmode = 0;
        if (GameSession.gameModeInfo != null) {
            gmode = (int)GameSession.gameModeInfo.gameMode;
        }
        gamemodeDropdown.SetValueWithoutNotify(gmode);
    }

    internal void LoadRoomSettings()
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        mapDiff = (MapDifficulty)hash[HASH_MAP_DIFF];
        livesIndex= (int)hash[HASH_PLAYER_LIVES];
        string versionCode = (string)hash[HASH_VERSION_CODE];
        if (versionCode != GameSession.GetVersionCode())
        {
            Debug.Log("Received Wrong " + versionCode); 
            PhotonNetwork.NickName = string.Format(
            "<color=#ff0000>클라이언트 버전</color>이 맞지않습니다. 방장 버전 {0}",
             versionCode);
        }
        gameStarted = (bool)hash[HASH_GAME_STARTED];
        GameSession.gameModeInfo = ConfigsManager.gameModeDictionary[(GameMode)hash[HASH_GAME_MODE]];
        Debug.Log("난입유저 룸세팅 동기화 끝");
        UpdateSettingsUI();
    }

    public void OnClick_ChangeTeam()
    {
        if (UI_PlayerLobbyManager.localPlayerInfo == null) return;
        if (!GameSession.gameModeInfo.isTeamGame) return;
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
        GameSession.gameModeInfo = ConfigsManager.gameModeDictionary[(GameMode)index];
        EventManager.TriggerEvent(MyEvents.EVENT_GAMEMODE_CHANGED, new EventObject() { objData = GameSession.gameModeInfo });
        UpdateSettingsUI();
    }
    public static ExitGames.Client.Photon.Hashtable GetInitOptions()
    {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(HASH_MAP_DIFF, default_difficult);
        hash.Add(HASH_PLAYER_LIVES, default_lives_index);
        hash.Add(HASH_VERSION_CODE, GameSession.GetVersionCode());
        hash.Add(HASH_GAME_MODE, GameMode.PVP);
        hash.Add(HASH_GAME_STARTED, false);
        hash.Add(HASH_GAME_AUTO, false);
        hash.Add(HASH_ROOM_RANDOM_SEED, UnityEngine.Random.Range(0,7));
        return hash;
    }
    public void PushRoomSettings()
    {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(HASH_MAP_DIFF, mapDiff);
        hash.Add(HASH_PLAYER_LIVES, livesIndex);
        hash.Add(HASH_VERSION_CODE, GameSession.GetVersionCode());
        hash.Add(HASH_GAME_STARTED, gameStarted);
        hash.Add(HASH_GAME_MODE, GameSession.gameModeInfo.gameMode);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }


}
