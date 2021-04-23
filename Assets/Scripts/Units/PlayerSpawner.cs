using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public GameObject myPlayer = null;
    internal Dictionary<string, Unit_Player> players = new Dictionary<string, Unit_Player>();
    int numPlayers = 0;
    int maxLives = 1;
    CharacterType myCharacter = CharacterType.NONE;

    private static PlayerSpawner instance;


    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
        instance = this;
    }
    public static PlayerSpawner GetInst() => instance;

    private void OnGameEnd(EventObject arg0)
    {
        isGameFinished = true;
    }

    bool isGameFinished = false;

    private void Start()
    {

        ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
        ExitGames.Client.Photon.Hashtable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        maxLives = (int)roomSetting[ConstantStrings.HASH_PLAYER_LIVES];
        // maxLives = (int)hash[ConstantStrings.HASH_PLAYER_LIVES];
        Debug.Log("Max lives " + maxLives);
        ConnectedPlayerManager.SetRoomSettings(ConstantStrings.HASH_PLAYER_LIVES, maxLives);

        if (hash.ContainsKey("CHARACTER"))
        {
            myCharacter = (CharacterType)hash["CHARACTER"];
            if (myCharacter == CharacterType.NONE) {
                myCharacter = EventManager.GetRandomCharacter();
                hash = new ExitGames.Client.Photon.Hashtable();
                hash.Add("CHARACTER", myCharacter);
                PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            }
        }
        else {
            myCharacter = EventManager.GetRandomCharacter();
            hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("CHARACTER", myCharacter);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
            PhotonNetwork.NickName =(string) ConnectedPlayerManager.GetPlayerSettings("NICKNAME", "ㅇㅇ");
            ConnectedPlayerManager.GetPlayerSettings("CHARACTER", myCharacter);
        }

        SpawnPlayer();
    }
 

    [SerializeField] UI_StatDisplay statDisplay;
    public void SpawnPlayer()
    {
        if (myPlayer != null || !PhotonNetwork.IsConnectedAndReady || GameSession.GetInst().gameOverManager.IsOver()) return;
        Vector3 spawnPos = GameSession.GetRandomPosOnMap(1f);
        myPlayer = PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0);
        myPlayer.GetComponent<PhotonView>().RPC("SetInformation", RpcTarget.AllBuffered, new int[] { (int)myCharacter, (int)maxLives });

        statDisplay.SetPlayer(myPlayer.GetComponent<Unit_Player>());
    }

    private void OnPlayerDied(EventObject eo)
    {
        if (!isGameFinished)
        {
            string deadID = eo.stringObj;
            players.Remove(deadID);
            numPlayers--;
            CheckWinConditions();
        }
    }
    public void CheckWinConditions()
    {
        if (numPlayers <= 1)
        {
            Unit_Player winner = null;
            foreach (Unit_Player p in players.Values)
            {
                if (p.gameObject.activeInHierarchy)
                {
                    winner = p;
                    break;
                }
            }
            string id = (winner == null) ? null : winner.pv.Owner.UserId;
            GameSession.GetInst().gameOverManager.pv.RPC("ShowPanel",RpcTarget.AllBuffered, id);
        }
    }

    private void OnPlayerSpawned(EventObject eo)
    {
        string id = eo.stringObj;
        Unit_Player go = eo.gameObject.GetComponent<Unit_Player>();
        numPlayers++;
        if (players.ContainsKey(id))
        {
            players[id] = go;
        }
        else
        {
            players.Add(id, go);
        }
    }

    public Transform GetTransformOfPlayer(string id)
    {
        if (players.ContainsKey(id))
        {
            return players[id].transform;
        }
        else
        {
            return null;
        }
    }
    public Unit_Player GetPlayerByOwnerID(string id)
    {
        if (players.ContainsKey(id))
        {
            return players[id];
        }
        else
        {
            return null;
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // currentPlayerNum++;
    }
    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        numPlayers--;
    }

    public static int GetRemainingPlayerNumber() {
        int dead = 0;
        int remain = 0;
        foreach (Unit_Player p in instance.players.Values)
        {
            if (p.gameObject.activeInHierarchy)
            {
                remain++;
            }
            else {
                dead++;
            }
        }
        Debug.Log("Dead " + dead + " remain " + remain);
        return remain;
    }
}
