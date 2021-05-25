using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    internal SortedDictionary<string, Unit_Player> unitsOnMap = new SortedDictionary<string, Unit_Player>();
    public List<Unit_Player> debugUnitList = new List<Unit_Player>();
    internal SortedDictionary<string, Player> playersOnMap = new SortedDictionary<string, Player>();
    int maxLives = 1;
    CharacterType myCharacter = CharacterType.NONE;
    [SerializeField] GameField gameField;


    private void OnEnable()
    {
        //   EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);

    }
    private void OnDisable()
    {
        //  EventManager.StopListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
        ResetPlayerMap();
    }

    public void StartEngine()
    {
        Debug.Log("Reset field units map");
        SpawnLocalPlayer();
        SpawnDesolator();
    }

    private void SpawnDesolator()
    {
        bool useTheMachine = (GameSession.gameMode == GameMode.PVP
   || GameSession.gameMode == GameMode.TEAM);
        if (!PhotonNetwork.IsMasterClient || !useTheMachine) return;

        GameObject desolator =
        PhotonNetwork.Instantiate(ConstantStrings.PREFAB_DESOLATOR, transform.position, Quaternion.identity, 0,
        new object[] { gameField.fieldNo});

    }

    private void SpawnLocalPlayer()
    {
        var hash = PhotonNetwork.LocalPlayer.CustomProperties;
        var roomHash = PhotonNetwork.CurrentRoom.CustomProperties;
        int livesIndex = (int)roomHash[ConstantStrings.HASH_PLAYER_LIVES];
        maxLives = UI_MapOptions.lives[livesIndex];
        if (GameSession.LocalPlayer_FieldNumber != gameField.fieldNo)
        {
            return;
        }
        if (hash.ContainsKey("CHARACTER"))
        {
            myCharacter = (CharacterType)hash["CHARACTER"];
            if (myCharacter == CharacterType.NONE)
            {
                myCharacter = GameSession.GetRandomCharacter();
                HUD_UserName.PushPlayerSetting(PhotonNetwork.LocalPlayer, "ACTUAL_CHARACTER", myCharacter);
            }
            SpawnPlayer();
        }
        else
        {
            //  PhotonNetwork.NickName = UI_ChangeName.default_name;
            ChatManager.SendNotificationMessage(PhotonNetwork.NickName + " 님이 난입했습니다.");
            MainCamera.FocusOnField(true);
            ChatManager.SetInputFieldVisibility(true);
        }

    }


    public void SpawnPlayer()
    {
        Vector3 spawnPos = GetAdjustedPosition();
        Debug.Log("Spawn player at " + spawnPos);
        PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0, new object[] { myCharacter, maxLives, gameField.fieldNo });
    }


    public void RegisterPlayer(string userID, Unit_Player unit)
    {
        AddUnitToMap(userID, unit);
        GameFieldManager.AddGlobalPlayer(userID, unit);
        if (unitsOnMap.Count == gameField.expectedNumPlayer)
        {
            //Everyone is spawned
            if (gameObject.activeInHierarchy)
                StartCoroutine(WaitAndCheck());
        }
    }

    private void AddUnitToMap(string id, Unit_Player go)
    {
        if (unitsOnMap.ContainsKey(id))
        {
            Debug.LogWarning("Duplicate add player at field dictionary");
            unitsOnMap[id] = go;
            playersOnMap[id] = go.pv.Owner;
        }
        else
        {
            Debug.Log(gameField.fieldNo + ": add player at field dictionary " + id);
            debugUnitList.Add(go);
            unitsOnMap.Add(id, go);
            playersOnMap.Add(id, go.pv.Owner);
        }
    }

    private Vector3 GetAdjustedPosition()
    {
        int myIndex = ConnectedPlayerManager.GetMyIndex(GameFieldManager.GetPlayersInField(gameField.fieldNo));
        return gameField.GetRandomPlayerSpawnPosition(myIndex);
    }
    IEnumerator WaitAndCheck()
    {
        //Enable되는걸 기다려야함 그래야 event듣고 사망
        yield return new WaitForFixedUpdate();
        GameStatus stat = new GameStatus(unitsOnMap);
        gameField.CheckFieldConditions(stat, true);
    }

    private void OnPlayerDied(EventObject eo)
    {
        //No one died in this field
        if (eo.intObj != gameField.fieldNo) return;
        if (gameField.gameFieldFinished)
        {
            //최후의 1인. 맵은 이미 지워져있음
            Debug.Log(gameField.fieldNo + ": null the player last, only global " + eo.stringObj);
            GameFieldManager.RemoveDeadPlayer(eo.stringObj);
            return;
        }
        Debug.Assert(unitsOnMap.ContainsKey(eo.stringObj), eo.stringObj + " is not on field!!");
        //   if (!unitsOnMap.ContainsKey(eo.stringObj)) return;
        unitsOnMap[eo.stringObj] = null;
        playersOnMap[eo.stringObj] = null;
        gameField.AddController(eo.stringObj);
        Debug.Log(gameField.fieldNo + ": null the player " + eo.stringObj);
        GameFieldManager.RemoveDeadPlayer(eo.stringObj);
        GameStatus stat = new GameStatus(unitsOnMap);//마지막 1인은 남아있어야함
        gameField.CheckFieldConditions(stat, false);

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


    public Unit_Player GetLowestScoreActivePlayer()
    {
        Unit_Player lowP = null;
        int lowestScore = 0;
        foreach (var entry in unitsOnMap)
        {
            if (entry.Value == null)
            { continue; }
            if (entry.Value.gameObject.activeInHierarchy)
            {
                int myScore = StatisticsManager.GetStat(StatTypes.SCORE, entry.Key);
                if (lowP == null || myScore < lowestScore)
                {
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


    public void ResetPlayerMap()
    {
        Debug.Log("Reset player map on ");
        unitsOnMap = new SortedDictionary<string, Unit_Player>();
        debugUnitList = new List<Unit_Player>();
        playersOnMap = new SortedDictionary<string, Player>();
    }


}
public class GameStatus
{
    public Player lastSurvivor;
    public int total;
    public int alive;
    public int alive_ourTeam;
    public int dead;
    public int toKill;
    public GameStatus(SortedDictionary<string, Unit_Player> unitDict)
    {
        GetGameStatus(unitDict);
    }

    void GetGameStatus(SortedDictionary<string, Unit_Player> unitDict)
    {
        Team myTeam = (Team)UI_PlayerLobbyManager.GetPlayerProperty("TEAM", Team.HOME);
        bool isTeamGame = GameSession.gameMode == GameMode.TEAM;
        foreach (Unit_Player p in unitDict.Values)
        {
            total++;
            if (p != null && p.gameObject.activeInHierarchy)
            {
                lastSurvivor = p.pv.Owner;
                alive++;
                if (isTeamGame)
                {
                    if (p.myTeam != myTeam)
                    {
                        toKill++;
                    }
                    else
                    {
                        alive_ourTeam++;
                    }
                }
                else
                {
                    if (p.pv.Owner.UserId != PhotonNetwork.LocalPlayer.UserId)
                    {
                        toKill++;
                    }
                }
            }
            else
            {
                dead++;
            }
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            lastSurvivor = PhotonNetwork.LocalPlayer;
        }
    }

    public override string ToString() {
        string o = "Game mode : " + GameSession.gameMode+"\n";
         o += "Total Players:" + total+"\n";
         o += "Total Alive:" + alive+"\n";
         o += "Total Alive in my team:"+UI_PlayerLobbyManager.GetPlayerProperty("TEAM",Team.HOME)+" ? " + alive+"\n";
        o += "Total dead:" + dead + "\n";
        o += "Total to kill:" + toKill + "\n";
        o += "Last survivor:" + lastSurvivor + "\n";
        return o;

    }
}