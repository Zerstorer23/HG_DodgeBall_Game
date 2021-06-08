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
    public Unit_SharedMovement desolator;
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
        SpawnLocalPlayer();
       // SpawnBots();
        SpawnDesolator();
    }

    private void SpawnBots()
    {
        if (!GameSession.instance.devMode) return;
        for (int i = 0; i < 3; i++) {
            CharacterType botCharacter = ConfigsManager.GetRandomCharacter();
            Vector3 spawnPos = gameField.GetRandomPosition(1); 
            PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0, new object[] { botCharacter, maxLives, gameField.fieldNo, true });
        }
    }

    private void SpawnDesolator()
    {
        if (!PhotonNetwork.IsMasterClient || !GameSession.gameModeInfo.useDesolator) return;

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
                myCharacter = ConfigsManager.GetRandomCharacter();
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
        PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0, new object[] { myCharacter, maxLives, gameField.fieldNo,false });
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
        GameStatus stat = new GameStatus(unitsOnMap, null);
        gameField.CheckFieldConditions(stat);
    }

    private void OnPlayerDied(EventObject eo)
    {
        //No one died in this field
        if (eo.intObj != gameField.fieldNo) return;
        if (gameField.gameFieldFinished)
        {
            //최후의 1인. 맵은 이미 지워져있음
          //  Debug.Log(gameField.fieldNo + ": null the player last, only global " + eo.stringObj);
            GameFieldManager.RemoveDeadPlayer(eo.stringObj);
            return;
        }
        Debug.Assert(unitsOnMap.ContainsKey(eo.stringObj), eo.stringObj + " is not on field!!");
        //   if (!unitsOnMap.ContainsKey(eo.stringObj)) return;
        Player lastDiedPlayer = playersOnMap[eo.stringObj];
        unitsOnMap[eo.stringObj] = null;
        playersOnMap[eo.stringObj] = null;
        if (desolator != null)
        {
            desolator.AddController(eo.stringObj);
        }

     //   Debug.Log(gameField.fieldNo + ": null the player " + eo.stringObj);
        GameFieldManager.RemoveDeadPlayer(eo.stringObj);
        GameStatus stat = new GameStatus(unitsOnMap , lastDiedPlayer);//마지막 1인은 남아있어야함
        gameField.CheckFieldConditions(stat);

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
        if (id == null) return null;
        if (unitsOnMap.ContainsKey(id))//check null id
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
        Transform nearest = null;
        float nearestDistance = 0;
        int i = 0;
        foreach (var entry in unitsOnMap)
        {
            if (entry.Value == null)
            {
                continue;
            }
            if (entry.Value.gameObject.activeInHierarchy)
            {
                if (entry.Value.pv.Owner.UserId == exclusionID) continue;
                Transform trans = entry.Value.gameObject.transform;
                float dist = Vector3.Distance(position, trans.position);
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
        unitsOnMap = new SortedDictionary<string, Unit_Player>();
        debugUnitList = new List<Unit_Player>();
        playersOnMap = new SortedDictionary<string, Player>();
    }


}
