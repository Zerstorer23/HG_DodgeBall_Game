using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameFieldManager;

public class GameSession : MonoBehaviourPun
{
    [SerializeField] Text versionText;
    [SerializeField] public Transform networkPos;
    public UI_TournamentPanel tournamentPanel;
    public Transform Home_Bullets;
    public UI_SkillBox skillPanelUI;
    public UI_Leaderboard leaderboardUI;
    public GameOverManager gameOverManager;
    public static ConfigsManager configsManager;
    public string versionCode;
    public static double STANDARD_PING = 0.08d;//0.075d;//자연스럽게 보이는 한 가장 크게
    public bool requireHalfAgreement = true;

    private static GameSession prGameSession;

    public static GameModeConfig gameModeInfo;
    public static int LocalPlayer_FieldNumber = -1;
    PhotonView pv;

    public float gameSpeed = 1f;
    public bool devMode = false;
    public CharacterType debugChara = CharacterType.NONE;

    public static bool auto_drive_enabled = false;
    public static bool auto_drive_toggled = false;
    public static bool jeopdae_enabled= false;
    private void FixedUpdate()
    {
        Time.timeScale = gameSpeed;
    }
    public static bool IsAutoDriving() {
        return (auto_drive_toggled ) &&(jeopdae_enabled|| auto_drive_enabled);
    }
    public static void toggleAutoDriveByKeyInput() {
        auto_drive_toggled = !auto_drive_toggled;
        Debug.Log("Driving " + auto_drive_toggled);
    }

    public static GameSession instance
    {
        get
        {
            if (!prGameSession)
            {
                prGameSession = FindObjectOfType<GameSession>();
                if (!prGameSession)
                {
                }
            }

            return prGameSession;
        }
    }
    public static GameSession GetInst() {
        return instance;
    }
    public static Transform GetBulletHome() => instance.Home_Bullets;
    private void Awake()
    {
        auto_drive_enabled = devMode;
        auto_drive_toggled = devMode;
        requireHalfAgreement = !devMode;

        pv = GetComponent<PhotonView>();
        configsManager = GetComponent<ConfigsManager>();
        versionText.text = versionCode + " 버전";
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);
    }

    public static bool gameStarted = false;
    private void OnGameFinished(EventObject obj)
    {
        gameStarted = false;
    }

    private void OnGameStarted(EventObject obj)
    {
        gameStarted = true;
    }

    private void Start()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.PreGame });
    }


    internal static string GetVersionCode()
    {
        return instance.versionCode;
    }
    public static float GetAngle(Vector2 vec1, Vector2 vec2)
    {
        Vector2 diference = vec2 - vec1;
        float sign = (vec2.y < vec1.y) ? -1.0f : 1.0f;
        return Vector2.Angle(Vector2.right, diference) * sign;
    }
    public static void PushRoomASetting(string key, object value) {
        var hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(key, value);
        PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
    }

    internal static void SetLocalPlayerFieldNumber(int myRoom)
    {
        LocalPlayer_FieldNumber = myRoom;
    }

    public static void ShowMainMenu() {
        instance.pv.RPC("ShowPanel", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void ShowPanel()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.PreGame });
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_CYCLE_RESTART, null);
    }
    [PunRPC]
    public void ResignMaster(Player newMaster) {
        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.SetMasterClient(newMaster);
        }
    }
    [PunRPC]
    public void LeaveRoom()
    {
        PlayerManager.embarkCalled = true;
        Debug.LogWarning("Leave room");
        //PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
        PhotonNetwork.LeaveRoom();
    }
    [PunRPC]
    public void QuitGame()
    {
        PhotonNetwork.LeaveRoom();
        Application.Quit();
    }
    [PunRPC]
    public void TriggerEvent(int eventCode)
    {
        MyEvents e = (MyEvents)   eventCode;
        Debug.Log("Trigger event " + e);
        EventManager.TriggerEvent(e, null);
    }
    public static IEnumerator CheckCoroutine(IEnumerator routine, IEnumerator newRoutine) {
        if (routine != null) {
            instance.StopCoroutine(routine);
        }
        return newRoutine;
    }

    public static CharacterType GetPlayerCharacter(UniversalPlayer player)
    {
        if (!player.HasProperty("CHARACTER")) return CharacterType.NONE;
        CharacterType character = player.GetProperty<CharacterType>("CHARACTER");
        if (character == CharacterType.NONE)
        {
            if (!player.HasProperty("ACTUAL_CHARACTER")) return CharacterType.NONE;
            character = player.GetProperty<CharacterType>("ACTUAL_CHARACTER");
        }
        return character;
    }
    void OnApplicationPause(bool paused)
    {
        if (Application.platform == RuntimePlatform.Android) {
            if (!GooglePlayManager.loggedIn) return;
            if (paused)
            {

                Application.Quit();
            }
        }
    }
    private void OnApplicationQuit()
    {
        try
        {

            PlayerPrefs.Save();
        }
        catch (Exception e) {
            Debug.LogWarning(e.Message);
        }
    }

}
