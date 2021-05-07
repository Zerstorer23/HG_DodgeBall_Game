using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
using static GameFieldManager;

public class Unit_Movement : MonoBehaviourPunCallbacks
    , IPunObservable
{
    public float moveSpeed = 8f;
    PhotonView pv;
    Vector3 lastVector = Vector3.up;
    float aimAngle = 0f;
    public Vector3 oldPosition;

    BuffManager buffManager;
    [SerializeField] internal GameObject directionIndicator;
    Transform networkPosIndicator;

    Queue<TimeVector> positionQueue = new Queue<TimeVector>();

    /*
      패킷은 모두 순서대로 온다
    상대가 계산한 위치 지점은 정확하다
    스무스는 정확하진않지만 비슷하다.
    standardping이 커질수록 받는오차가 적어짐
    해석오차는 update속도에 달려있음
     */
    string padXaxis = "RHorizontal";
    string padYaxis = "RVertical";

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        buffManager = GetComponent<BuffManager>();
        PhotonNetwork.SendRate = 60; //60 / 60 on update
        PhotonNetwork.SerializationRate = 60; // 32 32 on fixed

        if (UI_GamePadOptions.useGamepad) {
            SetAxisNames();
        }
        networkPosIndicator = GameSession.GetInst().networkPos;
    }

    void SetAxisNames() {
        switch (UI_GamePadOptions.padType)
        {
            case PadType.PS4:
                padXaxis = "RHorizontal";
                padYaxis = "RVertical";
                break;
            case PadType.XBOX:
                padXaxis = "RHorizontalXbox";
                padYaxis = "RVerticalXbox";
                break;
        }

    }
   internal Unit_AutoDrive autoDriver;

    private void Start()
    {
        if (pv.IsMine)
        {
            
            directionIndicator.SetActive(true);
            positionQueue = new Queue<TimeVector>();
            networkPos = transform.position;
        }
        //   StartCoroutine(UpdateWithSend(60));
    }
    // Update is called once per frame
    private void Update()
    {
        Move(Time.deltaTime);
        DequeuePositions();
        UpdateDirection();
    }
    void FixedUpdate()
    {
      // Move(Time.fixedDeltaTime);
    }
    //0 1 2 3 4 5 = 5
    // [12] [345]
    //0   2     5
    // 1

    private void Move(float delta)
    {
        float moveSpeedFinal = moveSpeed * buffManager.GetBuff(BuffType.MoveSpeed) * delta;

        if (pv.IsMine)
        {
            if (MenuManager.auto_drive)
            {
                GiveEvaluatedInput(moveSpeedFinal);
            }
            else
            {
                MoveByInput(moveSpeedFinal);
            }
        }
        else
        {

        }


    }

    double nextRandomTIme;
    Vector3 lastRandomMove = Vector3.zero;
    private void GiveRandomInput(float moveSpeedFinal)
    {

        var deltaX = lastRandomMove.x;
        var deltaY = lastRandomMove.y;
        if (networkPos.x >= xMax
            || networkPos.x <= xMin
            || networkPos.y >= yMax
            || networkPos.y <= yMin
            ) nextRandomTIme = PhotonNetwork.Time;

        if (PhotonNetwork.Time >= nextRandomTIme)
        {
            deltaX = Random.Range(-1, 2) * moveSpeedFinal;
            deltaY = Random.Range(-1, 2) * moveSpeedFinal;
            lastRandomMove = new Vector3(deltaX, deltaY, 0f);
            double time = Random.Range(0f, 1.5f);
            nextRandomTIme = PhotonNetwork.Time + time;
        }
        Vector3 newPosition = ClampPosition(new Vector3(networkPos.x + deltaX, networkPos.y + deltaY, 0f));
        // 32 16
        // 1.. 32   2. 16
        // 33 / 34 / 18
        EnqueuePosition(newPosition);
    }
    private void GiveEvaluatedInput(float moveSpeedFinal)
    {

        Vector3 recommendDirection = autoDriver.EvaluateMoves() * moveSpeedFinal; 
        Vector3 newPosition = ClampPosition(new Vector3(networkPos.x + recommendDirection.x, networkPos.y + recommendDirection.y, 0f));
        // 32 16
        // 1.. 32   2. 16
        // 33 / 34 / 18
        EnqueuePosition(newPosition);
    }

    private void MoveByInput(float moveSpeedFinal)
    {
        var deltaX = Input.GetAxis("Horizontal") * moveSpeedFinal;
        var deltaY = Input.GetAxis("Vertical") * moveSpeedFinal;

        Vector3 newPosition = ClampPosition(new Vector3(networkPos.x +  deltaX, networkPos.y + deltaY, 0f));
        EnqueuePosition(newPosition);

    }
    void EnqueuePosition(Vector3 newPosition)
    {
        enqueueCount++;
        if (newPosition != oldPosition)
        {
            lastVector = newPosition - oldPosition;
            networkPos = newPosition;
            networkExpectedTime = PhotonNetwork.Time + GameSession.STANDARD_PING;
            oldPosition = newPosition;
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            positionQueue.Enqueue(new TimeVector(networkExpectedTime, networkPos));
          }
        }
        else if (positionQueue.Count <= 0)
        {
            networkPos = transform.position;
            networkExpectedTime = PhotonNetwork.Time + GameSession.STANDARD_PING;
        }
        networkPosIndicator.position = newPosition;
    }
    Vector3 ClampPosition(Vector3 position) {
        float newX =  Mathf.Clamp(position.x, xMin, xMax);
        float newY = Mathf.Clamp(position.y, xMin, xMax);
        return new Vector3(newX, newY);
    }

    void DequeuePositions()
    {

        TimeVector tv = null;
        int skip = 0;
        while (positionQueue.Count > 0 && positionQueue.Peek().IsExpired())
        {
            tv = positionQueue.Dequeue();
            skip++;
        }
        if (tv != null) {
            transform.position = tv.position;
          //  lastDequeueTime = PhotonNetwork.Time;
           // lastDequeuedPosition = tv.position;
        }

    }


    float indicatorLength = 1f;
    void UpdateDirection()
    {
        if (UI_AimOption.aimManual)
        {
            if (UI_GamePadOptions.useGamepad) { 
                var deltaX = Input.GetAxis(padXaxis) ;
                var deltaY = Input.GetAxis(padYaxis) ;
                if (Mathf.Abs(deltaX) <Mathf.Epsilon && Mathf.Abs(deltaY) < Mathf.Epsilon) return;
                Vector3 aimDir = new Vector3(deltaX, deltaY, 0f);
                aimAngle = GameSession.GetAngle(Vector3.zero, aimDir); //벡터 곱 비교
            }
            else {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                aimAngle = GameSession.GetAngle(gameObject.transform.position, mousePos); //벡터 곱 비교
            }
        }
        else {
            aimAngle = GameSession.GetAngle(Vector3.zero, lastVector); //벡터 곱 비교
        }
        float rad = aimAngle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * indicatorLength;
        float dY = Mathf.Sin(rad) * indicatorLength;
        directionIndicator.transform.localPosition = new Vector3(dX, dY);
        directionIndicator.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
    }

    public float GetAim()
    {
        return aimAngle;
    }
    public double networkExpectedTime;
    public Vector3 networkPos;


    public uint receivedCount = 0;
    public uint sentCount = 0;
    public uint enqueueCount = 0;

    public double lastSendTime;

    // private Quaternion currRot;
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
            stream.SendNext(networkExpectedTime);
            lastSendTime = networkExpectedTime;
            sentCount++;
        }

        //클론이 통신을 받는 
        else
        {
            //tcp
            //udp
            Vector3 position = (Vector3)stream.ReceiveNext();
            double netTime = (double)stream.ReceiveNext();
            TimeVector tv = new TimeVector(netTime, position);
            positionQueue.Enqueue(tv);
            receivedCount++;
         //   Debug.Log(receivedCount + " 받음 " + tv.ToString());
            //만료되기 전에 받아야함
            //미리 쌓아놔야 스무싱이 가능하고 못쌓은건 텔레포트해야함
        }
    }
}


public class TimeVector
{
    public double timestamp;
    public Vector3 position;
    public Quaternion quaternion;
    public TimeVector(double t, Vector3 v)
    {
        this.timestamp = t;
        this.position = v;
    }
    public TimeVector(double t, Vector3 v, Quaternion q)
    {
        this.timestamp = t;
        this.position = v;
        this.quaternion = q;
    }
    public bool IsExpired()
    {
        return (timestamp <= PhotonNetwork.Time);
    }
    public override string ToString() {
        return timestamp + " : " + position;
    }

}