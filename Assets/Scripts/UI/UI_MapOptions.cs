using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;
using Random = UnityEngine.Random;

public class UI_MapOptions : MonoBehaviourPun
{
    //*****GAME SETTING***//
    public static int default_lives_index = 1;
    public static MapDifficulty default_difficult = MapDifficulty.Easy;
    // public int playerLives;
    // public MapDifficulty mapDifficulty;
    bool gameStarted = false;
    PhotonView pv;


    [SerializeField] Text mapDiffText;
    [SerializeField] Text playerDiffText;
    [SerializeField] Dropdown mapDiffDropdown;
    [SerializeField] Dropdown livesDropdown;
    [SerializeField] Dropdown gamemodeDropdown;
    [SerializeField] Dropdown mapOptionsDropdown;
    [SerializeField] GameObject subOptionsObject;
    [SerializeField] GameObject[] masterOnlyObjects;
    public MapDifficulty mapDiff;
    public int livesIndex = 0;
    public int mapSubOptionChoice = 0;

    public static int[] lives = new int[] { 1, 3, 5 };



    public void SetGameStarted(bool enable)
    {
        gameStarted = enable;
    }
    public bool GetGameStarted() => gameStarted;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        foreach (var go in masterOnlyObjects)
        {
            go.SetActive(false);
        }
    }
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_JEOPDAE_ENABLE, CheckAutoMasterClient);
        ShowSubMapOptions();
        UpdateSettingsUI();
        CheckAutoMasterClient();
    }
    private void OnDisable()
    {

        EventManager.StopListening(MyEvents.EVENT_JEOPDAE_ENABLE, CheckAutoMasterClient);
    }
    private void CheckAutoMasterClient(EventObject eo = null)
    {
        if (!GameSession.jeopdae_enabled || !PhotonNetwork.IsMasterClient || !pv.IsMine)
        {
            return;
        }
        RandomSettings();


    }
    void RandomSettings()
    {
        StartCoroutine(        SetRandomSubOptions());
        StartCoroutine(        SetRandomLives()     );
        StartCoroutine(        SetRandomObstacles() );
    }

    IEnumerator SetRandomSubOptions()
    {
        float randTime = Random.Range(0f, 1f);
        yield return new WaitForSeconds(randTime);
        randTime = Random.Range(0f, 1f);
        if (randTime <= 0.5f) {
            gamemodeDropdown.value = 0;
        }
        randTime = Random.Range(0f, 1f);
        yield return new WaitForSeconds(randTime);
        if (GameSession.gameModeInfo.gameMode == GameMode.PVP)
        {
            pv.RPC("SetMapSubOptions", RpcTarget.AllBuffered, 1);
        }
    }


    IEnumerator SetRandomObstacles()
    {
        float randTime = Random.Range(0f, 2f);
        yield return new WaitForSeconds(randTime);
        float rand = Random.Range(0f, 1f);
        if (rand <= 0.75)
        {
            int choice = Random.Range(0, 3);
            mapDiffDropdown.value = choice;

        }
        else
        {
            int choice = Random.Range(3, 5);
            mapDiffDropdown.value = choice;
        }
    }

    IEnumerator SetRandomLives()
    {
        float randTime = Random.Range(0f, 2f);
        yield return new WaitForSeconds(randTime);
        float randLives = Random.Range(0f, 1f);
        if (randLives <= 0.2f)
        {
            livesDropdown.value = 0;
        }
        else if (randLives <= 0.8f)
        {

            livesDropdown.value = 1;
        }
        else
        {
            livesDropdown.value = 2;
        }
    }

    public void ShowSubMapOptions()
    {
        mapOptionsDropdown.ClearOptions();
        switch (GameSession.gameModeInfo.gameMode)
        {
            case GameMode.PVP:
                OpenOptions("일반", "완전무작위");
                break;
            case GameMode.TEAM:
            case GameMode.Tournament:
            case GameMode.PVE:
                OpenOptions(null);
                break;
            case GameMode.TeamCP:
                OpenOptions("점령지 1개", "점령지 5개"
                    , "순차점령"
                    );//, );
                break;
        }
    }
    public void OpenOptions(params string[] names)
    {
        if (names == null || names.Length == 0)
        {
            subOptionsObject.SetActive(false);
            return;
        }
        int num = names.Length;
        subOptionsObject.SetActive(num > 0);
        for (int i = 0; i < num; i++)
        {
            var option = new Dropdown.OptionData();
            option.text = names[i];
            mapOptionsDropdown.options.Add(option);
        }
        mapSubOptionChoice = 0;
        mapOptionsDropdown.captionText.text = mapOptionsDropdown.options[0].text;
    }

    public void OnDropdown_MapSubOptions()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int index = mapOptionsDropdown.value;
            Debug.Log("SubOption " + index);
            pv.RPC("SetMapSubOptions", RpcTarget.AllBuffered, index);
        }
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
        foreach (var go in masterOnlyObjects)
        {
            go.SetActive(isMaster);
        }


        mapDiffDropdown.interactable = isMaster;
        livesDropdown.interactable = isMaster;
        gamemodeDropdown.interactable = isMaster;
        mapOptionsDropdown.interactable = isMaster;
        mapDiffDropdown.SetValueWithoutNotify((int)mapDiff);
        livesDropdown.SetValueWithoutNotify((int)livesIndex);
        int gmode = 0;
        if (GameSession.gameModeInfo != null)
        {
            gmode = (int)GameSession.gameModeInfo.gameMode;
        }
        gamemodeDropdown.SetValueWithoutNotify(gmode);
        StartCoroutine(UpdateDropdown());
    }
    IEnumerator UpdateDropdown()
    {
        yield return new WaitForFixedUpdate();
        mapOptionsDropdown.SetValueWithoutNotify(mapSubOptionChoice);
    }

    internal void LoadRoomSettings()
    {
        var hash = PhotonNetwork.CurrentRoom.CustomProperties;
        mapDiff = (MapDifficulty)hash[HASH_MAP_DIFF];
        livesIndex = (int)hash[HASH_PLAYER_LIVES];
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
    public void OnClick_AnonGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        HUD_UserName[] users = FindObjectsOfType<HUD_UserName>();
        foreach (var user in users)
        {
            user.pv.RPC("ChangeName", RpcTarget.AllBuffered, "ㅇㅇ");
        }
    }
    [PunRPC]
    public void SetMapDifficulty(int diff)
    {
        mapDiff = (MapDifficulty)diff;
        UpdateSettingsUI();
    }
    [PunRPC]
    public void SetMapSubOptions(int diff)
    {
        mapSubOptionChoice = diff;
        UpdateSettingsUI();
    }
    [PunRPC]
    public void SetPlayerLives(int index)
    {
        livesIndex = index;
        UpdateSettingsUI();
    }
    [PunRPC]
    public void SetGameMode(int index)
    {
        GameSession.gameModeInfo = ConfigsManager.gameModeDictionary[(GameMode)index];
        ShowSubMapOptions();
        EventManager.TriggerEvent(MyEvents.EVENT_GAMEMODE_CHANGED, new EventObject() { objData = GameSession.gameModeInfo });
        UpdateSettingsUI();
    }
    public static ExitGames.Client.Photon.Hashtable GetInitOptions()
    {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(HASH_MAP_DIFF, (GameSession.GetInst().devMode) ? MapDifficulty.None : default_difficult);
        hash.Add(HASH_PLAYER_LIVES, (GameSession.GetInst().devMode) ? 2 : default_lives_index);
        hash.Add(HASH_VERSION_CODE, GameSession.GetVersionCode());
        hash.Add(HASH_SUB_MAP_OPTIONS, 0);
        hash.Add(HASH_GAME_MODE, GameMode.PVP);
        hash.Add(HASH_GAME_STARTED, false);
        hash.Add(HASH_GAME_AUTO, false);
        hash.Add(HASH_ROOM_RANDOM_SEED, UnityEngine.Random.Range(0, 133));
        return hash;
    }
    public void PushRoomSettings()
    {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(HASH_MAP_DIFF, mapDiff);
        hash.Add(HASH_PLAYER_LIVES, livesIndex);
        hash.Add(HASH_VERSION_CODE, GameSession.GetVersionCode());
        hash.Add(HASH_SUB_MAP_OPTIONS, mapSubOptionChoice);
        hash.Add(HASH_GAME_STARTED, gameStarted);
        hash.Add(HASH_GAME_MODE, GameSession.gameModeInfo.gameMode);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }


}
