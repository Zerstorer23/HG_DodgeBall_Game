using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField_CP : GameField
{
    [SerializeField] Transform[] spawnPositions;
    internal Map_CapturePointManager cpManager;
    public override void Awake()
    {
        base.Awake();
        cpManager = GetComponentInChildren<Map_CapturePointManager>();
    }
    public override Vector3 GetPlayerSpawnPosition(UniversalPlayer myPlayer)
    {
        Team team = myPlayer.GetProperty("TEAM", Team.NONE);
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
        StartCoroutine(timeoutRoutine);
    }
    public static float timeout = 180f;
    IEnumerator WaitAndFinishGame() {
        float elapsedTime = 0f;
        Team winner;
        while (elapsedTime < timeout) {
            winner = cpManager.GetTeamWithMaxPoint();
            if (winner != Team.NONE)
            {
                if (PhotonNetwork.IsMasterClient) {
                    photonView.RPC("FinishGame", RpcTarget.AllBuffered, (int)winner);
                }
                yield break;
            }
            else {
                elapsedTime++;
                yield return new WaitForSeconds(1f);
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            winner = cpManager.GetHighestTeam();
            photonView.RPC("FinishGame", RpcTarget.AllBuffered, (int)winner);
        }

    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (timeoutRoutine != null) {

            StopCoroutine(timeoutRoutine);
        }
    }
    [PunRPC]
    public void FinishGame(Team winTeam)
    {
        UniversalPlayer winner = PlayerManager.GetPlayerOfTeam(winTeam);
        if (gameFieldFinished) return;
        if (timeoutRoutine != null)
        {
            StopCoroutine(timeoutRoutine);
        }

        Debug.LogWarning("GAME FISNISHED /  winner " + winner);
        gameFieldFinished = true;
        fieldWinner = winner;
        winnerName = winner.NickName;
        //  Debug.Log("FIeld " + fieldNo + " finished with winner " + fieldWinner);
        EventManager.TriggerEvent(MyEvents.EVENT_FIELD_FINISHED, new EventObject() { intObj = fieldNo });
        gameObject.SetActive(false);
        GameFieldManager.instance.FinishTheGame(winner.uid);
    }
    public override void CheckFieldConditions(GameStatus stat)
    {
        //Intentionally left blank to do nothing

    }
}
