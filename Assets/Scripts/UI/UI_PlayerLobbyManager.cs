using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;
using System;

public class UI_PlayerLobbyManager : MonoBehaviourPun
{

    //***Players***//
    PhotonView pv;
    [SerializeField] GameObject localPlayerObject;
    [SerializeField] Text numOfPlayers;
    [SerializeField] Text numReadyText;

    [SerializeField] UI_MapOptions mapOptions;
    [SerializeField] UI_CharacterSelector charSelector;
    public static HUD_UserName localPlayerInfo;
    public List<string> foundPlayers = new List<string>();
    Dictionary<string, HUD_UserName> playerDictionary = new Dictionary<string, HUD_UserName>();
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        EventManager.StartListening(MyEvents.EVENT_PLAYER_JOINED, OnNewPlayerEnter);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_TOGGLE_READY, UpdateReadyStatus);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_JOINED, OnNewPlayerEnter);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_TOGGLE_READY, UpdateReadyStatus);
    }
    public void ConnectedToRoom()
    {
        mapOptions.LoadRoomSettings();
        ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
        //TODO
        playerHash.Add("TEAM", Team.HOME);
        playerHash.Add("CHARACTER", CharacterType.NONE);
        playerHash.Add("SEED", UnityEngine.Random.Range(0, 133));
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
        Debug.Log("Game started? " + mapOptions.GetGameStarted());
        if (!mapOptions.GetGameStarted())
        {
            Debug.Log("Instantiate after connection");
            InstantiateMyself();
            UpdateReadyStatus();
            mapOptions.UpdateSettingsUI();//I join room
        }
        else
        {
            //난입유저 바로시작
            GameFieldManager.SetGameMap(GameSession.gameModeInfo.gameMode);
            GameFieldManager.ChangeToSpectator();
            Debug.Log("난입세팅끝");
            StartCoroutine(WaitAndStartGame());
        }
    }

    private void OnEnable()
    {
        playerDictionary.Clear();
        if (PhotonNetwork.CurrentRoom == null) return;
        mapOptions.SetGameStarted(false);
        Debug.Log("Instantiate after regame");
        if (PhotonNetwork.IsMasterClient)
        {
         //  Player randomPlayer = PhotonNetwork.LocalPlayer.GetNext();// PlayerManager.GetRandomPlayerExceptMe();
            Player randomPlayer = PlayerManager.GetRandomPlayerExceptMe();
            if (randomPlayer != null)
                PhotonNetwork.SetMasterClient(randomPlayer);
        }

        InstantiateMyself();
        UpdateReadyStatus();//I enter room
        mapOptions.UpdateSettingsUI();
    }
    private void InstantiateMyself()
    {
        Debug.Assert(localPlayerObject == null, "PLayer obj not removed");
        localPlayerObject = PhotonNetwork.Instantiate(PREFAB_STARTSCENE_PLAYERNAME, Vector3.zero, Quaternion.identity, 0, new object[] { false, PhotonNetwork.LocalPlayer.UserId});
        localPlayerInfo = localPlayerObject.GetComponent<HUD_UserName>();
        localPlayerInfo.lobbyManager = this;
        string name = PhotonNetwork.NickName;
        CharacterType character = PlayerManager.LocalPlayer.GetProperty("CHARACTER",(GameSession.instance.devMode)?GameSession.instance.debugChara : CharacterType.NONE);
        Team myTeam = PlayerManager.LocalPlayer.GetProperty("TEAM", Team.HOME);
        localPlayerInfo.pv.RPC("ChangeName", RpcTarget.AllBuffered, name);
        localPlayerInfo.pv.RPC("ChangeCharacter", RpcTarget.AllBuffered, (int)character);
        localPlayerInfo.pv.RPC("SetTeam", RpcTarget.AllBuffered, (int)myTeam);
        charSelector.SetInformation(character);
        UpdateReadyStatus();
    }

    internal void OnPlayerLeftRoom(Player newPlayer)
    {
        RemovePlayer(newPlayer.UserId);

    }
    public void RemovePlayer(string uid) {
        if (playerDictionary.ContainsKey(uid)) {
            Debug.Assert(playerDictionary.ContainsKey(uid), "Removing p doesnt exist");
            playerDictionary.Remove(uid);
        }
        UpdateReadyStatus();
        debugUI();
    }

    [SerializeField] Transform playerListTransform;
    private void OnNewPlayerEnter(EventObject eo)
    {
        string id = eo.stringObj;
        HUD_UserName info = eo.goData.GetComponent<HUD_UserName>();
        if (playerDictionary.ContainsKey(id))
        {
            Debug.LogWarning("Add duplicate panel name?");
            playerDictionary[id] = info;
        }
        else
        {
            playerDictionary.Add(id, info);
        }
        eo.goData.GetComponent<Transform>().SetParent(playerListTransform, false);
        RebalanceTeam();
        UpdateReadyStatus();
        debugUI();
    }

    public void RebalanceTeam() {
        if (!GameSession.gameModeInfo.isTeamGame) return;
        foreach (var hud in playerDictionary.Values) {
            if (hud == null || !hud.gameObject.activeInHierarchy) continue;
            if (hud.controller.IsMine) {
                hud.ResetTeam();
            }
        }
    }

    void debugUI()
    {
        foundPlayers.Clear();
        foreach (var entry in playerDictionary.Keys)
        {
            foundPlayers.Add(entry);
        }

    }

    #region start game
    [SerializeField] Text readyButtonText;
    public void OnClick_Ready()
    {
        localPlayerInfo.pv.RPC("ToggleReady", RpcTarget.AllBuffered);
        bool ready = localPlayerInfo.GetReady();
        readyButtonText.text = (ready) ? LocalizationManager.Convert("_IS_WAITING") : LocalizationManager.Convert("_IS_READY");
        UpdateReadyStatus();
        if (readyPlayers == totalPlayers)
        {
            Debug.Log("Same number. start");
            pv.RPC("OnClick_ForceStart", RpcTarget.MasterClient);
        }
    }
    [PunRPC]
    public void OnClick_ForceStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (!CheckHalfAgreement()) return;
            if (!CheckTeamValidity()) return;
            //정식유저 룸프로퍼티 대기
            Debug.Log("Mastercleint push setting requested");
            mapOptions.SetGameStarted(true);
            mapOptions.PushRoomSettings();
        }
    }
    public bool CheckTeamValidity() {
        if (!GameSession.gameModeInfo.isTeamGame || GameSession.instance.devMode) return true;
        int numHome = PlayerManager.GetNumberInTeam(Team.HOME);
        int numAway = PlayerManager.GetNumberInTeam(Team.AWAY);
        if (numHome == 0 || numAway == 0)
        {

            ChatManager.SendNotificationMessage(LocalizationManager.Convert("_game_imba_team_numbers"));
            return false;
        }
        else {
            return true;
        }
    }
    public bool CheckHalfAgreement()
    {
        if (readyPlayers < (totalPlayers) / 2 && GameSession.instance.requireHalfAgreement)
        {
            ChatManager.SendNotificationMessage(LocalizationManager.Convert("_game_not_enough_ready", PhotonNetwork.MasterClient.NickName, (totalPlayers / 2).ToString()));
            return false;
        }
        return true;
    }

    public void OnRoomPropertiesChanged()
    {
        var Hash = PhotonNetwork.CurrentRoom.CustomProperties;
        bool gameStarted = (bool)Hash[HASH_GAME_STARTED];
        mapOptions.SetGameStarted(gameStarted);
        //  Debug.Log("Start requested "+ gameStarted);
        if (gameStarted)
        {
            GameFieldManager.SetGameMap(GameSession.gameModeInfo.gameMode);
            Debug.Log("RPC Start game");
            StartGame();
        }
    }
    IEnumerator WaitAndStartGame() {
        yield return new WaitForFixedUpdate();
        StartGame();
    }
    public void StartGame()
    {
        if (localPlayerObject != null)
        {
            PhotonNetwork.Destroy(localPlayerObject);
            localPlayerObject = null;
        }
        GetComponent<UI_BotLobby>().DestoryBotsPanel();
        
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.InGame });
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_STARTED, null);
    }

    #endregion

    #region helper

    int totalPlayers;
    int readyPlayers;
    private void UpdateReadyStatus(EventObject eo = null)
    {
        totalPlayers = PlayerManager.GetPlayerDictionary().Count;
        readyPlayers = 0;
        foreach (var entry in playerDictionary.Values)
        {
            if (entry.isReady)
            {
                readyPlayers++;
            }
        }
        numOfPlayers.text = LocalizationManager.Convert("_CURRENT_CONNECTED")+totalPlayers + " / " + MenuManager.MAX_PLAYER_PER_ROOM;
        numReadyText.text = LocalizationManager.Convert("_CURRENT_CONNECTED") + "" + readyPlayers + " / " + totalPlayers;
        if (localPlayerInfo != null)
        {
            readyButtonText.text = (localPlayerInfo.GetReady()) ? LocalizationManager.Convert("_IS_WAITING") : LocalizationManager.Convert("_IS_READY");
        }
    }


    #endregion
}
