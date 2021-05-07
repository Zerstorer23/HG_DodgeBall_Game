using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedPlayerManager : MonoBehaviourPunCallbacks
{
    private static ConnectedPlayerManager prConnMan;
    public int myId = 0;
    //***************//

   public override void OnJoinedRoom()
    {
        Init();
    }

    public static ConnectedPlayerManager instance
    {
        get
        {
            if (!prConnMan)
            {
                prConnMan = FindObjectOfType<ConnectedPlayerManager>();
                if (!prConnMan)
                {
                }
                else {
                   // prConnMan.Init();
                }
            }

            return prConnMan;
        }
    }


    public Dictionary<string, Player> playerDict = new Dictionary<string, Player>();
    public Dictionary<string, object> roomSettings = new Dictionary<string, object>();
    public Dictionary<string, object> playerSettings = new Dictionary<string, object>();
    public int currentPlayerNum = 0;
    public void Init() {
        playerDict = new Dictionary<string, Player>();
        Player[] players = PhotonNetwork.PlayerList;
        foreach (Player p in players) { 
            playerDict.Add(p.UserId,p);
        }
        currentPlayerNum = playerDict.Count;
        roomSettings = new Dictionary<string, object>();
        playerSettings = new Dictionary<string, object>();
        Debug.Log("<color=#00ff00>Conn man : current size</color> " + currentPlayerNum);
    }
    public static Dictionary<string, Player> GetPlayerDictionary() {
        return instance.playerDict;
    }

    internal static int GetMyIndex()
    {

        Player[] players = PhotonNetwork.PlayerList;

        SortedSet<string> myList = new SortedSet<string>();
        foreach (Player p in players) {
            myList.Add(p.UserId);
        }
        int i = 0;
        string myID = PhotonNetwork.LocalPlayer.UserId;
        foreach (var val in myList)
        {
            if(val== myID) return i;
            i++;
        }
        return 0;
    }

    internal static Player GetRandomPlayerExceptMe()
    {
        Player[] players = PhotonNetwork.PlayerListOthers;
        if (players.Length > 0)
        {
            return players[UnityEngine.Random.Range(0, players.Length)];

        }
        else {
            return null;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (!playerDict.ContainsKey(newPlayer.UserId)) {
            playerDict.Add(newPlayer.UserId, newPlayer);
            currentPlayerNum++;
            Debug.Log("<color=#00ff00> Addplayer </color> " + currentPlayerNum);
        }
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        if (playerDict.ContainsKey(newPlayer.UserId))
        {

            playerDict.Remove(newPlayer.UserId);
            currentPlayerNum--;
            Debug.Log("<color=#00ff00> removePlayer </color> " + currentPlayerNum);
        }
    }
    public static Player GetPlayerByID(string id) {
        if (instance.playerDict.ContainsKey(id))
        {

            return instance.playerDict[id];
        }
        else {
            return null;
        }
    }

 /*   public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public void Reconnect()
    {
        if (!PhotonNetwork.IsConnected && wasConnected)
        {
            PhotonNetwork.ReconnectAndRejoin();
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }*/
}
