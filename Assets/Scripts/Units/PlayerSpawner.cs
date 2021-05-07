using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject myPlayer = null;
    Dictionary<string, Unit_Player> players = new Dictionary<string, Unit_Player>();
    int maxLives = 1;
    CharacterType myCharacter = CharacterType.NONE;

    PhotonView pv;
    private static PlayerSpawner instance;


    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStart);
        pv = GetComponent<PhotonView>();
        instance = this;
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
        EventManager.StopListening(MyEvents.EVENT_GAME_STARTED, OnGameStart);
    }

    public static PlayerSpawner GetInst() => instance;
    public static Dictionary<string, Unit_Player> GetPlayers() => instance.players;


    private void OnGameStart(EventObject arg0)
    {
        players = new Dictionary<string, Unit_Player>();
        DoSpawn();
        StartCoroutine(WaitAndCheck());
    }
    IEnumerator WaitAndCheck() {
        yield return new WaitForFixedUpdate();
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 2 && !GameSession.suddenDeathCalled)
        {
            GameSession.suddenDeathCalled = true;
            EventManager.TriggerEvent(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, new EventObject());
        }
    }


    private void DoSpawn()
    {

        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        var roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        maxLives = (int)roomSetting[ConstantStrings.HASH_PLAYER_LIVES];
        
        if (hash.ContainsKey("CHARACTER"))
        {
            myCharacter = (CharacterType)hash["CHARACTER"];
            if (myCharacter == CharacterType.NONE) {
                myCharacter = GameSession.GetRandomCharacter();
                hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add("ACTUAL_CHARACTER", myCharacter);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
            SpawnPlayer();
        }
        else
        {
            PhotonNetwork.NickName = UI_ChangeName.default_name;
            ChatManager.SendNotificationMessage(PhotonNetwork.NickName + " 님이 난입했습니다.");
            MainCamera.FocusOnField(true);
            ChatManager.SetInputFieldVisibility(true);
            ChatManager.FocusField();
        }

    }


    [SerializeField] UI_StatDisplay statDisplay;
    public void SpawnPlayer()
    {
        if (myPlayer != null) return;
        int myIndex = ConnectedPlayerManager.GetMyIndex();
        Vector3 spawnPos = GameFieldManager.GetInst().GetRandomPlayerSpawnPosition(myIndex);
        myPlayer = PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0);
        myPlayer.GetComponent<PhotonView>().RPC("SetInformation", RpcTarget.AllBuffered, new int[] { (int)myCharacter, (int)maxLives });
        statDisplay.SetPlayer(myPlayer.GetComponent<Unit_Player>());
    }




    private void OnPlayerSpawned(EventObject eo)
    {
        string id = eo.stringObj;
        Unit_Player go = eo.goData.GetComponent<Unit_Player>();
        if (players.ContainsKey(id))
        {
            Debug.LogWarning("Duplicate add player>");
            players[id] = go;
        }
        else
        {
            players.Add(id, go);
        }
    }
    private void OnPlayerDied(EventObject eo)
    {
        players[eo.stringObj] = null;
        if (GameSession.gameStarted)
        {
            pv.RPC("CheckWinConditions", RpcTarget.AllBuffered);
        }
    }
    [PunRPC]
    public void CheckWinConditions()
    {
        GameStatus stat = GetGameStatus();
        Debug.Log("Players alive " + stat.alive);
        if (stat.alive <= 2 && !GameSession.suddenDeathCalled) {
            GameSession.suddenDeathCalled = true;
            EventManager.TriggerEvent(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, new EventObject());
        }
        switch (GameSession.gameMode)
        {
            case GameMode.PVP:
                if (stat.alive > 1) return;
                break;
            case GameMode.TEAM:
                if (stat.toKill > 0 && stat.alive_ourTeam > 0) return;
                break;
            case GameMode.PVE:
                break;
        }


        if (PhotonNetwork.IsMasterClient) {
            var hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add(ConstantStrings.HASH_GAME_STARTED, false);
            PhotonNetwork.CurrentRoom.SetCustomProperties(hash);
        }

        string id = (stat.lastSurvivor == null) ? null : stat.lastSurvivor.pv.Owner.UserId;
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_FINISHED, null);
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.GameOver, boolObj = true });
        GameSession.GetInst().gameOverManager.SetPanel(id);//이거 먼저 호출하고 팝업하세요
    }

    public Transform GetTransformOfPlayer(string id)
    {
        if (players.ContainsKey(id))
        {
            return players[id].transform;
        }
        else
        {
            return null;
        }
    }
    public Unit_Player GetPlayerByOwnerID(string id)
    {
        if (players.ContainsKey(id))
        {
            return players[id];
        }
        else
        {
            return null;
        }
    }
    public static int GetRemainingPlayerNumber() {
        GameStatus stat = instance.GetGameStatus();
        return stat.toKill;
    }

    public static Unit_Player GetLowestScoreActivePlayer() {
        Unit_Player lowP = null;
        int lowestScore = 0;
        foreach (var entry in instance.players)
        {
            if (entry.Value == null)
            { continue; }
            if (entry.Value.gameObject.activeInHierarchy)
            {
                int myScore = StatisticsManager.GetStat(StatTypes.SCORE, entry.Key);
                if (lowP == null || myScore < lowestScore) {
                    lowP = entry.Value;
                    lowestScore = myScore;
                }
            }
        }
        return lowP;
    }
    int playerIterator = 0;
    public static Unit_Player GetNextActivePlayer() {
        int iteration = 0;
        List<string> namelist = new List<string>(instance.players.Keys);
        while (iteration < namelist.Count)
        {
            iteration++;
            instance.playerIterator++;
            instance.playerIterator %= instance.players.Count;
            Unit_Player p = instance.players[namelist[instance.playerIterator]];
            if (p != null && p.gameObject.activeInHierarchy)
            {
                return p;
            }
        }
        return null;
    }

    public static Transform GetNearestPlayerFrom(Vector3 position, string exclusionID = "")
    {
        Debug.Log("Search nearest " + instance.players.Count);
        Transform nearest = null;
        float nearestDistance = 0;
        int i = 0;
        foreach (var entry in instance.players)
        {
            Debug.Log("Search ... " + i++);
            if (entry.Value == null)
            {
                continue;
            }
            if (entry.Value.gameObject.activeInHierarchy)
            {
                if (entry.Value.pv.Owner.UserId == exclusionID) continue;
                Transform trans = entry.Value.gameObject.transform;
                float dist = Vector3.Distance(position, trans.position);
                Debug.Log("Found " + trans.position + " at " + dist);
                if (nearest == null || dist < nearestDistance)
                {
                    nearest = trans;
                    nearestDistance = dist;
                }
            }
        }
        return nearest;
    }

    public GameStatus GetGameStatus() {
        GameStatus stat = new GameStatus();
        bool myTeam = (bool)UI_PlayerLobbyManager.GetPlayerProperty("TEAM", true);
        bool isTeamGame = GameSession.gameMode == GameMode.TEAM;
        foreach (Unit_Player p in players.Values)
        {
            stat.total++;
            if (p != null && p.gameObject.activeInHierarchy)
            {
                stat.lastSurvivor = p;
                stat.alive++;
                if (isTeamGame)
                {
                    if (p.isHomeTeam != myTeam)
                    {
                        stat.toKill++;
                    }
                    else {
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
            else {
                stat.dead++;
            }
        }
        return stat;
    }


}
public class GameStatus {
    public Unit_Player lastSurvivor;
    public int total;
    public int alive;
    public int alive_ourTeam;
    public int dead;
    public int toKill;
}