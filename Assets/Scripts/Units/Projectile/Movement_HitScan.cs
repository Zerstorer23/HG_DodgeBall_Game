using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class Movement_HitScan : 
//  MonoBehaviour
//MonoBehaviourPunCallbacks, IPunObservable
MonoBehaviourPun
{
    bool netSync = false;
    float moveSpeed = 600f;
    Rigidbody2D myRigidBody;
    CircleCollider2D myCollider;
    Projectile_Movement pMove;

    Queue<VelocityVector> velocityQueue = new Queue<VelocityVector>();
    private void Awake()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        pMove = GetComponent<Projectile_Movement>();
        myCollider = GetComponent<CircleCollider2D>();

    }
    private void OnEnable()
    {
        velocityQueue.Clear();
     //  networkExpectedTime =(double) photonView.InstantiationData[3];
    }


    private void Update()
    {
      // if (PhotonNetwork.Time < networkExpectedTime) return;
        if (netSync)
        {

            pMove.moveSpeed = moveSpeed;
            myRigidBody.velocity = GetAngledVector(pMove.eulerAngle, moveSpeed);
            EnqueuePosition(transform.position);
            DequeuePositions();
        }
        else
        {

            pMove.moveSpeed = moveSpeed;
            myRigidBody.velocity = GetAngledVector(pMove.eulerAngle, moveSpeed);
        }
    }
    public Vector3 networkVelocity;
    public double networkExpectedTime;
    void EnqueuePosition(Vector3 newVelocity)
    {
        networkVelocity = newVelocity;
        networkExpectedTime = PhotonNetwork.Time + GameSession.STANDARD_PING;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            velocityQueue.Enqueue(new VelocityVector(networkExpectedTime, networkVelocity));
        }
    }

    void DequeuePositions()
    {
        VelocityVector tv = null;
        while (velocityQueue.Count > 0 && velocityQueue.Peek().IsExpired())
        {
            tv = velocityQueue.Dequeue();
        }
        if (tv != null)
        {
            //myRigidBody.velocity = tv.velocity;
            transform.position = tv.velocity;
        }
        pMove.moveSpeed = moveSpeed;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        // Debug.Log(gameObject.name + "Collision with " + collision.gameObject.name + " / tag " + tag);
        switch (tag)
        {
            case TAG_PLAYER:
            case TAG_PROJECTILE:
                myCollider.isTrigger = true;
             //   StartCoroutine(TurnOffTrigger());
                break;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        // Debug.Log(gameObject.name + "Collision with " + collision.gameObject.name + " / tag " + tag);
        switch (tag)
        {
            case TAG_PLAYER:
            case TAG_PROJECTILE:
                myCollider.isTrigger = false;
                break;
        }
    }
    IEnumerator TurnOffTrigger()
    {
        yield return new WaitForSeconds(0.5f);
        myCollider.isTrigger = false;
    }
    double lastSendTime;
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    { //통신을 보내는 
        if (stream.IsWriting)
        {
            //   if (networkExpectedTime != lastSendTime)
            //   {
            Debug.Log("Sending " + velocityQueue.Count);
            velocityQueue.Enqueue(new VelocityVector(networkExpectedTime, networkVelocity));
            //  }
            stream.SendNext(networkVelocity);
            stream.SendNext(networkExpectedTime);
            // lastSendTime = networkExpectedTime;
        }

        //클론이 통신을 받는 
        else
        {
            //tcp
            //udp
            Debug.Log("Receiving " + velocityQueue.Count);
            Vector3 velocity = (Vector3)stream.ReceiveNext();
            double netTime = (double)stream.ReceiveNext();
            VelocityVector tv = new VelocityVector(netTime, velocity);
            velocityQueue.Enqueue(tv);
        }
    }
}

public class VelocityVector
{
    public double timestamp;
    public Vector3 velocity;
    public VelocityVector(double t, Vector3 v)
    {
        this.timestamp = t;
        this.velocity = v;
    }

    public bool IsExpired()
    {
        return (timestamp <= PhotonNetwork.Time);
    }
    public override string ToString()
    {
        return timestamp + " : " + velocity;
    }

}