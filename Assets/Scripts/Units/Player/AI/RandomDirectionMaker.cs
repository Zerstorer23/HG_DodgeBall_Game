using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDirectionMaker 
{
    Vector3 randomDirection;
    double nextRandomTime;
    double randomPeriod = 1d;
    public Vector3 PollRandom() {
        if (PhotonNetwork.Time > nextRandomTime)
        {
            float rx = Random.Range(-1f, 1f);
            float ry = Random.Range(-1f, 1f);
            randomDirection = new Vector3(rx, ry).normalized;
            nextRandomTime = PhotonNetwork.Time + randomPeriod;
        }
        return randomDirection;

    }

}
