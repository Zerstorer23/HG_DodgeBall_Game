using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GameFieldManager;

public class GameSession : MonoBehaviour
{
    [SerializeField] Text versionText;
    [SerializeField] public Transform networkPos;

    public PlayerSpawner charSpawner;
    public UI_SkillBox skillPanelUI;
    public UI_Leaderboard leaderboardUI;
    public GameOverManager gameOverManager;
    public string versionCode = "0428.1";
    public static double STANDARD_PING = 0.08d;//0.075d;//자연스럽게 보이는 한 가장 크게

    private static GameSession prGameSession;

    public UnitConfig[] unitConfigs;
    public static Dictionary<CharacterType, UnitConfig> unitDictionary;
    public static GameMode gameMode ;
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
    private void Awake()
    {
        versionText.text = versionCode+" 버전";
        unitDictionary = new Dictionary<CharacterType, UnitConfig>();
        foreach (UnitConfig u in unitConfigs)
        {
            unitDictionary.Add(u.characterID, u);
        }
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);
    }

    public static bool gameStarted = false;
  public static  bool suddenDeathCalled = false;
    private void OnGameFinished(EventObject obj)
    {
        gameStarted = false;
        suddenDeathCalled = false;
    }

    private void OnGameStarted(EventObject obj)
    {
        gameStarted = true;
    }

    private void Start()
    {
        Debug.Log("SHow pregame");
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.PreGame });
    }
    public static Vector3 GetRandomPosOnMap(float boundOffset = 0) {

        float randX = Random.Range(xMin+boundOffset, xMax- boundOffset);
        float randY = Random.Range(yMin+ boundOffset, yMax- boundOffset);
        return new Vector3(randX, randY,0);
    }
    public static Vector3 GetRandomPosOnMapAround(Vector3 center, float window, float boundOffset = 0)
    {

        float randX = Random.Range(-window, window);
        float randY = Random.Range(-window, window);
        if (randX <= (xMin + boundOffset)) randX = xMin + boundOffset;
        if (randX >= (xMax - boundOffset)) randX = xMax - boundOffset;
        if (randY <= (yMin + boundOffset)) randY = yMin + boundOffset;
        if (randY <= (yMin + boundOffset)) randY = yMin + boundOffset;

        return new Vector3(randX, randY, 0);
    }
    public static Unit_Player GetPlayerByID(string id)
    {
        return instance.charSpawner.GetPlayerByOwnerID(id);
    }

    public static CharacterType GetRandomCharacter()
    {
        int rand = Random.Range(1, instance.unitConfigs.Length);
        Debug.Log("Rando m chara " + rand);
        return instance.unitConfigs[rand].characterID;
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
}
