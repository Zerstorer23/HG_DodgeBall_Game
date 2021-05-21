using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSpec {
    public float xMin, xMax, yMin, yMax, xMid, yMid;
    public override string ToString() {
        return string.Format("X {0}~ {1} + Y {2} ~ {3}", xMin, xMax, yMin, yMax);
    }

    internal bool IsOutOfBound(Vector3 position, float offset = 3f)
    {
        return (position.x < (xMin - offset)
            || position.x > (xMax + offset)
            || position.y < (yMin - offset)
            || position.y > (yMax + offset)
            );

      /*  if (position.x < (xMin - offset)) return true;
        if (position.x > (xMax + offset)) return true;
        if (position.y < (yMin - offset)) return true;
        if (position.y > (yMax + offset)) return true;
        return false;*/
    }
}
public class GameFieldManager : MonoBehaviourPun
{
    public float mapStepsize = 10f;
    public int mapStepPerPlayer = 5;
    //public static float xMin, xMax, yMin, yMax, xMid, yMid;
    private static GameFieldManager prGameFieldManager;
    private static SortedDictionary<string, Unit_Player> totalUnitsDictionary = new SortedDictionary<string, Unit_Player>();
    private SortedDictionary<string, Player> totalPlayersDictionary = new SortedDictionary<string, Player>();
    private SortedDictionary<int, List<Player>> playersInFieldsMap = new SortedDictionary<int, List<Player>>();

    [SerializeField] GameField pvpField, teamField;
    [SerializeField] GameField[] tournamentFields;

    [Header("BuffSpawner")]
    public float spawnAfter = 6f;
    public float spawnDelay = 6f;

    [Header("GameField")]
    public float suddenDeathTime = 60f;
    public double resizeOver = 60d;
    public float resize_EndSize = 10f;

    internal static bool CheckSuddenDeathCalled(int fieldNo)
    {
        return gameFields[fieldNo].suddenDeathCalled;
    }

    public static List<GameField> gameFields = new List<GameField>();

    internal static int GetRemainingPlayerNumber()
    {

            GameStatus stat = new GameStatus(totalUnitsDictionary);
            return stat.toKill;
        
    }

    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStartRequested);
    }

    public static void AddGlobalPlayer(string id, Unit_Player go)
    {
        if (totalUnitsDictionary.ContainsKey(id))
        {
            Debug.LogWarning("Duplicate add player>");
            totalUnitsDictionary[id] = go;
          //  totalPlayersDictionary[id] = go.pv.Owner;
        }
        else
        {
            totalUnitsDictionary.Add(id, go);
           // totalPlayersDictionary.Add(id, go.pv.Owner);
        }
    }
    public static void RemoveDeadPlayer(string id)
    {
        if (!totalUnitsDictionary.ContainsKey(id)) return;
        totalUnitsDictionary[id] = null;
        instance.totalPlayersDictionary[id] = null;
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
        gameFields = new List<GameField>();
        Debug.Log("Received game map " + mode);
        int numRooms = PhotonNetwork.CurrentRoom.PlayerCount + 2;
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
                numRooms = 2;
                break;
            case GameMode.PVE:
                break;
        }
        instance.AssignMyRoom(PhotonNetwork.PlayerList, numRooms);
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
        StartGame();
    }
    public List<Player> survivors;
    internal static void CheckGameFinished()
    {
        instance.survivors = new List<Player>();
        bool finished = instance.CheckOtherFields(instance.survivors);
        if (!finished) return;
        Debug.Log("Found Survivosr " + instance.survivors.Count);
        //All Field Finished
        if (instance.survivors.Count >= 2)
        {

            instance.StartCoroutine(instance.WaitAndContinueTournament(instance.survivors));
            //Proceed Tournament
        }
        else
        {
            FinishTheGame(instance.survivors);
        }
    }
    bool CheckOtherFields(List<Player> survivors) {
        for (int i = 0; i < instance.numActiveFields; i++)
        {
            GameField field = gameFields[i];
            Debug.Log("Field " + i + " finished " + field.gameFieldFinished + " winner " + field.fieldWinner);
            if (!field.gameFieldFinished)
            {
                return false;
            }
            if (field.fieldWinner != null)
            {
                survivors.Add(field.fieldWinner);
            }
        }
        return true;
    }
    private static void FinishTheGame(List<Player> survivors)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameSession.PushRoomASetting(ConstantStrings.HASH_GAME_STARTED, false);
        }
        Player survivor = (survivors.Count > 0) ? survivors[0] : null;
        GameSession.GetInst().gameOverManager.SetPanel(survivor);//이거 먼저 호출하고 팝업하세요
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_FINISHED, null);
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.GameOver, boolObj = true });
    }

    private IEnumerator WaitAndContinueTournament(List<Player> survivors)
    {
        float delay = 3f;
      //  Debug.Log("Open tourny panel ...");
        GameSession.instance.tournamentPanel.SetPanel(survivors.ToArray(), delay);
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.TournamentResult, boolObj = true });
        AssignMyRoom(survivors.ToArray(), 2); 
        GameSession.instance.tournamentPanel.SetNext(playersInFieldsMap);
        yield return new WaitForSeconds(delay);
        StartGame();
        if (GameSession.LocalPlayer_FieldNumber == -1) {
            ChangeToSpectator();
        }
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

    }


    int numActiveFields = 1;
    public void AssignMyRoom(Player[] playerList, int maxPlayerPerRoom)
    {
        totalUnitsDictionary = new SortedDictionary<string, Unit_Player>();
        totalPlayersDictionary = new SortedDictionary<string, Player>();
        playersInFieldsMap = new SortedDictionary<int, List<Player>>();

        int randomOffset = (int)PhotonNetwork.CurrentRoom.CustomProperties[ConstantStrings.HASH_ROOM_RANDOM_SEED];
        Debug.Log("Server seed : " + randomOffset);

        numActiveFields = Mathf.CeilToInt((float)playerList.Length / maxPlayerPerRoom);
        Debug.Log("Active fields : " + numActiveFields);
        Dictionary<string, int> indexMap = ConnectedPlayerManager.GetIndexMap(playerList, true);
        foreach (var entry in indexMap) { 
            int assignField = (entry.Value + randomOffset) % numActiveFields;
            Debug.Log("Player  : "+entry.Key+" -> " +assignField);
            AssociatePlayerToMap(assignField, ConnectedPlayerManager.GetPlayerByID(entry.Key));

            if (entry.Key == PhotonNetwork.LocalPlayer.UserId) {
                Debug.Log("My field  => " + assignField + " max field " + numActiveFields);
                GameSession.SetLocalPlayerFieldNumber(assignField);
            }
        }
        if (!indexMap.ContainsKey(PhotonNetwork.LocalPlayer.UserId)) {
            GameSession.SetLocalPlayerFieldNumber(-1);
        }

    }

    internal static bool PlayerIsActive(string userId)
    {
        Debug.Log("Player active : " + instance.totalPlayersDictionary.ContainsKey(userId));
        return instance.totalPlayersDictionary.ContainsKey(userId);
    }

    public static Player[] GetPlayersInField(int f) {
        Debug.Assert(instance.playersInFieldsMap.ContainsKey(f), " No such field");
        return instance.playersInFieldsMap[f].ToArray();
    }
    private void AssociatePlayerToMap(int field, Player player)
    {
        Debug.Log("Player : " + player);
        if (player == null)
        {
            //TODO
            Debug.LogWarning("No player global add");
            return;
        } 
        if (totalPlayersDictionary.ContainsKey(player.UserId))
        {
            Debug.LogWarning("Duplicated global add");
            totalPlayersDictionary[player.UserId] = player;
        }
        else
        {
            totalPlayersDictionary.Add(player.UserId,player);
        }
        if (!playersInFieldsMap.ContainsKey(field))
        {
            playersInFieldsMap.Add(field, new List<Player>());
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
        if (GameSession.gameMode == GameMode.PVP || GameSession.gameMode == GameMode.TEAM) {
          if(gameFields[0].desolator!=null)  namelist.Add("DESOLATOR");
        }
        while (iteration < namelist.Count)
        {
            iteration++;
            instance.playerIterator++;
            instance.playerIterator %= namelist.Count;
            string name = namelist[instance.playerIterator];
            if (name == "DESOLATOR") {
                return gameFields[0].desolator.gameObject;
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
