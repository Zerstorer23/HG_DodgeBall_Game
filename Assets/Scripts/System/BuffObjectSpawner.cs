using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffObjectSpawner : MonoBehaviour
{
    public float spawnAfter = 6f;
    public float spawnDelay = 6f;

    double lastSpawnTime;
    double startTime;
    [SerializeField] GameObject[] buffPrafabs;
    [SerializeField] GameField gameField;
    string location = "Prefabs/BuffObjects/";

    internal void StartEngine()
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
        Unit_Player lowestPlayer = gameField.playerSpawner.GetLowestScoreActivePlayer();
        if (lowestPlayer != null)
        {
            return gameField.GetRandomPositionNear(lowestPlayer.gameObject.transform.position, 10f, 2f);
        }
        else {
            return gameField.GetRandomPosition(2f);
        }

    }


}
