using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffObjectSpawner : MonoBehaviourPun
{
    PhotonView pv;
    public float spawnAfter = 6f;
    public float spawnDelay = 6f;

    double lastSpawnTime;
    double startTime;
    [SerializeField] GameObject[] buffPrafabs;
    string location = "Prefabs/BuffObjects/";

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
    }

 
    private void OnGameStarted(EventObject obj)
    {
        startTime = PhotonNetwork.Time;
    }

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!GameSession.gameStarted || PhotonNetwork.Time < startTime + spawnAfter) return;
        if (PhotonNetwork.Time >= lastSpawnTime + spawnDelay) {
            InatantiateBuffObject();
            lastSpawnTime = PhotonNetwork.Time;
        }
    }

    private void InatantiateBuffObject()
    {
        Vector3 randPos = GetRandomPosition();
        string prefab = location +  GetRandomBuff();
        PhotonNetwork.InstantiateRoomObject(prefab, randPos, Quaternion.identity, 0);
    }

    private string GetRandomBuff()
    {
        return buffPrafabs[UnityEngine.Random.Range(0, buffPrafabs.Length)].name;
    }

    private Vector3 GetRandomPosition()
    {
        Unit_Player lowestPlayer = PlayerSpawner.GetLowestScoreActivePlayer();
        if (lowestPlayer != null)
        {
            return GameSession.GetRandomPosOnMapAround(lowestPlayer.gameObject.transform.position, 10f, 2f);
        }
        else {
            return GameSession.GetRandomPosOnMap(2f);
        }

    }
}
