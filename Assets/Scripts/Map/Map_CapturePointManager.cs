using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_CapturePointManager : MonoBehaviourPun
{
    [SerializeField] Map_CapturePoint[] capturePoints;
    Dictionary<int, Map_CapturePoint> mapDictionary = new Dictionary<int, Map_CapturePoint>();
    GameField_CP gameFieldCP;
    public int maxIndex, homeNext, awayNext = 0;
    public bool serialCaptureRequired = true;
    float pointPerSec = 0.5f;
    public float endThreshold = 100f;
    public float currentPoint = 0;
    public bool finishOnAllCapture = false;
    private void Awake()
    {
        capturePoints = GetComponentsInChildren<Map_CapturePoint>();
        gameFieldCP = GetComponentInParent<GameField_CP>();
        mapDictionary.Clear();
        foreach (var cp in capturePoints)
        {
            if (cp.captureIndex >= maxIndex)
            {
                maxIndex = cp.captureIndex;
            }
            mapDictionary.Add(cp.captureIndex, cp);
        }

    }
    private void OnEnable()
    {
        gameFieldCP.gameFieldFinished = false;
        currentPoint = 0;
        awayNext = maxIndex;
        homeNext = 0;
        UI_RemainingPlayerDisplay.SetCPManager(this);
        EventManager.StartListening(MyEvents.EVENT_CP_CAPTURED, OnPointCaptured);
        if (pointerRoutine != null) StopCoroutine(pointerRoutine);
        pointerRoutine = PointCounter();
      //  Debug.Log("pointer count " + pointerRoutine);
        StartCoroutine(pointerRoutine);
    }


    private void OnDisable()
    {
        if (pointerRoutine != null) StopCoroutine(pointerRoutine);
        EventManager.StopListening(MyEvents.EVENT_CP_CAPTURED, OnPointCaptured);
    }
    IEnumerator pointerRoutine;
    IEnumerator PointCounter()
    {
       // Debug.Log("field finish count " + gameFieldCP.gameFieldFinished);
        while (!gameFieldCP.gameFieldFinished) {
            if (PhotonNetwork.IsMasterClient) {
                float amount = GetCP_Sum() * pointPerSec;
              //  Debug.Log("Set point " + amount);
                photonView.RPC("SetPoint", RpcTarget.AllBuffered, currentPoint + amount);
            }
            yield return new WaitForSeconds(1f);
        }
    }
    int GetCP_Sum() {
        int sum = 0;
        foreach (var cp in capturePoints) {
            if (cp.owner == Team.HOME)
            {
                sum++;
            }
            else if (cp.owner == Team.AWAY) {
                sum--;
            }
        }
        return sum;

    }
    [PunRPC]
    void SetPoint(float point) {
        currentPoint = point;
        if (!finishOnAllCapture)
        {
            if (currentPoint > endThreshold)
            {
                gameFieldCP.FinishGame(Team.HOME);
            }
            else if (currentPoint < -endThreshold)
            {
                gameFieldCP.FinishGame(Team.AWAY);
            }
        }
    }

    private void OnPointCaptured(EventObject arg0)
    {
        Debug.Log("Capture received");
        DefineOpenPoints();
        foreach (var cp in capturePoints) {
            cp.UpdateBanner();
        }
    }

    public void DefineOpenPoints()
    {
        int i = 0;
        while (i <= maxIndex) {
            if (mapDictionary[i].owner == Team.NONE
                || mapDictionary[i].owner == Team.AWAY)
            {
                homeNext = i;
                break;
            }
            else {
                i++;
            }
        }

        i = maxIndex;
        while (i >= 0)
        {
            if (mapDictionary[i].owner == Team.NONE
                || mapDictionary[i].owner == Team.HOME)
            {
                awayNext = i;
                break;
            }
            else
            {
                i--;
            }
        }
      
        if (homeNext > maxIndex)
        {
            homeNext = maxIndex;
            if (finishOnAllCapture) gameFieldCP.FinishGame(Team.HOME);
        }
        if (awayNext < 0)
        {
            awayNext = 0;
           if(finishOnAllCapture) gameFieldCP.FinishGame(Team.AWAY);
        }
    }
    void AdjustIterator() {
        int diff = (awayNext - homeNext);
        if (diff < -1)
        {
            awayNext++;
        }
        else if (diff > 1) {
            homeNext--;
        }
    }

    public bool IsValidCapturePoint(Team team, int index) {
        if (!serialCaptureRequired) return true;
        if (team == Team.HOME)
        {
            return homeNext == index;
        }
        else {
            return awayNext == index;
        }
    }

}
