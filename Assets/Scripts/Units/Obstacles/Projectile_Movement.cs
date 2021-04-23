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

    //Delay Move//
    float delay_enableAfter = 0f;
    float delay_duration;
    BoxCollider2D myCollider;
    [SerializeField] SpriteRenderer mySprite;

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
    }


    [PunRPC]
    public void SetBehaviour(int _moveType, float _direction)
    {
        moveType = (MoveType)_moveType;
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
        myCollider.enabled = false;
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
        myCollider.enabled = true;
        mySprite.DORewind();
        gameObject.transform.DORewind();
    }

    private void Start()
    {
        if(delay_enableAfter > 0)
        {
            myCollider.enabled = false;
            StartCoroutine(WaitAndEnable());
            mySprite.DOFade(1f, delay_enableAfter);
        }
    }

    private IEnumerator WaitAndEnable()
    {
        yield return new WaitForSeconds(delay_enableAfter);
        myCollider.enabled = true;
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

    private void DoMove_Straight()
    {
        float rad = eulerAngle / 180 * Mathf.PI ;
        float dX = Mathf.Cos(rad) * moveSpeed * Time.deltaTime;
        float dY = Mathf.Sin(rad) * moveSpeed * Time.deltaTime; 
        Vector3 moveDir = new Vector3(dX, dY);
     //   Debug.Log("Move to " + moveDir + "rad"+rad+" cos"+ Mathf.Cos(rad)+" dx "+dX);
     //   Debug.Log("Move to " + moveDir + "rad"+rad+ " Sin" + Mathf.Sin(rad)+" dy "+dY);
        transform.localPosition += moveDir;
       // transform.Translate(moveDir);
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
        transform.localPosition += moveDir;
        transform.eulerAngles = new Vector3(0, 0, eulerAngle);
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
        float rawAngle = (Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);// * boundFactor;
     //   Debug.Log("Prev angle " + eulerAngle + " -> " + rawAngle + " collision angle " + velocity);
        eulerAngle = rawAngle;
        transform.rotation = Quaternion.Euler(0, 0, eulerAngle);

    }
}

