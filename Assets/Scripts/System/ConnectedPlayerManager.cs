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
    public static bool init = false;
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
        if (init) return;
        init = true;
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


    internal static int GetMyIndex(Player[] players, bool useRandom = false)
    {
        instance.Init();
        SortedSet<string> myList = new SortedSet<string>();
        foreach (Player p in players)
        {
            int seed = (int)GetPlayerProperty(p, "SEED", 0);
            string id = (useRandom) ? seed + p.UserId : p.UserId;
            myList.Add(id);
        }
        int i = 0;
        int mySeed = (int)GetPlayerProperty("SEED", 0);
        string myID = (useRandom) ? mySeed + PhotonNetwork.LocalPlayer.UserId : PhotonNetwork.LocalPlayer.UserId;
        foreach (var val in myList)
        {
            if (val == myID) return i;
            i++;
        }
        return 0;
    }
    internal static SortedDictionary<string,int> GetIndexMap(Player[] players, bool useRandom = false)
    {
         instance.Init();
        SortedDictionary<string, string> decodeMap = new SortedDictionary<string, string>();
        foreach (Player p in players)
        {
            int seed = (int)GetPlayerProperty(p, "SEED", 0);
            string id = (useRandom) ? seed + p.UserId : p.UserId;
            decodeMap.Add(id, p.UserId);
        }
        int i = 0;
        SortedDictionary<string, int> indexMap = new SortedDictionary<string, int>();
        foreach (var val in decodeMap)
        {
            indexMap.Add(val.Value, i++);
        }
        return indexMap;
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
        if (!init) instance.Init();
        if (id == null) return null;
        if (instance.playerDict.ContainsKey(id))
        {
            return instance.playerDict[id];
        }
        else
        {
            return null;
        }
    }

    internal static int GetNumberInTeam(Team myTeam)
    {
        if (instance.teamCount == null) return 0;
        if (!instance.teamCount.ContainsKey(myTeam)) return 0;
        return instance.teamCount[myTeam];
    }

    Dictionary<Team, int> teamCount = new Dictionary<Team, int>();
    public static void CountPlayersInTeam() {
        instance.teamCount = new Dictionary<Team, int>();
        foreach (var p in instance.playerDict.Values)
        {
            if (!p.CustomProperties.ContainsKey("TEAM")) continue;
            Team pTeam = (Team)p.CustomProperties["TEAM"];
            if (instance.teamCount.ContainsKey(pTeam))
            {
                instance.teamCount[pTeam]++;
            }
            else {
                instance.teamCount.Add(pTeam, 1);
            }
        }
    }
    public static object GetPlayerProperty(string tag, object value)
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(tag))
        {
            return PhotonNetwork.LocalPlayer.CustomProperties[tag];
        }
        else
        {
            return value;
        }
    }

    public static object GetPlayerProperty(Player p, string tag, object value)
    {
        if (p.CustomProperties.ContainsKey(tag))
        {
            return p.CustomProperties[tag];
        }
        else
        {
            return value;
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
