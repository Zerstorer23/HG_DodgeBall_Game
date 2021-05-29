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
    bool doSpawn = false;

    [SerializeField] GameObject[] buffPrefabs;
    string location = "Prefabs/BuffObjects/";

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);
    }

    private void OnGameFinished(EventObject obj)
    {
        doSpawn = false;
    }

    private void OnGameStarted(EventObject obj)
    {
        startTime = PhotonNetwork.Time;
        doSpawn = true;
    }

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!doSpawn || PhotonNetwork.Time < startTime + spawnAfter) return;
        if (PhotonNetwork.Time >= lastSpawnTime + spawnDelay) {
            InatantiateBuffObject();
            lastSpawnTime = PhotonNetwork.Time;
        }
    }

    private void InatantiateBuffObject()
    {
        Vector3 randPos = GetRandomPosition();
        string prefab = location + GetRandomBuff();
        PhotonNetwork.InstantiateRoomObject(prefab, randPos, Quaternion.identity, 0);
    }

    private string GetRandomBuff()
    {
        return buffPrefabs[UnityEngine.Random.Range(0, buffPrefabs.Length)].name;
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
