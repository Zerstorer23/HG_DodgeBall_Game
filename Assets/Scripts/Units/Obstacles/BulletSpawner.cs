using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
using Random = UnityEngine.Random;

public class BulletSpawner : MonoBehaviourPun
{
    IEnumerator deleteRoutine;
    PhotonView pv;
   public Directions shootDir;
    [SerializeField] Transform spawnPos;
    public float rotationSpeed;
    public float angleClockBound;
    public float angleAntiClockBound;
    public float angleStack;
    public int goClockwise = 1;
    float delay;
    float delayStack;

    //Bulllet
    public SpawnDirection spawnDir;
    public MoveType moveType;
    private float blockWidth;
    float moveSpeed;
    float rotateSpeed;
    private bool isDead = false;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
    }
    private void OnEnable()
    {
        isDead = false;
    }
    private void OnGameEnd(EventObject arg0)
    {
        DoDeath();
    }

 

    [PunRPC]
    public void SetBehaviour(int shootDirection, float angleRange , float _delay, float duration)
    {
        SetAngles((Directions)shootDirection, angleRange);
        rotationSpeed = 25f;
        delay = _delay;

        DoTimer(duration);
    }
   
    private void DoTimer(float duration)
    {
        if (deleteRoutine != null)
            StopCoroutine(deleteRoutine);
        deleteRoutine = WaitAndDestroy(duration);
        StartCoroutine(deleteRoutine);
    }
    IEnumerator WaitAndDestroy(float duration)
    {
        yield return new WaitForSeconds(duration);
        DoDeath();
    }
    void DoDeath() {
        if (!pv.IsMine || isDead) return;
        isDead = true;
        EventManager.TriggerEvent(MyEvents.EVENT_SPAWNER_EXPIRE, null);
        try
        {
            PhotonNetwork.Destroy(pv);
        }
        catch (Exception e) {
            Debug.Log(e.StackTrace);
            
        }
    }
    void SetAngles(Directions shootDirection, float angleRange)
    {
        shootDir = shootDirection;
        float angle = BulletManager.DirectionsToEuler(shootDir);

        transform.eulerAngles = new Vector3(0, 0, angle);
        angleClockBound = angleRange;
        angleAntiClockBound = -angleRange;
    }

    [PunRPC]
    public void SetProjectile(int _spawnDir, int _moveType, float width, float _pMovespeed,float _pRotateSpeed)
    {
        spawnDir = (SpawnDirection)_spawnDir;
        moveType = (MoveType)_moveType;
        blockWidth = width;
        moveSpeed = _pMovespeed;
        rotateSpeed = _pRotateSpeed;


    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        DoRotation();
        CheckFire();
    }

    private void CheckFire()
    {
        delayStack += Time.deltaTime;
        if (delayStack >= delay) {
            delayStack -= delay;
                                                                                                        //
            GameObject obj = PhotonNetwork.InstantiateRoomObject(ConstantStrings.PREFAB_BULLET_1, spawnPos.position, transform.rotation,0);
            PhotonView pv = obj.GetComponent<PhotonView>();
            pv.RPC("SetMoveInformation", RpcTarget.AllBuffered, moveSpeed, rotateSpeed, angleClockBound);
            pv.RPC("SetScale",RpcTarget.AllBuffered, blockWidth, blockWidth);
            obj.GetComponent<PhotonView>().RPC("SetBehaviour", RpcTarget.AllBuffered, (int)moveType, transform.eulerAngles.z);
            pv.RPC("SetParentTransform", RpcTarget.AllBuffered);
        }
    }

    void DoRotation() {
        if (angleStack >= angleClockBound)
        {
            goClockwise = -1;
        }
        else if (angleStack <= -angleClockBound)
        {
            goClockwise = 1;
        }
        float amount = rotationSpeed * Time.deltaTime * goClockwise;
        angleStack += amount;
        float newAngle = transform.eulerAngles.z + amount;
      //  Debug.Log(transform.eulerAngles.z+" => newAngle" + newAngle);

        transform.eulerAngles = new Vector3(0, 0, newAngle);
    }

}

//생성구조


/*
 직선형
 곡선형
 경고후생성

 유도탄
 무유도탄
 정지 

타obj충돌시
 산탄
 바운스
 제거

 */
