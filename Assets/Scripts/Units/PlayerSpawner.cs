﻿using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    internal SortedDictionary<string, Unit_Player> unitsOnMap = new SortedDictionary<string, Unit_Player>();
    public List<Unit_Player> debugUnitList = new List<Unit_Player>();
    internal SortedDictionary<string, UniversalPlayer> playersOnMap = new SortedDictionary<string, UniversalPlayer>();
    int maxLives = 1;
    public PlayerSpawnerType spawnerType = PlayerSpawnerType.Once;
    public Unit_SharedMovement desolator;
    [SerializeField] GameField gameField;

    int numHome, numAway;
    private void OnEnable()
    {
        lastDiedPlayer = null;
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);

    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
        ResetPlayerMap();
    }

    public void StartEngine()
    {
        var roomHash = PhotonNetwork.CurrentRoom.CustomProperties;
        int livesIndex = (int)roomHash[ConstantStrings.HASH_PLAYER_LIVES];
        maxLives = UI_MapOptions.lives[livesIndex];

        SpawnLocalPlayer();
        SpawnBots();
        SpawnDesolator();
    }

    private void SpawnBots()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        UniversalPlayer[] botPlayers = PlayerManager.GetBotPlayers();
        foreach(var player in botPlayers)
        {
            SpawnPlayer(player);
        }
    }

    private void SpawnDesolator()
    {
        if (!PhotonNetwork.IsMasterClient || !GameSession.gameModeInfo.useDesolator) return;
        PhotonNetwork.InstantiateRoomObject(ConstantStrings.PREFAB_DESOLATOR, transform.position, Quaternion.identity, 0, new object[] { gameField.fieldNo});

    }

    private void SpawnLocalPlayer()
    {
        Debug.Log("Spawn local");
        UniversalPlayer localPlayer = PlayerManager.LocalPlayer;
        if (GameSession.LocalPlayer_FieldNumber != gameField.fieldNo)
        {
            return;
        }
        if (localPlayer.HasProperty("CHARACTER"))
        {
            SpawnPlayer(localPlayer);
            Debug.LogWarning("Finish spawn");
        }
        else
        {
            //  PhotonNetwork.NickName = UI_ChangeName.default_name;
            ChatManager.SendNotificationMessage(LocalizationManager.Convert("_msg_joined_in_middle", PhotonNetwork.NickName));
            MainCamera.FocusOnField(true);
            ChatManager.SetInputFieldVisibility(true);
        }

    }


    public void SpawnPlayer(UniversalPlayer player)
    {
        Debug.Assert(player != null, " null player !!");
        CharacterType character = player.GetProperty("CHARACTER", CharacterType.NONE);
        Vector3 spawnPos;
        if (character == CharacterType.NONE)
        {
            character = ConfigsManager.GetRandomCharacter();
            player.SetCustomProperties("ACTUAL_CHARACTER", character);
        }

        spawnPos = gameField.GetPlayerSpawnPosition(player);
        GameObject unit;
        if (player.IsBot)
        {

           unit = PhotonNetwork.InstantiateRoomObject(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0,
                new object[] { character, maxLives, gameField.fieldNo, true, player.uid });
        }
        else {

            unit = PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0,
                new object[] { character, maxLives, gameField.fieldNo, false, player.uid });
        }

        if (spawnerType == PlayerSpawnerType.Respawn)
        {
            BuffManager bm = unit.GetComponent<BuffManager>();
            bm.pv.RPC("AddBuff",RpcTarget.AllBuffered, (int)BuffType.InvincibleFromBullets, 1f, 3d);
        }
    }
    [SerializeField] float respawnTime = 5f;
    IEnumerator RespawnPlayer(UniversalPlayer player) {
        Team team = player.GetProperty("TEAM", Team.NONE);
        float modRespawnTime = ModifyRespawnTime(team);
        for (int i = 0; i < modRespawnTime; i++)
        {
            if (player.IsLocal)
            {
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject(LocalizationManager.Convert("_msg_respawn_after",(modRespawnTime - i).ToString("0"))));
            }
            yield return new WaitForSeconds(1f);
        }
        SpawnPlayer(player);    
    }
    float ModifyRespawnTime(Team team)
    {
        return respawnTime;
/*        if ((team == Team.HOME && numAway > numHome)) {
            return respawnTime * ((float)numHome / numAway);
        
        } else if (team == Team.AWAY && numHome > numAway)
        {
            return respawnTime * ((float)numAway / numHome);
        }
        return respawnTime;*/
    }

    public void RegisterPlayer(string uid, Unit_Player unit)
    {
        AddUnitToMap(uid, unit);
        GameFieldManager.AddGlobalPlayer(uid, unit);
        if (unitsOnMap.Count == gameField.expectedNumPlayer)
        {
            //Everyone is spawned
            numHome = PlayerManager.GetNumberInTeam(Team.HOME);
            numAway = PlayerManager.GetNumberInTeam(Team.AWAY);
            if (gameObject.activeInHierarchy)
                StartCoroutine(WaitAndCheck());
        }
    }

    private void AddUnitToMap(string id, Unit_Player go)
    {
        if (unitsOnMap.ContainsKey(id))
        {
            unitsOnMap[id] = go;
            playersOnMap[id] = go.controller.Owner;
        }
        else
        {
            debugUnitList.Add(go);
            unitsOnMap.Add(id, go);
            playersOnMap.Add(id, go.controller.Owner);
        }
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
        string deadID = eo.stringObj;
        lastDiedPlayer = playersOnMap[deadID];
        Debug.Assert(lastDiedPlayer!=null, deadID + " is not on field!!");
        if (gameField.gameFieldFinished)
        {
            //최후의 1인. 맵은 이미 지워져있음
            GameFieldManager.RemoveDeadPlayer(deadID);
            return;
        }
        Debug.Assert(unitsOnMap.ContainsKey(deadID), deadID + " is not on field!!");
        unitsOnMap[deadID] = null;
        playersOnMap[deadID] = null;
        if (desolator != null && !lastDiedPlayer.IsBot)
        {
            desolator.AddController(deadID);
        }

     //   Debug.Log(gameField.fieldNo + ": null the player " + eo.stringObj);
        GameFieldManager.RemoveDeadPlayer(deadID);
        GameStatus stat = new GameStatus(unitsOnMap , lastDiedPlayer);//마지막 1인은 남아있어야함
        gameField.CheckFieldConditions(stat);
        if (spawnerType == PlayerSpawnerType.Respawn && lastDiedPlayer.AmController()) {
            StartCoroutine(RespawnPlayer(lastDiedPlayer));
        }
    }
    public UniversalPlayer lastDiedPlayer = null;


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
    public Unit_Player GetUnitByControllerID(string id)
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
        foreach (var entry in unitsOnMap)
        {
            if (entry.Value == null)
            {
                continue;
            }
            if (entry.Value.gameObject.activeInHierarchy)
            {
                if (entry.Value.controller.Equals(exclusionID)) continue;
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
        unitsOnMap.Clear();
        debugUnitList.Clear();
        playersOnMap.Clear();
    }


}
public enum PlayerSpawnerType { 
    Once,Respawn
}