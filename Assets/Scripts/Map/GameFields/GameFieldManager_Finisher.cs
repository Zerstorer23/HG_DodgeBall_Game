﻿using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public partial class GameFieldManager : MonoBehaviourPun
{
    public List<UniversalPlayer> survivors = new List<UniversalPlayer>();
    [PunRPC]
    public void NotifyFieldWinner(int fieldNo, string winnerID)
    {
        GameField field = gameFields[fieldNo];
        if (field.gameFieldFinished) return;
        UniversalPlayer winner = PlayerManager.GetPlayerByID(winnerID);
        Debug.Log(fieldNo + " Received notifty field winner " + winner);
        field.gameFieldFinished = true;
        field.fieldWinner = winner;
        field.winnerName = winner.NickName;
        //  Debug.Log("FIeld " + fieldNo + " finished with winner " + fieldWinner);
        EventManager.TriggerEvent(MyEvents.EVENT_FIELD_FINISHED, new EventObject() { intObj = fieldNo });
        field.gameObject.SetActive(false);
        CheckGameFinished();
    }
    internal static void CheckGameFinished()
    {
        instance.survivors.Clear();
        instance.gameFinished = instance.CheckOtherFields(instance.survivors);
        if (!instance.gameFinished) return;
        Debug.Log("Found Survivor " + instance.survivors.Count);
        //All Field Finished
        if (instance.survivors.Count >= 2)
        {
            instance.StartCoroutine(instance.WaitAndContinueTournament(instance.survivors));
            //Proceed Tournament
        }
        else
        {
            UniversalPlayer winner = (instance.survivors.Count > 0) ? instance.survivors[0] : null;
            instance.FinishTheGame(winner.uid);
        }
    }
    bool CheckOtherFields(List<UniversalPlayer> survivors)
    {
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
    internal static void QueryGameFinished()
    {
        UniversalPlayer winner = null;
        for (int i = 0; i < instance.numActiveFields; i++)
        {
            (bool,UniversalPlayer) finishedStat = gameFields[i].QueryFieldFinish();
            Debug.Log("Finished " + finishedStat.Item1 + " winner " + finishedStat.Item2);
            if (!finishedStat.Item1) return;
            winner =finishedStat.Item2;
        }

        //All Field Finished
        Debug.LogWarning("Error game ");
        ChatManager.SendNotificationMessage(LocalizationManager.Convert("_game_error"));
        instance.photonView.RPC("EjectGame",RpcTarget.AllBuffered, winner.uid);
    }
    [PunRPC]
    public void EjectGame(string winnerID)
    {
        for (int i = 0; i < instance.numActiveFields; i++)
        {
            EventManager.TriggerEvent(MyEvents.EVENT_FIELD_FINISHED, new EventObject() { intObj = i });
        }
        instance.gameFinished = true;
        if (PhotonNetwork.IsMasterClient)
        {
            GameSession.PushRoomASetting(ConstantStrings.HASH_GAME_STARTED, false);
        }
        UniversalPlayer winner = PlayerManager.GetPlayerByID(winnerID);
        GameSession.GetInst().gameOverManager.SetPanel(winner);//이거 먼저 호출하고 팝업하세요
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_FINISHED, null);
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.GameOver, boolObj = true });
    }
    [PunRPC]
    public void FinishTheGame(string winnerID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameSession.PushRoomASetting(ConstantStrings.HASH_GAME_STARTED, false);
        }
        UniversalPlayer winner = PlayerManager.GetPlayerByID(winnerID);
        GameSession.GetInst().gameOverManager.SetPanel(winner);//이거 먼저 호출하고 팝업하세요
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_FINISHED, null);
        EventManager.TriggerEvent(MyEvents.EVENT_POP_UP_PANEL, new EventObject() { objData = ScreenType.GameOver, boolObj = true });
    }
    IEnumerator tournamentRoutine;
    IEnumerator TournamentGameChecker()
    {
        yield return new WaitForSeconds(5f);
        while (!instance.gameFinished)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                QueryGameFinished();
            }
            yield return new WaitForSeconds(2f);
        }
    }
}
