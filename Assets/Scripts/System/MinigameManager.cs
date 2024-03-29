﻿using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameManager : MonoBehaviourPunCallbacks
{

    Dictionary<string, bool> players = new Dictionary<string, bool>();
    string lastCalledPlayer;
    int nextNumber = 1;
    MinigameCode lastCode = MinigameCode.GameCannotBegin;
    internal PhotonView pv;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);

    }

    private void OnPlayerDied(EventObject arg0)
    {
        string uid = arg0.stringObj;
        if (!players.ContainsKey(uid)) {
            players.Add(uid, false);
        }
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        if (players.ContainsKey(newPlayer.UserId))
        {
            players.Remove(newPlayer.UserId);
        }
    }

    [PunRPC]
    public void AddNumber(string uid, int number) {
        if (!GameSession.gameStarted) {
            lastCode = MinigameCode.WrongNumber;
            return;
        }
        if (!players.ContainsKey(uid)) {
            lastCode = MinigameCode.NotInGame;
            return;
        }
        if (players.Count < 2)
        {
            lastCode = MinigameCode.GameCannotBegin;
            return;
        }
        if (nextNumber == number)
        {
            players[uid] = true;
            lastCalledPlayer = uid;
            nextNumber++;
            if (nextNumber >= players.Count) {
                lastCalledPlayer = GetUncalledPlayerID();
                lastCode = MinigameCode.LastPlayerRemain;
                return;
            }

            lastCode = (number == 1) ? MinigameCode.Begin : MinigameCode.Pass;
            return;
        }
        else if (number == nextNumber - 1) {
            lastCalledPlayer = uid;
            nextNumber++;
            lastCode = MinigameCode.Duplicated;
            return;
        }
        lastCode = MinigameCode.WrongNumber;
    }
    public MinigameCode GetLastCode() {
        return lastCode;
    }
    public void UpdateNextNumber(int num) {
        nextNumber = num;
    }

    string GetUncalledPlayerID() {
        foreach (string id in players.Keys)
        {
            if(!players[id]) return id;
        }
        return lastCalledPlayer;
    }

    public string GetLastCalledPlayer() {
        return lastCalledPlayer;
    }
    [PunRPC]
    public void ResetGame() {
        nextNumber = 1;
        lastCalledPlayer = "";
        lastCode = MinigameCode.GameCannotBegin; 
        List<string> keys = new List<string>(players.Keys);
        foreach (string key in keys)
        {
            players[key] = false;
        }
    }

}
public enum MinigameCode { 
    None,Begin,Pass,Duplicated,WrongNumber,NotInGame,GameCannotBegin,LastPlayerRemain

}
