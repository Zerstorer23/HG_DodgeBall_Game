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
        Debug.Log("Joined room");
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        mapOptions.gameStarted = (bool)hash[HASH_GAME_STARTED];
        string roomversion = (string)hash[HASH_VERSION_CODE];
        if (GameSession.GetVersionCode() != roomversion) {
            PhotonNetwork.NickName = string.Format(
                "<color=#ff0000>클라이언트 버전</color>이 맞지않습니다. 방장 버전 {0}",
                roomversion);
        }

        Debug.Log("Game started? " + mapOptions.gameStarted);
        if (!mapOptions.gameStarted)
        {
            Debug.Log("Instantiate after connection");


            InstantiateMyself();
            UpdateReadyStatus();
            mapOptions.UpdateSettingsUI();//I join room
        }
        else {
            //난입유저 바로시작
            ExitGames.Client.Photon.Hashtable playerHash = new ExitGames.Client.Photon.Hashtable();
            playerHash.Add("TEAM", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerHash);
            Debug.Log("난입세팅끝");
            RPC_StartGame();
        }
    }

    private void OnEnable()
    {
        playerDictionary = new Dictionary<string, HUD_UserName>();
        if (!PhotonNetwork.IsConnectedAndReady) return;
        Debug.Log("Instantiate after regame");
        if (PhotonNetwork.IsMasterClient) {
            Player randomPlayer = ConnectedPlayerManager.GetRandomPlayerExceptMe();
            if(randomPlayer != null)
            PhotonNetwork.SetMasterClient(randomPlayer);
        }

        InstantiateMyself();
        UpdateReadyStatus();//I enter room
        mapOptions.UpdateSettingsUI();
    }
    private void InstantiateMyself()
    {
        Debug.Assert(localPlayerObject == null, "PLayer obj not removed");
        localPlayerObject = PhotonNetwork.Instantiate(ConstantStrings.PREFAB_STARTSCENE_PLAYERNAME, Vector3.zero, Quaternion.identity, 0);
        localPlayerInfo = localPlayerObject.GetComponent<HUD_UserName>();
        string name = PhotonNetwork.NickName;
        CharacterType character = (CharacterType)GetPlayerProperty("CHARACTER", CharacterType.HARUHI);
        bool isHomeTeam = (bool)GetPlayerProperty("TEAM", true);
        localPlayerInfo.pv.RPC("ChangeName", RpcTarget.AllBuffered, name);
        localPlayerInfo.pv.RPC("ChangeCharacter", RpcTarget.AllBuffered, (int)character);
        localPlayerInfo.pv.RPC("SetTeam", RpcTarget.AllBuffered, (bool)isHomeTeam);
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
        foreach (var entry in playerDictionary.Keys) {
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
            pv.RPC("OnClick_ForceStart", RpcTarget.AllBuffered);
        }
    }

    //Awake Start Update <Coroutine>
    //1 초
    //UPdate 1ch 60번 < 지금시간이 1초뒤인지 매번확인
    //Corooutine ,_ 1초뒤
    // 1초뒤에 함수
    [PunRPC]
    public void OnClick_ForceStart()
    {
        Debug.Log("Start requested");
        var Hash = PhotonNetwork.CurrentRoom.CustomProperties;
        bool gameStarted = (bool)Hash[HASH_GAME_STARTED];
        Debug.Log("Game start? " + gameStarted);
        if (PhotonNetwork.IsMasterClient)
        {
            //정식유저 룸프로퍼티 대기
            Debug.Log("Mastercleint push setting requested");
            mapOptions.gameStarted = true;
            mapOptions.PushRoomSettings();
        }
    }
    public void OnRoomPropertiesChanged()
    {
        var Hash = PhotonNetwork.CurrentRoom.CustomProperties;
        bool gameStarted = (bool)Hash[HASH_GAME_STARTED];
      //  Debug.Log("Start requested "+ gameStarted);
        if (gameStarted)
        {
            Debug.Log("RPC Start game");
            RPC_StartGame();
        }
    }
    public void RPC_StartGame()
    {
        Debug.Log(playerDictionary.Count + " vs " + PhotonNetwork.CurrentRoom.PlayerCount);
        GameFieldManager.GetInst().InitialiseMapSize(PhotonNetwork.CurrentRoom.PlayerCount);
        if (localPlayerObject != null) {
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
        totalPlayers = ConnectedPlayerManager.GetPlayerDictionary().Count;
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
    public static object GetPlayerProperty(string tag, object value)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(tag))
        {
            return PhotonNetwork.LocalPlayer.CustomProperties[tag];
        }
        else
        {
            return value;
        }
    }
    #endregion
}
