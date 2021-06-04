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


    TransformSynchronisation netTransform;
    bool transSync;

    Projectile_Homing projectileHoming;
    bool isHoming = false;

    public float eulerAngle;
    public float moveSpeed;

    //Rotation//

    public float rotateScale;
    public float angleClockBound;
    public float angleAntiClockBound;
    public float angleStack;
    public int goClockwise = 1;
    delegate void voidFunc();
    delegate Vector3 vectorFunc();
    delegate Quaternion quartFunc();
    voidFunc DoMove;
    vectorFunc GetPosition;
    quartFunc GetQuarternion;
    public MoveType moveType;
    public ReactionType reactionType = ReactionType.Bounce;
    public CharacterType characterUser = CharacterType.NONE;
    internal void SetAssociatedField(int fieldNo)
    {
        mapSpec = GameFieldManager.gameFields[fieldNo].mapSpec;
    }

    //Delay Move//
    float delay_enableAfter = 0f;
    [SerializeField] SpriteRenderer mySprite;
    MapSpec mapSpec;
    HealthPoint hp;
    bool isMapObject = false;

    private void Awake()
    {
        hp = GetComponent<HealthPoint>();
        netTransform = GetComponent<TransformSynchronisation>();
        transSync = netTransform != null;
        projectileHoming = GetComponent<Projectile_Homing>();
        isHoming = projectileHoming != null;
        transSync = netTransform != null;
        if (hp.damageDealer != null)
        isMapObject = hp.damageDealer.isMapObject;
        if (transSync)
        {
            GetPosition = GetNetworkPosition;
            GetQuarternion = GetNetworkQuarternion;
        }
        else
        {
            GetPosition = GetMyPosition;
            GetQuarternion = GetMyQuarternion;

        }
    }
    Vector3 GetMyPosition() => transform.localPosition;
    Quaternion GetMyQuarternion() => transform.rotation;
    Vector3 GetNetworkPosition() => netTransform.networkPos;
    Quaternion GetNetworkQuarternion() => netTransform.networkQuaternion;

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
                eulerAngle = GetQuarternion().eulerAngles.z;
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
        ChangeTransform(moveDir);
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
        ChangeTransform(moveDir);
        
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
            ChangeTransform(moveDir);
        }
        else
        {
            eulerAngle += orbitSpeed * Time.deltaTime;
            if (transSync)
            {
                netTransform.networkPos = Vector3.zero;
            }
            else
            {
                transform.localPosition = Vector3.zero;
            }
            Vector3 moveDir = GetAngledVector(eulerAngle, orbitLength);
            ChangeTransform(moveDir);
        }
    }


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
        ChangeTransform(Vector3.zero);
    }

    void ChangeTransform(Vector3 newDirection)
    {
        if (eulerAngle < 0) eulerAngle += 360;
        if (eulerAngle > 360) eulerAngle %= 360;
        if (isHoming) {
            eulerAngle = projectileHoming.AdjustDirection(GetPosition(),eulerAngle);
            newDirection = GetAngledVector(eulerAngle, newDirection.magnitude);
        }
        if (transSync)
        {
            netTransform.EnqueueLocalPosition(GetPosition() + newDirection, Quaternion.Euler(0, 0, eulerAngle));

        }
        else {
            transform.localPosition = GetPosition() + newDirection;
            transform.rotation = Quaternion.Euler(0, 0, eulerAngle);
        }
    }
    public Vector3 GetNextPosition() {
        Vector3 dir = Vector3.zero;
        switch (moveType)
        {
            case MoveType.Curves:
                float amount = rotateScale * Time.deltaTime * goClockwise;
                dir = GetAngledVector(eulerAngle + amount, moveSpeed * Time.deltaTime);
                break;
            case MoveType.Straight:
                dir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime); 
                break;
            case MoveType.OrbitAround:
                if (distanceMoved < orbitLength)
                {
                    dir = GetAngledVector(eulerAngle, moveSpeed * Time.deltaTime * 2);
                }
                else
                {
                    dir = GetAngledVector(eulerAngle + orbitSpeed * Time.deltaTime, orbitLength);
                }
                break;
        }
        return GetPosition() + dir;
    }

}

