using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject myPlayer = null;
   internal Dictionary<string, Unit_Player> unitsOnMap = new Dictionary<string, Unit_Player>();
    internal Dictionary<string, Player> playersOnMap = new Dictionary<string, Player>();
    int maxLives = 1;
    CharacterType myCharacter = CharacterType.NONE;
    [SerializeField] GameField gameField;

    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }

    public void StartEngine()
    {
        unitsOnMap = new Dictionary<string, Unit_Player>();
        playersOnMap = new Dictionary<string, Player>();
        SpawnLocalPlayer();
        StartCoroutine(WaitAndCheck());
    }
    IEnumerator WaitAndCheck() {
        yield return new WaitForFixedUpdate();
        CheckFieldConditions();
    }


    private void SpawnLocalPlayer()
    {
        int fieldNo = (int)PhotonNetwork.LocalPlayer.CustomProperties["FIELD"];
        if (fieldNo != gameField.fieldNo) {
            return;
        }

        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        var roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        int liveIndex = (int)roomSetting[ConstantStrings.HASH_PLAYER_LIVES];
        maxLives = UI_MapOptions.lives[liveIndex];
        
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


    public void SpawnPlayer()
    {
        if (myPlayer != null) return;
        int myIndex = ConnectedPlayerManager.GetMyIndex();
        Vector3 spawnPos = GameFieldManager.RequestRandomPositionOnField(myIndex,gameField.fieldNo );
        myPlayer = PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0, new object[] { myCharacter, maxLives, gameField.fieldNo });
        UI_StatDisplay.SetPlayer(myPlayer.GetComponent<Unit_Player>());
    }

    private void OnPlayerSpawned(EventObject eo)
    {
        string id = eo.stringObj;
        Unit_Player go = eo.goData.GetComponent<Unit_Player>();
        if (unitsOnMap.ContainsKey(id))
        {
            Debug.LogWarning("Duplicate add player>");
            unitsOnMap[id] = go;
            playersOnMap[id] = go.pv.Owner;
        }
        else
        {
            unitsOnMap.Add(id, go);
            playersOnMap.Add(id, go.pv.Owner);
        }
    }
    private void OnPlayerDied(EventObject eo)
    {
        //No one died in this field
        if (!unitsOnMap.ContainsKey(eo.stringObj)) return;

        unitsOnMap[eo.stringObj] = null;
        playersOnMap[eo.stringObj] = null;
        if (gameField.fieldWinner == null) // Winner did not come out yet.
        {
            CheckFieldConditions();
        }
    }

    private void CheckFieldConditions()
    {
        GameStatus stat = GetGameStatus();
        Debug.Log("Players alive " + stat.alive);
        CheckSuddenDeath(stat.alive);
        bool fieldFinished = true;
        switch (GameSession.gameMode)
        {
            case GameMode.PVP:
                fieldFinished = (stat.alive <= 1) ;
                break;
            case GameMode.TEAM:
                fieldFinished = (stat.toKill <= 0 || stat.alive_ourTeam <= 0);
                break;
            case GameMode.PVE:
                break;
        }
        if (!fieldFinished) return;

        Player winner = (stat.lastSurvivor == null) ? null : stat.lastSurvivor.pv.Owner;
        gameField.NotifyFieldWinner(winner);

    }
    public void CheckSuddenDeath(int numAlive) {
        if (GameSession.gameMode != GameMode.PVP
            || GameSession.gameMode != GameMode.TEAM
            ) return;
        if (numAlive <= 2 && !gameField.suddenDeathCalled)
        {
            gameField.suddenDeathCalled = true;
            EventManager.TriggerEvent(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, new EventObject());
        }
    }

    public Transform GetTransformOfPlayer(string id)
    {
        if (unitsOnMap.ContainsKey(id))
        {
            return unitsOnMap[id].transform;
        }
        else
        {
            return null;
        }
    }
    public Unit_Player GetPlayerByOwnerID(string id)
    {
        if (unitsOnMap.ContainsKey(id))
        {
            return unitsOnMap[id];
        }
        else
        {
            return null;
        }
    }


    public Unit_Player GetLowestScoreActivePlayer() {
        Unit_Player lowP = null;
        int lowestScore = 0;
        foreach (var entry in unitsOnMap)
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
    
    public Transform GetNearestPlayerFrom(Vector3 position, string exclusionID = "")
    {
        Debug.Log("Search nearest " + unitsOnMap.Count);
        Transform nearest = null;
        float nearestDistance = 0;
        int i = 0;
        foreach (var entry in unitsOnMap)
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
        Team myTeam = (Team)UI_PlayerLobbyManager.GetPlayerProperty("TEAM", Team.HOME);
        bool isTeamGame = GameSession.gameMode == GameMode.TEAM;
        foreach (Unit_Player p in unitsOnMap.Values)
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