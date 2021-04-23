using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedPlayerManager : MonoBehaviourPunCallbacks
{
    private static ConnectedPlayerManager prConnMan;
    //***************//

    private void Awake()
    {
        ConnectedPlayerManager[] obj = FindObjectsOfType<ConnectedPlayerManager>();
        if (obj.Length > 1)
        {
            Destroy(gameObject);

        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
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
        Debug.Log("<color=#00ff00>Conn man : current size</color> " + currentPlayerNum);
    }
    public static Dictionary<string, Player> GetPlayerDictionary() {
        return instance.playerDict;
    }
    public static void SetRoomSettings(string key, object value)
    {
        if (instance.roomSettings.ContainsKey(key))
        {
            instance.roomSettings[key] = value;
        }
        else {
            instance.roomSettings.Add(key, value);
        }
    }
    public static object GetRoomSettings(string key, object defaultVal = null) {
        if (instance.roomSettings.ContainsKey(key))
        {
            return instance.roomSettings[key];
        }
        else
        {
            if (defaultVal != null) {
                instance.roomSettings.Add(key, defaultVal);
            }
            return defaultVal;
        }
    }
    public static void SetPlayerSettings(string key, object value)
    {
        if (instance.playerSettings.ContainsKey(key))
        {
            instance.playerSettings[key] = value;
        }
        else
        {
            instance.playerSettings.Add(key, value);
        }
    }
    public static object GetPlayerSettings(string key, object defaultVal = null)
    {
        
        if (instance.playerSettings.ContainsKey(key))
        {
            return instance.playerSettings[key];
        }
        else
        {
            if (defaultVal != null)
            {
                instance.playerSettings.Add(key, defaultVal);
            }
            return defaultVal;
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
        return instance.playerDict[id];
    }
}
