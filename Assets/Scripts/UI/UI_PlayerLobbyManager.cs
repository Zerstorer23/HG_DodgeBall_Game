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
            PlayerManager.CountPlayersInTeam();
            GameFieldManager.SetGameMap(GameSession.gameModeInfo.gameMode);
            GameFieldManager.ChangeToSpectator();
            Debug.Log("난입세팅끝");
            StartCoroutine(WaitAndStartGame());
        }
    }

    private void OnEnable()
    {
        playerDictionary = new Dictionary<string, HUD_UserName>();
        if (PhotonNetwork.CurrentRoom == null) return;
        mapOptions.SetGameStarted(false);
        Debug.Log("Instantiate after regame");
        if (PhotonNetwork.IsMasterClient)
        {
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
        string id = newPlayer.UserId;
        Debug.Assert(playerDictionary.ContainsKey(id), "Removing p doesnt exist");
        playerDictionary.Remove(id);
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
        UpdateReadyStatus();
        debugUI();
    }
    void debugUI()
    {
        foundPlayers = new List<string>();
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
        readyButtonText.text = (ready) ? "다른사람을 기다리는 중" : "준비되었음!";
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
        Team masterTeam = (Team)PhotonNetwork.LocalPlayer.CustomProperties["TEAM"];
        Player[] players = PhotonNetwork.PlayerList;
        foreach (Player p in players)
        {
            Team away = (Team)p.CustomProperties["TEAM"];
            if (masterTeam != away) {
                return true;
            }
        }
        ChatManager.SendNotificationMessage("최소 한명은 팀이 달라야합니다 장애인들아");
        return false;
    }
    public bool CheckHalfAgreement()
    {
        if (readyPlayers < (totalPlayers) / 2 && GameSession.instance.requireHalfAgreement)
        {
            ChatManager.SendNotificationMessage(string.Format("{0}님이 강제시작을 하려다 실패하였습니다. 요구인원 :{1}", PhotonNetwork.MasterClient.NickName, (totalPlayers / 2)));
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
            PlayerManager.CountPlayersInTeam();
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
        numOfPlayers.text = "현재접속: " + totalPlayers + " / " + MenuManager.MAX_PLAYER_PER_ROOM;
        numReadyText.text = "준비: " + readyPlayers + " / " + totalPlayers;
        if (localPlayerInfo != null)
        {
            readyButtonText.text = (localPlayerInfo.GetReady()) ? "다른사람을 기다리는 중" : "준비되었음!";
        }
    }


    #endregion
}
