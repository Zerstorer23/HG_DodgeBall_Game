using Photon.Pun;
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

    public static GameModeConfig gameModeInfo ;
    public static int LocalPlayer_FieldNumber = -1;
    PhotonView pv;

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
        pv = GetComponent<PhotonView>();
        configsManager = GetComponent<ConfigsManager>();
        versionText.text = versionCode+" 버전";
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
    }
    void OnApplicationPause(bool paused)
    {
        if (Application.platform == RuntimePlatform.Android) {

            if (paused)
            {
                Application.Quit();
            }
        }
    }

}
