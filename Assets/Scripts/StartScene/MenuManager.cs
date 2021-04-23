using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using ExitGames.Client.Photon;
using TMPro;
using static ConstantStrings;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject playerNamePrefab;
    [SerializeField] GameObject localPlayerObject;
    [SerializeField] GameObject loadingChuu;
    [SerializeField] Text numOfPlayers;
    [SerializeField] Text numReadyText;
    [SerializeField] GameObject[] disableInLoading;
    const string PRIMARY_ROOM = "PrimaryRoom";
    //  ExitGames.Client.Photon.Hashtable roomSetting = new ExitGames.Client.Photon.Hashtable();
    PhotonView pv;

    //***Players***//
    public const int MAX_PLAYER_PER_ROOM = 18;
    public static HUD_UserName localPlayerInfo;
    Dictionary<string, HUD_UserName> playerDictionary = new Dictionary<string, HUD_UserName>();


    //*****GAME SETTING***//
    public static readonly int default_lives = 5;
    public static readonly MapDifficulty default_difficult = MapDifficulty.None;
    public int playerLives;
    public MapDifficulty mapDifficulty;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        Debug.Log("Nickname photon " + PhotonNetwork.NickName);
        forceStart.SetActive(false);
  
        EventManager.StartListening(MyEvents.EVENT_PLAYER_JOINED, OnNewPlayerEnter);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_LEFT, OnJoinedPlayerLeave);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SELECTED_CHARACTER, OnCharacterSelected);

    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_JOINED, OnNewPlayerEnter);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_LEFT, OnJoinedPlayerLeave);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_SELECTED_CHARACTER, OnCharacterSelected);

    }
    private void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            return;
        }
        else
        {
            Debug.Log("Do loading");
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
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

   


    private void OnJoinedPlayerLeave(EventObject arg0)
    {
        string id = arg0.stringObj;
        if (playerDictionary.ContainsKey(id))
        {
            playerDictionary.Remove(id);
        }
        if (PhotonNetwork.IsMasterClient) {
            UpdateSettingsUI();
        }
        UpdatePlayersStatus();
    }
    [SerializeField] Transform playerListTransform;
    private void OnNewPlayerEnter(EventObject arg0)
    {

        string id = arg0.stringObj;
        HUD_UserName info = arg0.gameObject.GetComponent<HUD_UserName>();
        if (playerDictionary.ContainsKey(id))
        {
            playerDictionary[id] = info;
        }
        else
        {
            playerDictionary.Add(id, info);
        }
        arg0.gameObject.GetComponent<Transform>().SetParent(playerListTransform, false);
        UpdatePlayersStatus();
    }



    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }
    public override void OnJoinedLobby()
    {
        JoinRoom();
    }
    public void JoinRoom()
    {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        mapDifficulty=(MapDifficulty) ConnectedPlayerManager.GetRoomSettings(HASH_MAP_DIFF, default_difficult);
        playerLives=(int) ConnectedPlayerManager.GetRoomSettings(HASH_PLAYER_LIVES, default_lives);
        hash.Add(HASH_MAP_DIFF, mapDifficulty);
        hash.Add(HASH_PLAYER_LIVES, playerLives);
        RoomOptions roomOpts = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = MAX_PLAYER_PER_ROOM,
            PublishUserId = true,
            CustomRoomProperties = hash
        };
        PhotonNetwork.JoinOrCreateRoom(PRIMARY_ROOM, roomOpts, TypedLobby.Default);

    }
    public override void OnJoinedRoom()
    {
        AddJoinedPlayer();

        loadingChuu.SetActive(false);
        foreach (GameObject go in disableInLoading)
        {
            go.SetActive(true);
        }
        UpdateSettingsUI();
        UpdatePlayersStatus();

    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
    }
    [SerializeField] GameObject mapDiffPanel;
    [SerializeField] GameObject playerDiffPanel;
    [SerializeField] GameObject forceStart;
    [SerializeField] Text mapDiffText;
    [SerializeField] Text playerDiffText;

   [PunRPC]
    public void UpdateSettingsUI()
    {
        ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
        mapDifficulty = (MapDifficulty)hash[HASH_MAP_DIFF];
        playerLives = (int)hash[HASH_PLAYER_LIVES];
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            mapDiffPanel.SetActive(true);
            playerDiffPanel.SetActive(true);
            forceStart.SetActive(true);
        }
        else
        {
            mapDiffPanel.SetActive(false);
            playerDiffPanel.SetActive(false);
            forceStart.SetActive(false);
        }
        mapDiffText.text = "장애물 난이도: ";
        switch (mapDifficulty)
        {
            case MapDifficulty.None:
                mapDiffText.text += "쉬움";
                break;
            case MapDifficulty.BoxOnly:
                mapDiffText.text += "장애물만";
                break;
            case MapDifficulty.Standard:
                mapDiffText.text += "표준";
                break;
            case MapDifficulty.Hard:
                mapDiffText.text += "어려움";
                break;
        }
        playerDiffText.text = "플레이어 라이프: " + playerLives;
    }

    [SerializeField] Text readyButtonText;
    #region UIMethods
    public void OnClick_Ready()
    {
        localPlayerInfo.pv.RPC("ToggleReady", RpcTarget.AllBuffered);
        bool ready = localPlayerInfo.GetReady();
        readyButtonText.text = (ready) ? "다른사람을 기다리는 중" : "준비되었음!";
        UpdatePlayersStatus();
        CheckGameStart();
    }

    int totalPlayers;
    int readyPlayers;
    private void UpdatePlayersStatus()
    {
        totalPlayers = playerDictionary.Count;
        readyPlayers = 0;
        foreach (KeyValuePair<string, HUD_UserName> entry in playerDictionary)
        {
            if (entry.Value.isReady)
            {
                readyPlayers++;
            }
        }
        numOfPlayers.text = "현재접속: " + totalPlayers + " / " + MAX_PLAYER_PER_ROOM;
        numReadyText.text = "준비: " + readyPlayers + " / " + totalPlayers;

    }
    private void CheckGameStart()
    {
        if (readyPlayers == totalPlayers)
        {
            pv.RPC("OnClick_ForceStart", RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    public void OnClick_ForceStart()
    {
        if (PhotonNetwork.IsMasterClient) { 
            PhotonNetwork.LoadLevel(1);
        }
    }
    [PunRPC]
    void SetRoomSettings()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
            hash[HASH_MAP_DIFF] = mapDifficulty;
            hash[HASH_PLAYER_LIVES] = playerLives;
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }
    }


 

    [SerializeField] Text myCharName;
    [SerializeField] Image myCharImage;
    public void OnCharacterSelected(EventObject eo)
    {
        int charID = eo.intObj;
        UnitConfig u =(UnitConfig)eo.objData;
        myCharName.text = u.txt_name;
        myCharImage.sprite = u.portraitImage;
        localPlayerInfo.pv.RPC("ChangeCharacter", RpcTarget.AllBuffered, charID);

    }
    public void OnClick_MapDifficulty(int amount)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            mapDifficulty = (MapDifficulty)amount;
            pv.RPC("SetRoomSettings", RpcTarget.AllBuffered);
            pv.RPC("UpdateSettingsUI", RpcTarget.AllBuffered);
        }
    }
    public void OnClick_PlayerDifficulty(int amount)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            playerLives = amount;
            pv.RPC("SetRoomSettings", RpcTarget.AllBuffered);
            pv.RPC("UpdateSettingsUI", RpcTarget.AllBuffered);
        }
    }
    public static string GetLocalName()
    {
        return PhotonNetwork.LocalPlayer.NickName;
    }

    #endregion

    private void AddJoinedPlayer()
    {
        localPlayerObject = PhotonNetwork.Instantiate(ConstantStrings.PREFAB_STARTSCENE_PLAYERNAME, Vector3.zero, Quaternion.identity, 0);
        localPlayerInfo = localPlayerObject.GetComponent<HUD_UserName>();
        string name = (string)ConnectedPlayerManager.GetPlayerSettings("NICKNAME", UI_ChangeName.default_name);
        CharacterType character = (CharacterType)ConnectedPlayerManager.GetPlayerSettings("CHARACTER", CharacterType.HARUHI);
        localPlayerInfo.pv.RPC("ChangeName", RpcTarget.AllBuffered, name);
        localPlayerInfo.pv.RPC("ChangeCharacter", RpcTarget.AllBuffered, (int)character);

        UnitConfig u = EventManager.unitDictionary[character];
        myCharName.text = u.txt_name;
        myCharImage.sprite = u.portraitImage;
        EventManager.TriggerEvent(MyEvents.EVENT_SCENE_CHANGED, new EventObject() { intObj = 0 });
/*        ScreenCapture.CaptureScreenshot(Application.dataPath + "/screenshots/" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".png");
        UnityEditor.AssetDatabase.Refresh();*/
    }
}


public enum MapDifficulty
{
    None = 0, BoxOnly, Standard, Hard
}
