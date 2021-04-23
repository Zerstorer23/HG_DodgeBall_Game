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
    public ReactionType reaction;

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
    public void SetBehaviour(int _moveType, int _reaction)
    {
        moveType = (MoveType)_moveType;
        reaction = (ReactionType)_reaction;

        switch (moveType)
        {
            case MoveType.Static:
                DoMove = DoMove_Static;
                break;
            case MoveType.Curves:
                DoMove = DoMove_Curve;
                break;
            case MoveType.Straight:
                DoMove = DoMove_Straight;
                break;
        }
    }
    [PunRPC]
    public void SetMoveInformation(float blockWidth, float _speed,float _rotate,float rotateBound, float _direction) {
        eulerAngle = _direction;
        moveSpeed = _speed;
        rotateScale = _rotate;
        angleClockBound = rotateBound;
        angleAntiClockBound =  -rotateBound;
        transform.localScale = new Vector3(blockWidth, blockWidth, 1);
    }
    [PunRPC]
    public void SetDelay(float delay) {
        delay_enableAfter = delay;
        myCollider.enabled = false;
    }


    float targetScale;
    [PunRPC]
    public void SetGradualScale(float delay, float maxScale)
    {
        delay_duration = delay;
        targetScale = maxScale;
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
        targetScale = 0f;
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
        if (delay_duration > 0) {
            StartCoroutine(WaitAndKill());
        }
        if (targetScale > 0)
        {
            gameObject.transform.DOScale(new Vector3(targetScale, targetScale, 1), delay_duration);
        }
    }

    private IEnumerator WaitAndEnable()
    {
        yield return new WaitForSeconds(delay_enableAfter);
        myCollider.enabled = true;
    }
    private IEnumerator WaitAndKill()
    {
        yield return new WaitForSeconds(delay_duration);
        GetComponent<HealthPoint>().DoDamage(true);
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
        float rad = eulerAngle/180 * Mathf.PI ;
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
  [Range(-15f,15f)]  public float boundFactor = 2f;
    public void Bounce(ContactPoint2D contact, Vector3 contactPoint) {

        float rad = eulerAngle * Mathf.Deg2Rad;
        float dX = Mathf.Cos(rad) * moveSpeed * Time.deltaTime;
        float dY = Mathf.Sin(rad) * moveSpeed * Time.deltaTime;
        Vector3 velocity = new Vector3(dX, dY);

        //Find the BOUNCE of the object
        velocity = 2 * (Vector3.Dot(velocity, Vector3.Normalize(contact.normal))) * Vector3.Normalize(contact.normal) - velocity; //Following formula  v' = 2 * (v . n) * n - v

        velocity *= -1; //Had to multiply everything by -1. Don't know why, but it was all backwards.
        transform.position += velocity;
        Vector3 v = contactPoint - transform.position;
        float rawAngle = (Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg);// * boundFactor;
        eulerAngle = rawAngle;
        angleStack = 0;
    }

}
public enum ProjectileSkill { 
    Normal, Nagato, Haruhi

}

