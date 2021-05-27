using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Movement : MonoBehaviourPunCallbacks
    , IPunObservable
{
    public float moveSpeed = 8f;
    PhotonView pv;
    Vector3 lastVector = Vector3.up;
    float aimAngle = 0f;
    public Vector3 oldPosition;

    BuffManager buffManager;
    Unit_Player unitPlayer;
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
    public MapSpec mapSpec;


    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        buffManager = GetComponent<BuffManager>();
        unitPlayer = GetComponent<Unit_Player>();
        PhotonNetwork.SendRate = 60; //60 / 60 on update
        PhotonNetwork.SerializationRate = 60; // 32 32 on fixed

        networkPosIndicator = GameSession.GetInst().networkPos;
    }
    private new void OnEnable()
    {
        if (UI_GamePadOptions.useGamepad)
        {
            InputHelper.SetAxisNames();
        }
    }


    public void SetMapSpec(MapSpec spec) {
        mapSpec = spec;
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
    }
    // Update is called once per frame
    private void Update()
    {
        Move(Time.deltaTime);
        DequeuePositions();
        UpdateDirection();
    }

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
        if (networkPos.x >= mapSpec.xMax
            || networkPos.x <= mapSpec.xMin
            || networkPos.y >= mapSpec.yMax
            || networkPos.y <= mapSpec.yMin
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
        var deltaX = InputHelper.GetInputHorizontal() * moveSpeedFinal;
        var deltaY = InputHelper.GetInputVertical() * moveSpeedFinal;

        Vector3 newPosition = ClampPosition(new Vector3(networkPos.x +  deltaX, networkPos.y + deltaY, 0f));
        EnqueuePosition(newPosition);

    }

    void EnqueuePosition(Vector3 newPosition)
    {
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
        float newX =  Mathf.Clamp(position.x, mapSpec.xMin, mapSpec.xMax);
        float newY = Mathf.Clamp(position.y, mapSpec.yMin, mapSpec.yMax);
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
        if (tv != null)
        {
            FlipBody( tv.position.x - transform.position.x);
            transform.position = tv.position;
            //  lastDequeueTime = PhotonNetwork.Time;
            // lastDequeuedPosition = tv.position;
        }
        else {
            transform.position = ClampPosition(transform.position);
        }

    }
    void FlipBody(float xDelta) {
        Vector3 localScale = unitPlayer.charBody.transform.localScale;
        float x = localScale.x;
        if (
            (xDelta < 0 && x > 0)
            ||
             (xDelta > 0 && x < 0)
            )
        {
            unitPlayer.charBody.transform.localScale = new Vector3(-localScale.x, localScale.y, localScale.z);
        }
    }

    float indicatorLength = 1f;
    void UpdateDirection()
    {
        if (UI_AimOption.aimManual)
        {
            if (UI_GamePadOptions.useGamepad) { 
                var deltaX = Input.GetAxis(InputHelper.padXaxis) ;
                var deltaY = Input.GetAxis(InputHelper.padYaxis) ;
                if (Mathf.Abs(deltaX) <Mathf.Epsilon && Mathf.Abs(deltaY) < Mathf.Epsilon) return;
                Vector3 aimDir = new Vector3(deltaX, deltaY, 0f);
                aimAngle = GameSession.GetAngle(Vector3.zero, aimDir); //벡터 곱 비교
            }
            else {
                Vector3 target = InputHelper.GetTargetVector();
                aimAngle = GameSession.GetAngle(gameObject.transform.position, target); //벡터 곱 비교
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