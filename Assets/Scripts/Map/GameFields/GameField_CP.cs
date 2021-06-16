using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField_CP : GameField
{
    [SerializeField] Transform[] spawnPositions;
    Map_CapturePointManager cpManager;
    public override void Awake()
    {
        base.Awake();
        cpManager = GetComponentInChildren<Map_CapturePointManager>();
    }
    public override Vector3 GetPlayerSpawnPosition()
    {
        Team team = PlayerManager.LocalPlayer.GetProperty("TEAM", Team.NONE);
        if (team == Team.HOME)
        {
            return spawnPositions[0].position;
        }
        else {
            return spawnPositions[1].position;
        }
    }

    IEnumerator timeoutRoutine;
    public override void OnEnable()
    {
        base.OnEnable();
        timeoutRoutine = GameSession.CheckCoroutine(timeoutRoutine, WaitAndFinishGame());
       if(!GameSession.instance.devMode) StartCoroutine(timeoutRoutine);
    }
    float timeout = 360f;
    IEnumerator WaitAndFinishGame() {
        yield return new WaitForSeconds(timeout);
        float point = cpManager.currentPoint;
        if (point < 0)
        {
            FinishGame(Team.AWAY);
        }
        else 
        {
            FinishGame(Team.HOME);
        }
    }
    public override void OnDisable()
    {
        base.OnDisable();
        StopCoroutine(timeoutRoutine);
    }
    internal void FinishGame(Team winTeam)
    {
        UniversalPlayer winner = PlayerManager.GetPlayerOfTeam(winTeam);
        Debug.Log("GAME FISNISHED /  winner " + winner);
        if (gameFieldFinished) return;
        gameFieldFinished = true;
        fieldWinner = winner;
        winnerName = winner.NickName;
        //  Debug.Log("FIeld " + fieldNo + " finished with winner " + fieldWinner);
        EventManager.TriggerEvent(MyEvents.EVENT_FIELD_FINISHED, new EventObject() { intObj = fieldNo });
        gameObject.SetActive(false);
        GameFieldManager.pv.RPC("FinishTheGame", RpcTarget.AllBufferedViaServer, winner.uid);
    }
    public override void CheckFieldConditions(GameStatus stat)
    {

    }
}
