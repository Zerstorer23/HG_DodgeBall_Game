using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuffObjectSpawner : MonoBehaviour
{


    double lastSpawnTime;
    double startTime;
    [SerializeField] GameObject[] buffPrafabs;
    [SerializeField] GameField gameField;
    string prefabName = "Prefabs/BuffObjects/buffObject";

    internal void StartEngine()
    {
        startTime = PhotonNetwork.Time;
    }

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!GameSession.gameStarted || PhotonNetwork.Time < startTime + GameFieldManager.instance.spawnAfter) return;
        float thisDelay = GameFieldManager.instance.spawnDelay;
        if (gameField.suddenDeathCalled) {
            thisDelay /= 2;
        }
        if (PhotonNetwork.Time >= lastSpawnTime + thisDelay) {
            InatantiateBuffObject();
            lastSpawnTime = PhotonNetwork.Time;
        }
    }

    private void InatantiateBuffObject()
    {
        Vector3 randPos = GetRandomPosition();
        int randIndex = Random.Range(0, GameSession.instance.buffConfigs.Length);
        PhotonNetwork.InstantiateRoomObject(prefabName, randPos, Quaternion.identity, 0,
            new object[] {gameField.fieldNo, randIndex }
            );
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
