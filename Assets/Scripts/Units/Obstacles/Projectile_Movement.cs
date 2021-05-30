using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
using static ConstantStrings;

public class Projectile_Movement : MonoBehaviourPun
{
    bool syncTransform = false;
    TransformSynchronisation transSync;

    public float eulerAngle;
    public float moveSpeed;

    //Rotation//

    public float rotateScale;
    public float angleClockBound;
    public float angleAntiClockBound;
    public float angleStack;
    public int goClockwise = 1;

    delegate void voidFunc();
    voidFunc DoMove;
    public MoveType moveType;
    public ReactionType reactionType = ReactionType.Bounce;

    internal void SetAssociatedField(int fieldNo)
    {
        mapSpec = GameFieldManager.gameFields[fieldNo].mapSpec;
    }

    //Delay Move//
    float delay_enableAfter = 0f;
    [SerializeField] SpriteRenderer mySprite;

    PhotonView pv;
    MapSpec mapSpec;
    HealthPoint hp;
    bool isMapObject = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        hp = GetComponent<HealthPoint>();
        transSync = GetComponent<TransformSynchronisation>();
        syncTransform = transSync != null;
        if(hp.damageDealer != null)
        isMapObject = hp.damageDealer.isMapObject;
    }

    private void OnEnable()
    {
        distanceMoved = 0;
    }

    [PunRPC]
    public void SetBehaviour(int _moveType, int _reaction, float _direction)
    {
        moveType = (MoveType)_moveType;
        reactionType = (ReactionType)_reaction;
        switch (moveType)
        {
            case MoveType.Static:
                DoMove = DoMove_Static;
                eulerAngle = transform.rotation.eulerAngles.z;
                break;
            case MoveType.Curves:
                DoMove = DoMove_Curve;
                eulerAngle = _direction;
                break;
            case MoveType.Straight:
                DoMove = DoMove_Straight;
                eulerAngle = _direction;// transform.rotation.eulerAngles.z;
                break;
            case MoveType.OrbitAround:
                DoMove = DoMove_Orbit;
                eulerAngle = _direction;
                break;
        }
    }


    [PunRPC]
    public void SetMoveInformation(float _speed,float _rotate,float rotateBound) {
        moveSpeed = _speed;
        rotateScale = _rotate;
        angleClockBound = rotateBound;
        angleAntiClockBound =  -rotateBound;
    }
    [PunRPC]
    public void SetScale(float w, float h) {
        gameObject.transform.localScale = new Vector3(w, h, 1);
    }
    public bool tweenEase = false;
    [PunRPC]
    public void DoTweenScale(float delay, float maxScale)
    {
        if (tweenEase)
        {

            gameObject.transform.DOScale(new Vector3(maxScale, maxScale, 1), delay).SetEase(Ease.InQuint);
        }
        else
        {
            gameObject.transform.DOScale(new Vector3(maxScale, maxScale, 1), delay);

        }
    }


    private void OnDisable()
    {
        delay_enableAfter = 0f;
        mySprite.DORewind();
        gameObject.transform.DORewind(); 
        synchedInitialCriticalPoint = false;
    }

    private void Update()
    {
        if (DoMove == null) return;
        if (delay_enableAfter > 0)
        {
            delay_enableAfter -= Time.deltaTime;
        }
        else {
            DoMove();
        }

    }
    private void FixedUpdate()
    {
        if (mapSpec.IsOutOfBound(transform.position, 6f)) {
            hp.Kill_Immediate();
        };
    }

    private void DoMove_Straight()
    {
        Vector3 moveDir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime);
        ChangeTransform(moveDir, eulerAngle);
    }
    private void DoMove_Curve()
    {
        if (angleStack >= angleClockBound)
        {
            goClockwise = -1;
        }
        else if (angleStack <= -angleClockBound)
        {
            goClockwise = 1;
        }
        float amount = rotateScale * Time.deltaTime * goClockwise;
        angleStack += amount;
        eulerAngle += amount;
        Vector3 moveDir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime);
        ChangeTransform(moveDir, eulerAngle);
        
    }
    float orbitLength = 4f;
    float distanceMoved = 0;
    float orbitSpeed = 120f;
    private void DoMove_Orbit()
    {
        if (distanceMoved < orbitLength)
        {
            Vector3 moveDir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime * 2);
            distanceMoved += Vector2.Distance(moveDir, Vector3.zero);
            ChangeTransform(moveDir, eulerAngle);
        }
        else {
            eulerAngle += orbitSpeed * Time.deltaTime;
            transform.localPosition = Vector3.zero;
            Vector3 moveDir = GetAngledVector(eulerAngle, orbitLength);
            ChangeTransform(moveDir, eulerAngle);
        }
    }

    public bool synchedInitialCriticalPoint = false;
   /* public void SyncAPoint(bool forceSynch = false) {
        if (synchedInitialCriticalPoint && !forceSynch) return;
        synchedInitialCriticalPoint = true;
        if (pv.IsMine) {
            pv.RPC("SyncAPoint_Helper", RpcTarget.AllBufferedViaServer, transform.localPosition, transform.rotation);
        }
    }
    [PunRPC]
    public void SyncAPoint_Helper(Vector3 position, Quaternion rotation)
    {
        transform.localPosition = position;
        transform.rotation = rotation;
    }*/
    private void DoMove_Static()
    {
        
    }

    public void Bounce2(ContactPoint2D contact, Vector3 contactPoint)
    {
        Vector3 normal = contact.normal;
        float rad = eulerAngle * Mathf.Deg2Rad;
        float dX = Mathf.Cos(rad) * moveSpeed * Time.deltaTime;
        float dY = Mathf.Sin(rad) * moveSpeed * Time.deltaTime;
        Vector3 velocity = new Vector3(dX, dY);
         velocity = Vector3.Reflect(velocity, normal);
        eulerAngle = (Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);// * boundFactor;
                                                       //   Debug.Log("Prev angle " + eulerAngle + " -> " + rawAngle + " collision angle " + velocity);
        ChangeTransform(Vector3.zero, eulerAngle);
    }

    void ChangeTransform(Vector3 newDirection, float newEulerAngle) {
        if (syncTransform )
        {
            if (pv.IsMine) {
                transSync.EnqueueLocalPosition(transSync.networkPos + newDirection, Quaternion.Euler(0, 0, newEulerAngle));
            }
        }
        else
        {
            transform.localPosition += newDirection;
            transform.rotation = Quaternion.Euler(0, 0, newEulerAngle);
        }
    }
   public void GetExpectedPosition(List<Vector3> collisionList, Vector3 collisionSource, float maxRange, float duration = 1f) {
        if (moveType == MoveType.Static) return ;
        float totalDistance = moveSpeed * duration;
        Vector3 endPosition = GetAngledVector(eulerAngle, totalDistance);
        float stepLength = GetRadius(transform.localScale);
        int steps = (int)(endPosition.magnitude / stepLength);
        Vector3 unitVector = GetAngledVector(eulerAngle, (totalDistance / steps));
        for (int i = 0; i < steps; i++) {
            Vector3 point = transform.position + unitVector * i;
            float collAngle = GetAngleBetween(point, collisionSource);
            point += GetAngledVector(collAngle, stepLength);
            //if (Vector2.Distance(point, collisionSource) > maxRange) continue;
           // Debug.Log
            collisionList.Add(point);
        }
        if ((stepLength * steps) < totalDistance)
        {
            float collAngle = GetAngleBetween(endPosition, collisionSource);
            Vector3 point = endPosition + GetAngledVector(collAngle, stepLength);
           // if (Vector2.Distance(point, collisionSource) > maxRange) return;
            collisionList.Add(point);
        } 
    }
}

