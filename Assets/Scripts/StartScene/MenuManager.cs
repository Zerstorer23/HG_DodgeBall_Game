using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System;
using ExitGames.Client.Photon;
using static ConstantStrings;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject playerNamePrefab;
    [SerializeField] GameObject localPlayerObject;
    [SerializeField] GameObject loadingChuu;
    [SerializeField] InputField userNameInput;
    [SerializeField] Text numOfPlayers;
    [SerializeField] Text numReadyText;
    [SerializeField] GameObject[] disableInLoading;
    const string PRIMARY_ROOM = "PrimaryRoom";
    //  ExitGames.Client.Photon.Hashtable roomSetting = new ExitGames.Client.Photon.Hashtable();
    PhotonView pv;

    //***Players***//
    string default_name = "ㅇㅇ";
    public const int MAX_PLAYER_PER_ROOM = 18;
    HUD_UserName localPlayerInfo;
    Dictionary<string, HUD_UserName> playerDictionary = new Dictionary<string, HUD_UserName>();
    //***************//
    public UnitConfig[] unitConfigs;
   public static Dictionary<CharacterType, UnitConfig> unitDictionary = new Dictionary<CharacterType, UnitConfig>();


    //*****GAME SETTING***//
    public int playerLives = 1;
    public MapDifficulty mapDifficulty = MapDifficulty.Standard;
    int[] mapDifficulties = { 0, 12, 24, 36 };

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        loadingChuu.SetActive(true);
        foreach (GameObject go in disableInLoading) {
            go.SetActive(false);
        }
        forceStart.SetActive(false);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;

        foreach (UnitConfig u in unitConfigs) {
            unitDictionary.Add(u.characterID, u);
        }

        EventManager.StartListening(MyEvents.EVENT_PLAYER_JOINED, OnNewPlayerEnter);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_LEFT, OnJoinedPlayerLeave);

    }

    private void OnJoinedPlayerLeave(EventObject arg0)
    {
        string id = arg0.stringObj;
        if (playerDictionary.ContainsKey(id))
        {
            playerDictionary.Remove(id);
        }
    }
    [SerializeField] Transform playerListTransform;
    private void OnNewPlayerEnter(EventObject arg0)
    {
        Debug.Log("Received player join");
        string id = arg0.stringObj;
        HUD_UserName info = arg0.gameObject.GetComponent<HUD_UserName>();
        if (playerDictionary.ContainsKey(id)) {
            playerDictionary[id] = info;
        }
        else {
            playerDictionary.Add(id, info);
        }
        arg0.gameObject.GetComponent<RectTransform>().SetParent(playerListTransform);
    }

    private void OnDestroy()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;

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
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(HASH_MAP_DIFF, mapDifficulties[(int)mapDifficulty]);
        hash.Add(HASH_PLAYER_LIVES, playerLives);
        RoomOptions roomOpts = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 10,
            PublishUserId = true,
            CustomRoomProperties = hash
        };
        PhotonNetwork.JoinOrCreateRoom(PRIMARY_ROOM, roomOpts, TypedLobby.Default);

    }
    public override void OnJoinedRoom()
    {
        //Play Game Scene
        //    = new ExitGames.Client.Photon.Hashtable();

        //     hash.Add("Team ", 0);
        //  PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        AddJoinedPlayer();

        loadingChuu.SetActive(false);
        foreach (GameObject go in disableInLoading)
        {
            go.SetActive(true);
        }
        UpdateSettingsUI();
        userNameInput.placeholder.GetComponent<Text>().text = default_name;
        UpdatePlayersStatus();

    }
    public override void OnPlayerEnteredRoom(Player newPlayer) {

    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        UpdateSettingsUI();
    }
    [SerializeField] GameObject mapDiffPanel;
    [SerializeField] GameObject playerDiffPanel;
    [SerializeField] GameObject forceStart;
    [SerializeField] Text mapDiffText;
    [SerializeField] Text playerDiffText;

    public void UpdateSettingsUI() {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            mapDiffPanel.SetActive(true);
            playerDiffPanel.SetActive(true);
            forceStart.SetActive(true);
        }
        else {
            mapDiffPanel.SetActive(false);
            playerDiffPanel.SetActive(false);
            forceStart.SetActive(false);
        }
        mapDiffText.text = "장애물 난이도: ";
        switch (mapDifficulty)
        {
            case MapDifficulty.None:
                mapDiffText.text += "없음";
                break;
            case MapDifficulty.Standard:
                mapDiffText.text += "표준";
                break;
            case MapDifficulty.Hard:
                mapDiffText.text += "어려움";
                break;
            case MapDifficulty.VeryHard:
                mapDiffText.text += "개어려움";
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
    private void UpdatePlayersStatus() {
        totalPlayers = playerDictionary.Count;
        readyPlayers = 0;
        foreach (KeyValuePair<string, HUD_UserName> entry in playerDictionary)
        {
            if (entry.Value.isReady) {
                readyPlayers++;
            }
        }
        numOfPlayers.text = "현재접속: "+totalPlayers+" / " + MAX_PLAYER_PER_ROOM;
        numReadyText.text = "준비: "+ readyPlayers + " / " + totalPlayers;

    }
    private void CheckGameStart()
    {
        if (readyPlayers == totalPlayers) {
            pv.RPC("OnClick_ForceStart",RpcTarget.MasterClient);
        }
    }

    [PunRPC]
    public void OnClick_ForceStart()
    {
        if (PhotonNetwork.IsMasterClient) {
            SetRoomSettings();
            PhotonNetwork.LoadLevel(1);
        }
    }
    void SetRoomSettings() {
        ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
        hash[HASH_MAP_DIFF]= mapDifficulties[(int)mapDifficulty];
        hash[HASH_PLAYER_LIVES]  = playerLives;
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);

    }

    public void OnNameField_Changed()
    {
        localPlayerInfo.pv.RPC("ChangeName", RpcTarget.AllBuffered, userNameInput.text);
    }
    public void OnClick_MapDifficulty(int amount) {
        if (PhotonNetwork.LocalPlayer.IsMasterClient) {
            object[] datas = new object[] {amount};
            SendSerializedEvent(SetingsCode.MapDifficulty, datas);
        }
    }

    [SerializeField] Text myCharName;
    [SerializeField] Image myCharImage;
    public void OnClick_SetCharacter(int charID) {
        UnitConfig u = unitDictionary[(CharacterType)charID];
        myCharName.text = u.txt_name;
        myCharImage.sprite = u.portraitImage;
        localPlayerInfo.pv.RPC("ChangeCharacter",RpcTarget.AllBuffered,charID);

    }

    public void OnClick_PlayerDifficulty(int amount)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            object[] datas = new object[] { amount };
            SendSerializedEvent(SetingsCode.PlayerDifficulty, datas);
        }
    }
    public void SendSerializedEvent(SetingsCode ecode, object[] parameters) {

        RaiseEventOptions options = new RaiseEventOptions
        {
            CachingOption = EventCaching.DoNotCache,
            Receivers = ReceiverGroup.All
        };

        SendOptions sendOptions = new SendOptions();
        sendOptions.Reliability = true;

        PhotonNetwork.RaiseEvent((byte)ecode, parameters, options, sendOptions);
    }
    private void OnEvent(EventData photonEvent)
    {

        byte eventCode = photonEvent.Code;
        object content = photonEvent.CustomData;
        SetingsCode code = (SetingsCode)eventCode;
        object[] datas = content as object[];
        switch (code)
        {
            case SetingsCode.MapDifficulty:
                mapDifficulty = (MapDifficulty)datas[0];
                break;
            case SetingsCode.PlayerDifficulty:
                playerLives = (int)datas[0];
                break;
        }
        UpdateSettingsUI();
    }



    #endregion

    public List<HUD_UserName> connectedPlayerTexts = new List<HUD_UserName>();
    [SerializeField] GameObject playerListContent;

  
    private void AddJoinedPlayer()
    {
        if (localPlayerObject == null) {
            localPlayerObject = PhotonNetwork.Instantiate(ConstantStrings.PREFAB_STARTSCENE_PLAYERNAME, Vector3.zero, Quaternion.identity, 0);
            localPlayerInfo = localPlayerObject.GetComponent<HUD_UserName>();
            localPlayerInfo.pv.RPC("ChangeName", RpcTarget.AllBuffered, default_name);
        }
    }
}
public enum SetingsCode { 
    MapDifficulty =0,
    PlayerDifficulty
}

public enum MapDifficulty { 
    None = 0,Standard,Hard,VeryHard
}
