using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    private static PlayerManager prConnMan;
    public int myId = 0;
    public bool init = false;
    //***************//

   public override void OnJoinedRoom()
    {
        Init();
    }

    public static PlayerManager instance
    {
        get
        {
            if (!prConnMan)
            {
                prConnMan = FindObjectOfType<PlayerManager>();
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

    internal static void AddBotPlayer(UniversalPlayer botPlayer)
    {
        instance.playerDict.Add(botPlayer.uid, botPlayer);
        instance.currentPlayerNum++;
    }
    public static void RemoveBotPlayer(string uid) {
        if (instance.playerDict.ContainsKey(uid)) {
            instance.playerDict.Remove(uid);
            instance.currentPlayerNum--;
        }
    }

    public Dictionary<string, UniversalPlayer> playerDict = new Dictionary<string, UniversalPlayer>();

    public int currentPlayerNum = 0;
    public void Init() {
        if (init) return;
        init = true;
        playerDict.Clear();
        Player[] players = PhotonNetwork.PlayerList;
        foreach (Player p in players) {
            UniversalPlayer uPlayer = new UniversalPlayer(p);
            playerDict.Add(p.UserId,uPlayer);
        }
        currentPlayerNum = playerDict.Count;
        Debug.Log("<color=#00ff00>Conn man : current size</color> " + currentPlayerNum);
    }

   public static int botIDnumber = 0;
    internal static string PollBotID()
    {
        botIDnumber++;
        return "T-"+PhotonNetwork.LocalPlayer.UserId+"-"+botIDnumber;
    }

    public static Dictionary<string, UniversalPlayer> GetPlayerDictionary() {
        return instance.playerDict;
    }

    internal static int GetMyIndex(UniversalPlayer myPlayer, UniversalPlayer[] players, bool useRandom = false)
    {
        instance.Init();
        SortedSet<string> myList = new SortedSet<string>();
        foreach (UniversalPlayer p in players)
        {
            int seed = p.GetProperty("SEED", 0);
            string id = (useRandom) ? seed + p.uid : p.uid;
            myList.Add(id);
        }
        int i = 0;
        int mySeed = myPlayer.GetProperty("SEED", 0);
        string myID = (useRandom) ? mySeed + myPlayer.uid : myPlayer.uid;
        foreach (var val in myList)
        {
            if (val == myID) return i;
            i++;
        }
        return 0;
    }
    internal static SortedDictionary<string,int> GetIndexMap(UniversalPlayer[] players, bool useRandom = false)
    {
         instance.Init();
        SortedDictionary<string, string> decodeMap = new SortedDictionary<string, string>();
        foreach (UniversalPlayer p in players)
        {
            int seed = p.GetProperty("SEED", 0);
            string id = (useRandom) ? seed + p.uid : p.uid;
            decodeMap.Add(id, p.uid);
        }
        int i = 0;
        SortedDictionary<string, int> indexMap = new SortedDictionary<string, int>();
        foreach (var val in decodeMap)
        {
            indexMap.Add(val.Value, i++);
        }
        return indexMap;
    }

    internal static UniversalPlayer[] GetHumanPlayers()
    {
        var list = from UniversalPlayer p in instance.playerDict.Values
                   where p.IsHuman
                   select p;
        return list.ToArray();
    }
    internal static UniversalPlayer[] GetBotPlayers()
    {
        var list = from UniversalPlayer p in instance.playerDict.Values
                   where p.IsBot
                   select p;
        return list.ToArray();
    }
    public static void RemoveAllBots()
    {

        var list = (from UniversalPlayer p in instance.playerDict.Values
                   where p.IsBot
                   select p.uid).ToArray();
        foreach (string s in list) {
            instance.playerDict.Remove(s);
        }
        botIDnumber = 0;
    }
    internal static UniversalPlayer[] GetPlayers() {
        return instance.playerDict.Values.ToArray();
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
        if (!playerDict.ContainsKey(newPlayer.UserId))
        {
            UniversalPlayer uPlayer = new UniversalPlayer(newPlayer);
            playerDict.Add(newPlayer.UserId, uPlayer);
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
    public static UniversalPlayer GetPlayerByID(string id) {
        instance.Init();
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
    public static UniversalPlayer GetPlayerOfTeam(Team team)
    {
        instance.Init();
        return (from UniversalPlayer p in instance.playerDict.Values
                   where p.GetProperty("TEAM", Team.NONE) == team
                   select p).First();
                   
/*
        foreach (var p in instance.playerDict.Values)
        {
            if (!p.HasProperty("TEAM")) continue;
            Team pTeam = p.GetProperty<Team>("TEAM");
            if (pTeam == team)
            {
                return p;
            }
        }
        return null;*/
    }

    internal static int GetNumberInTeam(Team myTeam)
    {
        return (from UniversalPlayer p in instance.playerDict.Values
                where p.GetProperty("TEAM", Team.NONE) == myTeam
                select p).Count();
/*
        if (instance.teamCount == null) return 0;
        if (!instance.teamCount.ContainsKey(myTeam)) return 0;
        return instance.teamCount[myTeam];*/
    }

    readonly Dictionary<Team, int> teamCount = new Dictionary<Team, int>();
    public static void CountPlayersInTeam() {
        instance.teamCount.Clear();
        foreach (var p in instance.playerDict.Values)
        {
            if (!p.HasProperty("TEAM")) {
                Debug.LogWarning("No team"+p);
                continue;
            }
            Team pTeam = p.GetProperty<Team>("TEAM");
            Debug.LogWarning(p +"+ team" + pTeam);
            if (instance.teamCount.ContainsKey(pTeam))
            {
                instance.teamCount[pTeam]++;
            }
            else {
                instance.teamCount.Add(pTeam, 1);
            }
        }
    }
    public static UniversalPlayer LocalPlayer { 
        get => instance.playerDict [PhotonNetwork.LocalPlayer.UserId];
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected " + cause.ToString());
        ChatManager.SendLocalMessage("게임연결이 끊겼습니다. 클라이언트를 재가동해주세요.");
        instance.init = false;
        SceneManager.LoadScene(0);
       
    }
    public override void OnLeftRoom()
    {
        if (PhotonNetwork.IsConnected)
        StartCoroutine(WaitAndReset());
    }
    IEnumerator WaitAndReset() {
        Debug.LogWarning("Reload scene in 2 seconds");
        yield return new WaitForSeconds(2f);
        embarkCalled = false;
        instance.init = false;
        SceneManager.LoadScene(0);
        MenuManager.JoinRoom();
    }
  static  IEnumerator WaitAndQuit()
    {
        Debug.LogWarning("Reload scene in 1 seconds");
        yield return new WaitForSeconds(0.5f);
        GameSession.instance.LeaveRoom();
    }
    public static bool embarkCalled = false;
    public static void ReconnectEveryone() {
        if (!PhotonNetwork.IsMasterClient) return;
        GameSession.PushRoomASetting(ConstantStrings.HASH_GAME_STARTED, false);
        GameSession.instance.photonView.RPC("LeaveRoom", RpcTarget.Others);
        instance.StartCoroutine(WaitAndQuit());
    }
    public static void KickEveryoneElse()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GameSession.instance.photonView.RPC("QuitGame", RpcTarget.Others);
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
