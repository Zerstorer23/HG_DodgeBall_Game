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
    public ReactionType reaction;
    float moveSpeed;
    float rotateSpeed;
    float blockWidth;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
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
        if (!PhotonNetwork.IsMasterClient) yield break;
        EventManager.TriggerEvent(MyEvents.EVENT_SPAWNER_EXPIRE, null);
        PhotonNetwork.Destroy(pv);
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
    public void SetProjectile(int _spawnDir, int _moveType, int _reaction, float width, float _pMovespeed,float _pRotateSpeed)
    {
        spawnDir = (SpawnDirection)_spawnDir;
        moveType = (MoveType)_moveType;
        reaction = (ReactionType)_reaction;
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
            GameObject obj = PhotonNetwork.InstantiateRoomObject(ConstantStrings.PREFAB_BULLET_1, spawnPos.position, transform.rotation);
            obj.GetComponent<PhotonView>().RPC("SetMoveInformation", RpcTarget.AllBuffered, blockWidth, moveSpeed, rotateSpeed, angleClockBound, transform.eulerAngles.z);

            obj.GetComponent<PhotonView>().RPC("SetBehaviour", RpcTarget.AllBuffered, (int)moveType, (int)reaction);
            obj.transform.SetParent(BulletManager.GetInstance().Home_Bullets);
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
