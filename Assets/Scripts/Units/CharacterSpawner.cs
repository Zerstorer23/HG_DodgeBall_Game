using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSpawner : MonoBehaviourPunCallbacks
{
    public GameObject myPlayer = null;
    internal PhotonView pv;
    Dictionary<string,GameObject> players = new Dictionary<string,GameObject>();
    int numPlayers = 0;
    int maxLives = 1;
    CharacterType myCharacter = CharacterType.HARUHI;

    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerSpawned);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerDied);
    }

    public override void OnJoinedRoom() {
        SpawnPlayer();
    }
    private void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            ExitGames.Client.Photon.Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
            maxLives = (int)PhotonNetwork.CurrentRoom.CustomProperties[ConstantStrings.HASH_PLAYER_LIVES];
            myCharacter = (CharacterType)PhotonNetwork.LocalPlayer.CustomProperties["CHARACTER"];
        }
        SpawnPlayer(); 
    }

    public void SpawnPlayer() {
        if (myPlayer != null || !PhotonNetwork.IsConnectedAndReady) return;
        Vector3 spawnPos = GameSession.GetRandomPosOnMap(1f);
        myPlayer= PhotonNetwork.Instantiate(ConstantStrings.PREFAB_PLAYER, spawnPos, Quaternion.identity, 0);   
        myPlayer.GetComponent<PhotonView>().RPC("SetInformation",RpcTarget.AllBuffered,(int)myCharacter,(int)maxLives);
    }

    private void OnPlayerDied(EventObject eo)
    {
        players.Remove(eo.stringObj);
        numPlayers--;
    }

    private void OnPlayerSpawned(EventObject eo)
    {
        string id = eo.stringObj;
        GameObject go = eo.gameObject;
        numPlayers++;
        if (players.ContainsKey(id))
        {
            players[id] = go;
        }
        else {
            players.Add(id, go);
        }
    }

    public Transform GetTransformOfPlayer(string id) {
        if (players.ContainsKey(id))
        {
            return players[id].transform;
        }
        else
        {
            return null;
        }
    }

}
