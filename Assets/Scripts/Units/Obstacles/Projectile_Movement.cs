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
    public bool syncTransform = false;
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
    float delay_duration;
    PolygonCollider2D myColliderP;
    CircleCollider2D myColliderC;
    BoxCollider2D myColliderB;
    [SerializeField] SpriteRenderer mySprite;

    PhotonView pv;
    MapSpec mapSpec;
    HealthPoint hp;

    private void Awake()
    {
        myColliderP = GetComponent<PolygonCollider2D>();
        myColliderC = GetComponent<CircleCollider2D>();
        myColliderB = GetComponent<BoxCollider2D>();
        pv = GetComponent<PhotonView>();
        hp = GetComponent<HealthPoint>();
        if (syncTransform) {
            transSync = GetComponent<TransformSynchronisation>();
        }

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
    public void SetDelay(float delay) {
        delay_enableAfter = delay;
        EnableColliders(false);
    }
    [PunRPC]
    public void SetScale(float w, float h) {
        gameObject.transform.localScale = new Vector3(w, h, 1);
    }

    [PunRPC]
    public void DoTweenScale(float delay, float maxScale)
    {
        gameObject.transform.DOScale(new Vector3(maxScale, maxScale, 1), delay);
    }
    [PunRPC]
    public void SetDuration(float delay)
    {
        delay_duration = delay;
    }

    private void OnDisable()
    {
        delay_enableAfter = 0f;
        delay_duration = 0f;
        EnableColliders(true);
        mySprite.DORewind();
        gameObject.transform.DORewind();
    }

    private void Start()
    {
        if(delay_enableAfter > 0)
        {
            EnableColliders(false);
            StartCoroutine(WaitAndEnable());
            mySprite.DOFade(1f, delay_enableAfter);
        }
    }
    private void EnableColliders(bool enable) {
        if (myColliderP != null)
        {
            myColliderP.enabled = enable;
        }
        if (myColliderB != null)
        {
            myColliderB.enabled = enable;
        }
        if (myColliderC != null)
        {
            myColliderC.enabled = enable;
        }

    }
    private IEnumerator WaitAndEnable()
    {
        yield return new WaitForSeconds(delay_enableAfter);
        EnableColliders(true);
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
        if (mapSpec.IsOutOfBound(transform.position, 3f)) {
            hp.Kill_Immediate();
        };
    }

    private void DoMove_Straight()
    {
        float rad = eulerAngle / 180 * Mathf.PI ;
        float dX = Mathf.Cos(rad) * moveSpeed * Time.deltaTime;
        float dY = Mathf.Sin(rad) * moveSpeed * Time.deltaTime; 
        Vector3 moveDir = new Vector3(dX, dY);
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
        float rad = eulerAngle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * moveSpeed * Time.deltaTime;
        float dY = Mathf.Sin(rad) * moveSpeed * Time.deltaTime;
        Vector3 moveDir = new Vector3(dX, dY);
        ChangeTransform(moveDir, eulerAngle);
        
    }
    float orbitLength = 4f;
    float distanceMoved = 0;
   float orbitSpeed = 120f;
    private void DoMove_Orbit()
    {
        if (distanceMoved < orbitLength)
        {
            float rad = eulerAngle / 180 * Mathf.PI;
            float dX = Mathf.Cos(rad) * moveSpeed * Time.deltaTime * 2;
            float dY = Mathf.Sin(rad) * moveSpeed * Time.deltaTime * 2;
            Vector3 moveDir = new Vector3(dX, dY);
            distanceMoved += Vector2.Distance(moveDir, Vector3.zero);
            ChangeTransform(moveDir, eulerAngle);
        }
        else {
            eulerAngle += orbitSpeed * Time.deltaTime;
            float rad = eulerAngle / 180 * Mathf.PI;
            float dX = Mathf.Cos(rad) * orbitLength;
            float dY = Mathf.Sin(rad) * orbitLength;
            transform.localPosition = Vector3.zero;
            Vector3 moveDir = new Vector3(dX, dY);
            ChangeTransform(moveDir, eulerAngle);
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
        ChangeTransform(Vector3.zero, eulerAngle);
    }

    void ChangeTransform(Vector3 newDirection, float newEulerAngle) {
        if (syncTransform && pv.IsMine)
        {
            transSync.EnqueueLocalPosition(transSync.networkPos + newDirection, Quaternion.Euler(0, 0, newEulerAngle));
        }
        else
        {
            transform.localPosition += newDirection;
            transform.rotation = Quaternion.Euler(0, 0, newEulerAngle);
        }
    }
}

