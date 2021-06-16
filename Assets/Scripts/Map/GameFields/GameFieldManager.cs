using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class GameFieldManager : MonoBehaviourPun
{
    public float mapStepsize = 10f;
    public int mapStepPerPlayer = 5;
    //public static float xMin, xMax, yMin, yMax, xMid, yMid;
    private static GameFieldManager prGameFieldManager;
    private static SortedDictionary<string, Unit_Player> totalUnitsDictionary = new SortedDictionary<string, Unit_Player>();
    private SortedDictionary<int, List<UniversalPlayer>> playersInFieldsMap = new SortedDictionary<int, List<UniversalPlayer>>();

    [SerializeField] GameField singleField;
    [SerializeField] GameField field_CP;
    [SerializeField] GameField field_CP_FFA;

    [Header("BuffSpawner")]
    public float spawnAfter = 6f;
    public float spawnDelay = 6f;

    [Header("GameField")]
    public float suddenDeathTime = 60f;
    public double resizeOver = 60d;
    public float resize_EndSize = 10f;
    [Header("PVE settings")]
    public double incrementEverySeconds = 4d;

    public static PhotonView pv;
    public static List<GameField> gameFields = new List<GameField>();
    public bool gameFinished = false;
    internal static bool CheckSuddenDeathCalled(int fieldNo)
    {
        return gameFields[fieldNo].suddenDeathCalled;
    }


    internal static int GetRemainingPlayerNumber()
    {
        var list = from Unit_Player p in totalUnitsDictionary.Values
                   where (p != null && p.gameObject.activeInHierarchy && !p.controller.IsLocal)
                   select p;

        return list.Count();
    }

    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStartRequested);
        pv = GetComponent<PhotonView>();
    }
    
    public static void AddGlobalPlayer(string id, Unit_Player go)
    {
        if (totalUnitsDictionary.ContainsKey(id))
        {
            totalUnitsDictionary[id] = go;
        }
        else
        {
            totalUnitsDictionary.Add(id, go);
        }
    }
    public static void RemoveDeadPlayer(string id)
    {
        if (!totalUnitsDictionary.ContainsKey(id)) return;
        totalUnitsDictionary.Remove(id);
    }

    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_GAME_STARTED, OnGameStartRequested);
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
        gameFields.Clear();
        switch (mode)
        {
            case GameMode.PVP:
            case GameMode.TEAM:
            case GameMode.PVE:
                SetUpSingleField(instance.singleField);
                instance.AssignSingleRoom(PlayerManager.GetPlayers());
                break;
            case GameMode.TeamCP:
                SetUp_CP();
                instance.AssignSingleRoom(PlayerManager.GetPlayers());
                break;
            case GameMode.Tournament:
                SetUpTournament();
                instance.AssignMyRoom(PlayerManager.GetHumanPlayers(), 2);
                break;
        }
    }

    private static void SetUp_CP()
    {
        int homeNum = PlayerManager.GetNumberInTeam(Team.HOME);
        int awayNum = PlayerManager.GetNumberInTeam(Team.AWAY);
        if (homeNum == awayNum)
        {
            SetUpSingleField(instance.field_CP_FFA);
        }
        else
        {
           // SetUpSingleField(instance.field_CP_FFA);
            SetUpSingleField(instance.field_CP);
        }
    }

    private static void SetUpSingleField(GameField field)
    {
        gameFields.Add(field);
        gameFields[0].InitialiseMap(0);
        Debug.LogWarning("Add gamefield 0");
    }
    private void OnGameStartRequested(EventObject arg0)
    {
        gameFinished = false;
        StartGame();
    }
   
   
    private void StartGame() {
        var roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        MapDifficulty mapDiff = (MapDifficulty)roomSetting[ConstantStrings.HASH_MAP_DIFF];

        for (int i = 0; i < gameFields.Count; i++)
        {
            if (i < numActiveFields)
            {
                gameFields[i].gameObject.SetActive(true);
                gameFields[i].expectedNumPlayer = playersInFieldsMap[i].Count;
                gameFields[i].StartEngine(mapDiff);
            }
            else
            {
                gameFields[i].gameObject.SetActive(false);
            }
        }
        CheckGoogleEvents();
        if(GameSession.gameModeInfo.gameMode == GameMode.Tournament) {
            tournamentRoutine = GameSession.CheckCoroutine(tournamentRoutine, TournamentGameChecker());
            StartCoroutine(tournamentRoutine);
        }
    }

 

    private void CheckGoogleEvents()
    {
        if (PhotonNetwork.IsMasterClient) {

            switch (GameSession.gameModeInfo.gameMode)
            {
                case GameMode.PVP:
                    GooglePlayManager.IncrementEvent(GPGSIds.event_pvp_played, 1);
                    break;
                case GameMode.TEAM:
                    GooglePlayManager.IncrementEvent(GPGSIds.event_team_played, 1);
                    break;
                case GameMode.Tournament:
                    GooglePlayManager.IncrementEvent(GPGSIds.event_tournament_played, 1);
                    break;
                case GameMode.PVE:
                    GooglePlayManager.IncrementEvent(GPGSIds.event_pvp_played, 1);
                    break;
            }
            GooglePlayManager.IncrementEvent(GPGSIds.event_total_games_played, 1);
            GooglePlayManager.IncrementEvent(GPGSIds.event_total_users_connected, PhotonNetwork.CurrentRoom.PlayerCount);
        }
    }

    int numActiveFields = 1;
    public void AssignMyRoom(UniversalPlayer[] playerList, int maxPlayerPerRoom)
    {
        //TODO Tournament 봇 막기
        totalUnitsDictionary.Clear();
        playersInFieldsMap.Clear();

        int randomOffset = (int)PhotonNetwork.CurrentRoom.CustomProperties[ConstantStrings.HASH_ROOM_RANDOM_SEED];
        numActiveFields = Mathf.CeilToInt((float)playerList.Length / maxPlayerPerRoom);
        string o = "<color=#00c8c8>=============Active fields :"+ numActiveFields + "====================</color>\n";
        SortedDictionary<string, int> indexMap = PlayerManager.GetIndexMap(playerList, true);
        foreach (var entry in indexMap)
        {
            int assignField = (entry.Value + randomOffset) % numActiveFields;
            UniversalPlayer player = PlayerManager.GetPlayerByID(entry.Key);
            o += "Player : " + player+"\n";
            AssociatePlayerToMap(assignField, player);

            if (entry.Key == PhotonNetwork.LocalPlayer.UserId)
            {
                GameSession.SetLocalPlayerFieldNumber(assignField);
                o += "-MyField : "+ assignField + " \n";
            }
        }
        if (!indexMap.ContainsKey(PhotonNetwork.LocalPlayer.UserId))
        {
            GameSession.SetLocalPlayerFieldNumber(-1);
            o += "-MyField : -1 \n";
        }
        o+="===================================== \n";
        Debug.Log(o);
    }
    public void AssignSingleRoom(UniversalPlayer[] playerList)
    {
        //TODO Tournament 봇 막기
        totalUnitsDictionary.Clear();
        playersInFieldsMap.Clear();
        numActiveFields = 1;
        int fieldNo = 0;
        string o = "<color=#00c8c8>=============Active fields :" + numActiveFields + "====================</color>\n";
        foreach (var player in playerList)
        {
            o += "Player : " + player + "\n";
            AssociatePlayerToMap(fieldNo, player);
            if (player.uid == PhotonNetwork.LocalPlayer.UserId)
            {
                GameSession.SetLocalPlayerFieldNumber(fieldNo);
                o += "-MyField : " + fieldNo + " \n";
            }
        }
        o += "===================================== \n";
        Debug.Log(o);
    }

    public static UniversalPlayer[] GetPlayersInField(int f) {
        Debug.Assert(instance.playersInFieldsMap.ContainsKey(f), " No such field");
        return instance.playersInFieldsMap[f].ToArray();
    }
    private void AssociatePlayerToMap(int field, UniversalPlayer player)
    {
        if (!playersInFieldsMap.ContainsKey(field))
        {
            playersInFieldsMap.Add(field, new List<UniversalPlayer>());
        }
        playersInFieldsMap[field].Add(player);
    }

    public static SortedDictionary<string,Unit_Player> GetPlayersInArea(int field = 0) {
        return gameFields[field].playerSpawner.unitsOnMap;
    }

    int playerIterator = 0;
    public static GameObject GetNextActivePlayer()
    {
        int iteration = 0;
        List<string> namelist = new List<string>(totalUnitsDictionary.Keys);
        if (GameSession.gameModeInfo.useDesolator) {
          if(gameFields[0].playerSpawner.desolator!=null)  namelist.Add("DESOLATOR");
        }
        while (iteration < namelist.Count)
        {
            iteration++;
            instance.playerIterator++;
            instance.playerIterator %= namelist.Count;
            string name = namelist[instance.playerIterator];
            if (name == "DESOLATOR") {
                return gameFields[0].playerSpawner.desolator.gameObject;
            }
            Unit_Player p = totalUnitsDictionary[name];
            if (p != null && p.gameObject.activeInHierarchy)
            {
                return p.gameObject;
            }
        }
        return null;
    }
    public static void ChangeToSpectator()
    {
        instance.StartCoroutine(instance.WaitAndSpectate());
    }
    public IEnumerator WaitAndSpectate()
    {
        yield return new WaitForSeconds(1f);
        ChatManager.SetInputFieldVisibility(true);
        MainCamera.FocusOnField(true);
       // MainCamera.instance.FocusOnAlivePlayer();
    }

}
