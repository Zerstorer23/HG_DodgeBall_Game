using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSynchronisation : MonoBehaviourPunCallbacks
    , IPunObservable
{

    public double networkExpectedTime;
    public Vector3 networkPos;
    public Quaternion networkQuaternion;
    Queue<TimeVector> positionQueue = new Queue<TimeVector>();
    private double lastSendTime;

   public bool syncRotation = true;
    private void Start()
    {
        positionQueue = new Queue<TimeVector>();
        networkPos = transform.localPosition;
        networkQuaternion = transform.rotation;
    }
    private void Update()
    {
        DequeuePositions();
    }
    public void EnqueueLocalPosition(Vector3 newPosition, Quaternion newQuaternion)
    {
        networkPos = newPosition;
        networkQuaternion = newQuaternion;
        networkExpectedTime = PhotonNetwork.Time + GameSession.STANDARD_PING;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos, networkQuaternion));
        }
        else if (positionQueue.Count <= 0)
        {
            networkPos = transform.localPosition;
            networkQuaternion = transform.rotation;
            networkExpectedTime = PhotonNetwork.Time + GameSession.STANDARD_PING;
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
            transform.rotation = tv.quaternion;
        }

    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //통신을 보내는 
        if (stream.IsWriting)
        {
            if (networkExpectedTime != lastSendTime)
            {
                positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos));
            }
            stream.SendNext(networkPos);
            if (syncRotation) {

                stream.SendNext(networkQuaternion);
            }
            stream.SendNext(networkExpectedTime);
            lastSendTime = networkExpectedTime;
        }

        //클론이 통신을 받는 
        else
        {

            Vector3 position = (Vector3)stream.ReceiveNext();
            Quaternion rotation = Quaternion.identity;
            if (syncRotation)
            {
                rotation = (Quaternion)stream.ReceiveNext();
            }
            double netTime = (double)stream.ReceiveNext();
            TimeVector tv = new TimeVector(netTime, position,rotation);
            positionQueue.Enqueue(tv);
        }
    }
}
