using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpec {
    public float xMin, xMax, yMin, yMax, xMid, yMid;
}
public class GameFieldManager : MonoBehaviourPun
{
    Transform mapTransform;
    public float mapStepsize = 10f;
    public int mapStepPerPlayer = 5;
    //public static float xMin, xMax, yMin, yMax, xMid, yMid;
    private static GameFieldManager prGameFieldManager;
    public static Dictionary<string, Unit_Player> totalUnitsDictionary = new Dictionary<string, Unit_Player>();
    public static Dictionary<string, Player> totalPlayersDictionary = new Dictionary<string, Player>();

    [SerializeField] GameField pvpField, teamField;
    [SerializeField] GameField[] tournamentFields;

    internal static bool CheckSuddenDeathCalled(int fieldNo)
    {
        return gameFields[fieldNo].suddenDeathCalled;
    }

    public static List<GameField> gameFields = new List<GameField>();

    internal static int GetRemainingPlayerNumber()
    {

            GameStatus stat = GetTotalGameStatus();
            return stat.toKill;
        
    }

    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStartRequested);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }

    private void OnPlayerSpawned(EventObject eo)
    {
        string id = eo.stringObj;
        Unit_Player go = eo.goData.GetComponent<Unit_Player>();
        if (totalUnitsDictionary.ContainsKey(id))
        {
            Debug.LogWarning("Duplicate add player>");
            totalUnitsDictionary[id] = go;
            totalPlayersDictionary[id] = go.pv.Owner;
        }
        else
        {
            totalUnitsDictionary.Add(id, go);
            totalPlayersDictionary.Add(id, go.pv.Owner);
        }
    }
    private void OnPlayerDied(EventObject eo)
    {
        //No one died in this field
        if (!totalUnitsDictionary.ContainsKey(eo.stringObj)) return;
        totalUnitsDictionary[eo.stringObj] = null;
        totalPlayersDictionary[eo.stringObj] = null;
    }

    private void OnDestroy()
    {

        EventManager.StopListening(MyEvents.EVENT_GAME_STARTED, OnGameStartRequested);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }



    public static GameFieldManager instance
    {
        get
        {
            if (!prGameFieldManager)
            {
                prGameFieldManager = FindObjectOfType<GameFieldManager>();
                if (!prGameFieldManager)
                {
                    Debug.LogWarning("There needs to be one active GameFieldManager script on a GameObject in your scene.");
                }
            }

            return prGameFieldManager;
        }
    }
    public static void SetGameMap(GameMode mode) {
        gameFields = new List<GameField>();
        totalUnitsDictionary = new Dictionary<string, Unit_Player>();
        totalPlayersDictionary = new Dictionary<string, Player>();
        switch (mode)
        {
            case GameMode.PVP:
                gameFields.Add(instance.pvpField);
                gameFields[0].InitialiseMap(0);
                break;
            case GameMode.TEAM:
                gameFields.Add(instance.teamField);
                gameFields[0].InitialiseMap(0);
                break;
            case GameMode.Tournament:
                SetUpTournament();
                break;
            case GameMode.PVE:
                break;
        }

    }
    private static void SetUpTournament()
    {
        int i = 0;
        foreach (GameField field in instance.tournamentFields)
        {
            gameFields.Add(field);
            field.InitialiseMap(i++);
        }
    }
    private void OnGameStartRequested(EventObject arg0)
    {
        if (startRoutine != null) return;
        startRoutine = WaitAndStart();
        StartCoroutine(startRoutine);
    }
    IEnumerator startRoutine = null;


    internal static Vector3 RequestRandomPositionOnField(int myIndex, int fieldNo)
    {
       return gameFields[fieldNo].GetRandomPlayerSpawnPosition(myIndex);
    }

    internal static void CheckGameFinished()
    {
        List<Player> survivors = new List<Player>();
        for (int i = 0; i < instance.numActiveFields; i++)
        {
            GameField field = gameFields[0];
            if (field.fieldWinner == null) return;
            survivors.Add(field.fieldWinner);
        }
        //All Field Finished
        if (survivors.Count >= 2)
        {
            //Proceed Tournament
            instance.DistributeRooms(survivors.ToArray());
        }
        else {
            if (PhotonNetwork.IsMasterClient)
            {
                GameSession.PushRoomASetting(ConstantStrings.HASH_GAME_STARTED, false);
            }
            GameSession.GetInst().gameOverManager.SetPanel(survivors[0]);//이거 먼저 호출하고 팝업하세요
            EventManager.TriggerEvent(MyEvents.EVENT_GAME_FINISHED, null);
            EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.GameOver, boolObj = true });
        }
    }


    private IEnumerator WaitAndStart() {
        yield return new WaitForSeconds(0.025f);
        for (int i = 0; i < gameFields.Count; i++)
        {
            if (i < numActiveFields)
            {
                gameFields[i].gameObject.SetActive(true);
                gameFields[i].StartEngine();
            }
            else
            {
                gameFields[i].gameObject.SetActive(false);
            }
        }
    }


    int numActiveFields = 1;

    public void DistributeRooms(Player[] playerList, int maxPlayerPerRoom = -1)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int randomOffset = UnityEngine.Random.Range(0, playerList.Length);
            numActiveFields = (maxPlayerPerRoom < 0) ? 1 :Mathf.CeilToInt((float)playerList.Length / maxPlayerPerRoom);
            for (int i = 0; i < playerList.Length; i++)
            {

                int myRoom = (maxPlayerPerRoom < 0) ? 0 :
                    (i + randomOffset) % numActiveFields;
                Debug.Log("Player " + i + " => " + myRoom);
                HUD_UserName.PushPlayerSetting(playerList[i], "FIELD", myRoom);
            }
        }
    }

    public static Dictionary<string,Unit_Player> GetPlayersInArea(int field = 0) {
        return gameFields[field].playerSpawner.unitsOnMap;
    }

    int playerIterator = 0;
    public static Unit_Player GetNextActivePlayer()
    {
        int iteration = 0;
        List<string> namelist = new List<string>(totalUnitsDictionary.Keys);
        while (iteration < namelist.Count)
        {
            iteration++;
            instance.playerIterator++;
            instance.playerIterator %= totalUnitsDictionary.Count;
            Unit_Player p = totalUnitsDictionary[namelist[instance.playerIterator]];
            if (p != null && p.gameObject.activeInHierarchy)
            {
                return p;
            }
        }
        return null;
    }
    public static GameStatus GetTotalGameStatus()
    {
        GameStatus stat = new GameStatus();
        Team myTeam = (Team)UI_PlayerLobbyManager.GetPlayerProperty("TEAM", Team.HOME);
        bool isTeamGame = GameSession.gameMode == GameMode.TEAM;
        foreach (Unit_Player p in totalUnitsDictionary.Values)
        {
            stat.total++;
            if (p != null && p.gameObject.activeInHierarchy)
            {
                stat.lastSurvivor = p;
                stat.alive++;
                if (isTeamGame)
                {
                    if (p.myTeam != myTeam)
                    {
                        stat.toKill++;
                    }
                    else
                    {
                        stat.alive_ourTeam++;
                    }
                }
                else
                {
                    if (p.pv.Owner.UserId != PhotonNetwork.LocalPlayer.UserId)
                    {
                        stat.toKill++;
                    }
                }
            }
            else
            {
                stat.dead++;
            }
        }
        return stat;
    }
}
