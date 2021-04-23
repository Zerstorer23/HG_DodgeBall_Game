using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static GameSession;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BulletManager : MonoBehaviourPun
{
    public Transform Home_Bullets;
    private static BulletManager instance;
    public bool isMaster = false;

    public int activeMax = 30;
    public int currentSpawned = 0;
    [Header("Spawner setting")]
    public float minDelay, maxDelay;
    public float minDuration, maxDuration;

    [Header("Projectile settings")]
    public float maxProjSpeed = 15f;
    public float maxProjRotateScale = 180f;
    [Header("Box settings")]
    public float maxWidth = 10f;
    public float spawnDelay = 3f;

    PhotonView pv;

    private void Awake()
    {

        pv = GetComponent<PhotonView>();
        instance = this;
        EventManager.StartListening(MyEvents.EVENT_SPAWNER_EXPIRE, OnSpawnerExpired);

    
    }
    private void Start()
    {
        Hashtable hash = PhotonNetwork.CurrentRoom.CustomProperties;
        activeMax = (int)hash[ConstantStrings.HASH_MAP_DIFF];
    }
    public static BulletManager GetInstance() {
        return instance;
    }


    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_SPAWNER_EXPIRE, OnSpawnerExpired);
    }

    // Update is called once per frame
    void Update()
    {
        // if (!PhotonNetwork.IsConnectedAndReady) return;
        isMaster = PhotonNetwork.IsMasterClient;
        if (!isMaster) return;
        CheckSpawnerSpawns();

    }

    private void CheckSpawnerSpawns()
    {
        if (currentSpawned >= activeMax) return;
        while (currentSpawned < activeMax)
        {
            SpawnDirection spawnDir = GetRandomSpawnDir();
            MoveType moveType = GetRandomMoveType(spawnDir);
            ReactionType reaction = GetRandomReaction(spawnDir);
            InstantiateSpanwer(spawnDir, moveType, reaction);
            pv.RPC("IncrementSpawned", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    public void IncrementSpawned() {
        currentSpawned++;
    }
    [PunRPC]
    public void DecrementSpawned()
    {
        currentSpawned--;
    }

    private void OnSpawnerExpired(EventObject eo = null)
    {
        pv.RPC("DecrementSpawned", RpcTarget.AllBuffered);
    }
    private void InstantiateSpanwer(SpawnDirection spawnDir, MoveType moveType, ReactionType reaction)
    {
        //Projectile
        switch ((MoveType)moveType)
        {
            case MoveType.Static:
                InstantiateBox();
                break;
            case MoveType.Curves:
            case MoveType.Straight:
                Vector3 randPos = GetRandomBoundaryPos();
                GameObject spawner = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BulletSpawner", randPos, Quaternion.identity);
                SetProjectileInformation(spawner, spawnDir, moveType, reaction);
                SetProjectileBehaviour(spawner, randPos);
                spawner.transform.SetParent(transform);
                break;
        }



    }

    private void InstantiateBox()
    {
        float randW = Random.Range(1f, maxWidth);
        Vector3 randPos = GameSession.GetRandomPosOnMap();

        GameObject box = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BoxObstacle", randPos, Quaternion.identity);
        box.GetComponent<PhotonView>().RPC("SetInformation", RpcTarget.AllBuffered, randW,spawnDelay);
        box.transform.SetParent(transform);
    }

    void SetProjectileInformation(GameObject spawner, SpawnDirection spawnDir, MoveType moveType, ReactionType reaction)
    {
        float moveSpeed = Random.Range(5f, maxProjSpeed);
        float rotateSpeed = Random.Range(5f, maxProjRotateScale);
        float blockSize = Random.Range(0.25f, 1f);
        spawner.GetComponent<PhotonView>().RPC("SetProjectile", RpcTarget.AllBuffered, (int)spawnDir, (int)moveType, (int)reaction, blockSize, moveSpeed, rotateSpeed);
    }
    void SetProjectileBehaviour(GameObject spawner, Vector3 randPos)
    {
        //Behaviour
        int headingDirection = (int)GetHeadingAngle(randPos);
        float angleRange;
        if (headingDirection <= 3)
        {
            angleRange = Random.Range(0f, 40f);
        }
        else
        {
            angleRange = Random.Range(0f, 20f);
        }

        float delay = Random.Range(minDelay, maxDelay);
        float duration = Random.Range(minDuration, maxDuration);
        spawner.GetComponent<PhotonView>().RPC("SetBehaviour", RpcTarget.AllBuffered, headingDirection, angleRange, delay, duration);
    }

    private Directions GetHeadingAngle(Vector3 randPos)
    {
        if (randPos.x == xMin)
        {
            if (randPos.y == yMin)
            {
                return Directions.NE;

            }
            else if (randPos.y == yMax)
            {
                return Directions.SE;
            }
            else
            {
                return Directions.E;
            }
        }
        else if (randPos.x == xMax)
        {
            if (randPos.y == yMin)
            {
                //shoot 45'
                return Directions.NW;

            }
            else if (randPos.y == yMax)
            {
                return Directions.SW;
            }
            else
            {
                return Directions.W;
            }

        }
        else
        {
            if (randPos.y == yMin)
            {
                return Directions.N;

            }
            else
            {
                return Directions.S;
            }
        }
    }

    private Vector3 GetRandomBoundaryPos()
    {
        Vector3 randPos = GameSession.GetRandomPosOnMap();
        float randX = randPos.x;
        float randY = randPos.y;
        bool xClamp = Random.Range(0f, 1f) < 0.5f;
        if (xClamp)
        {
            randX = (randX < xMid) ? xMin : xMax;
        }
        else
        {
            randY = (randY < yMid) ? yMin : yMax;
        }
        return new Vector3(randX, randY);// new Vector3(randX, randY);
    }

    private ReactionType GetRandomReaction(SpawnDirection spawnDir)
    {
        if (spawnDir == SpawnDirection.Preemptive)
            return ReactionType.None;
        return (ReactionType)Random.Range(1, 4);
    }

    private MoveType GetRandomMoveType(SpawnDirection spawnDir)
    {
        if (spawnDir == SpawnDirection.Preemptive)
            return MoveType.Static;
        return (MoveType)Random.Range(1, 3);
    }

    private SpawnDirection GetRandomSpawnDir()
    {
            return (SpawnDirection)Random.Range(0, 3);
    }

    public enum SpawnDirection
    {
        Preemptive, Straight, Spiral, None
    }

    //이동방식
    public enum MoveType
    {
        Static, Curves, Straight
    }
    //다른 bullet과 충돌
    public enum ReactionType
    {
        None, Shatter, Bounce, Die
    }
    public enum Directions
    {
        W=0, E =1, N=2, S=3,  NW, NE, SW, SE,
    }
    public static float DirectionsToEuler(Directions dir)
    {
        switch (dir)
        {
            case Directions.N:
                return 90f;
            case Directions.S:
                return 270f;
            case Directions.W:
                return 180f;// 180f / 2f;
            case Directions.E:
                return 0f;
            case Directions.SW:
                return 225f;
            case Directions.SE:
                return 315f;
            case Directions.NW:
                return 135f;
            case Directions.NE:
                return 45f;
            default:
                Debug.LogWarning("Wrong cardinal");
                return 0f;
        }

    }

    /*
     bullet spawner 생성 - photoninstan
     info 주입 -master rpc

    spawner가 bullet생성
     - 마스터 네트워크 생성
    이동처리 각자
    충돌 각자
    스킬 각자
    사망 각자처리후 전송
     */


}