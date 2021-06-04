using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSynchronisation : MonoBehaviourPunCallbacks
    , IPunObservable
{

    public double networkExpectedTime;
    public Vector3 networkPos;//ONLY USE FOR PV MINE TODO
    public Quaternion networkQuaternion;
    Queue<TimeVector> positionQueue = new Queue<TimeVector>();
    private double lastSyncTIme;

    public bool syncRotation = true;
    public SyncType syncType = SyncType.Always;
    public double syncPeriod = -1;

    public delegate bool boolFunc();
    boolFunc CheckSyncCondition;
    boolFunc CheckSyncLocal;
    private void Awake()
    {
        switch (syncType)
        {
            case SyncType.Timed:
                CheckSyncCondition = TimedSyncCheck;
                CheckSyncLocal = TimedSyncLocal;
                break;
            case SyncType.Always:
                CheckSyncCondition = AlwaysSyncCheck;
                CheckSyncLocal = AlwaysSyncLocal;
                syncPeriod = 0;
                break;
        }
    }
    private void Start()
    {
        positionQueue = new Queue<TimeVector>();
        networkPos = transform.localPosition;
        networkQuaternion = transform.rotation;

    }

    bool AlwaysSyncCheck()
    {
        return photonView.IsMine;
    }
    bool AlwaysSyncLocal()
    {
        return false;
    }
    bool TimedSyncCheck()
    {
        if (photonView.IsMine && PhotonNetwork.Time > (lastSyncTIme + syncPeriod)) {
                return true;
        }
        else
        {
            return false;
        }
    }
    bool TimedSyncLocal()
    {
        return true;
    }
    private void Update()
    {
        DequeuePositions();
    }
    public void EnqueueLocalPosition(Vector3 newPosition, Quaternion newQuaternion)
    {
        bool needSync = CheckSyncCondition();
        networkPos = newPosition;
        networkQuaternion = newQuaternion;
        networkExpectedTime = PhotonNetwork.Time + GameSession.STANDARD_PING;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1 || CheckSyncLocal())
        {
            positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos, networkQuaternion));
            return;
        }
    }
 
    void DequeuePositions()
    {

        TimeVector tv = null;
        while (positionQueue.Count > 0 && positionQueue.Peek().IsExpired())
        {
            tv = positionQueue.Dequeue();
        }
        if (tv != null)
        {
            transform.localPosition = tv.position;
            if (syncRotation)
            {
                transform.rotation = tv.quaternion;
            }

        }
/*        else {
            //nothing to dequeue, update my position
            EnqueueLocalPosition(transform.localPosition, transform.rotation);
        }*/

    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        bool needSync = CheckSyncCondition();
        //통신을 보내는 
        if (stream.IsWriting)
        {
            if (!needSync) return;
            if (networkExpectedTime != lastSyncTIme)
            {
                positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos, networkQuaternion));
            }
            stream.SendNext(networkPos);
            if (syncRotation) {
                stream.SendNext(networkQuaternion);
            }
            stream.SendNext(networkExpectedTime);
            lastSyncTIme = networkExpectedTime;
        }

        //클론이 통신을 받는 
        else
        {
            var position = (Vector3)stream.ReceiveNext();
            var quaternion = transform.rotation;
            if (syncRotation)
            {
                quaternion = (Quaternion)stream.ReceiveNext();
            }
            double netTime = (double)stream.ReceiveNext();
            TimeVector tv = new TimeVector(netTime, position, quaternion);
            if (syncType == SyncType.Timed) {
                InvalidateAfter(netTime);
            }
            positionQueue.Enqueue(tv);
        }
    }

    public void InvalidateAfter(double netTime) {
        int invalidated = 0;
        while(positionQueue.Count != 0 && positionQueue.Peek().timestamp >= netTime){
            positionQueue.Dequeue();
            invalidated++;
        }
   /*     if (invalidated > 0) {
            Debug.LogWarning("INvalidated " + invalidated);
        }*/
    }
}
public enum SyncType { 
    Timed,Always
}