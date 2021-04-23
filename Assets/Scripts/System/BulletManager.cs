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
    [Header("Spawner setting")]
    public int currentSpawned = 0;
    public float minDelay, maxDelay;
    public float minDuration, maxDuration;

    [Header("Projectile settings")]
    public float maxProjSpeed = 15f;
    public float maxProjRotateScale = 180f;
    [SerializeField] float minProjSize = 0.5f, maxProjSize = 1.5f;
    [Header("Box settings")]
    public float maxWidth = 10f;
    public float spawnDelay = 3f;

    PhotonView pv;

    int[] mapDifficulties = { 0, 12, 12, 24 };
    MapDifficulty currentDifficult;
    private void Awake()
    {

        pv = GetComponent<PhotonView>();
        instance = this;
        EventManager.StartListening(MyEvents.EVENT_SPAWNER_EXPIRE, OnSpawnerExpired);

        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
    }

    private bool isGameFinished;
    private void OnGameEnd(EventObject arg0)
    {
        isGameFinished = true;
    }

    private void OnEnable()
    {
        isGameFinished = false;
    }


    private void Start()
    {
         Hashtable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        MapDifficulty mapDiff =(MapDifficulty)roomSetting[ConstantStrings.HASH_MAP_DIFF];
        ConnectedPlayerManager.SetRoomSettings(ConstantStrings.HASH_MAP_DIFF, mapDiff);
        currentDifficult = mapDiff;
        activeMax = mapDifficulties[(int)currentDifficult];
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
        if (!PhotonNetwork.IsMasterClient || isGameFinished) return;
        CheckSpawnerSpawns();

    }

    private void CheckSpawnerSpawns()
    {
        if (currentSpawned >= activeMax ) return;
        while (currentSpawned < activeMax)
        {
            SpawnDirection spawnDir = GetRandomSpawnDir();
            MoveType moveType = GetRandomMoveType(spawnDir);
            InstantiateSpanwer(spawnDir, moveType);
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
    private void InstantiateSpanwer(SpawnDirection spawnDir, MoveType moveType)
    {
        if (isGameFinished) return;
        //Projectile
        switch ((MoveType)moveType)
        {
            case MoveType.Static:
                InstantiateBox();
                break;
            case MoveType.Curves:
            case MoveType.Straight:
                Vector3 randPos = GetRandomBoundaryPos();
                GameObject spawner = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BulletSpawner", randPos, Quaternion.identity,0);
                SetProjectileInformation(spawner, spawnDir, moveType);
                SetProjectileBehaviour(spawner, randPos);
                spawner.transform.SetParent(transform);
                break;
        }



    }

    private void InstantiateBox()
    {
        float randW = Random.Range(1f, maxWidth);
        Vector3 randPos = GameSession.GetRandomPosOnMap();

        GameObject box = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BoxObstacle", randPos, Quaternion.identity,0);
        box.GetComponent<PhotonView>().RPC("SetInformation", RpcTarget.AllBuffered, randW,spawnDelay);
        box.transform.SetParent(transform);
    }
    void SetProjectileInformation(GameObject spawner, SpawnDirection spawnDir, MoveType moveType)
    {
        float moveSpeed = Random.Range(5f, maxProjSpeed);
        float rotateSpeed = Random.Range(5f, maxProjRotateScale);
        float blockSize = Random.Range(minProjSize, maxProjSize);
        spawner.GetComponent<PhotonView>().RPC("SetProjectile", RpcTarget.AllBuffered, (int)spawnDir, (int)moveType, blockSize, moveSpeed, rotateSpeed);
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

    private MoveType GetRandomMoveType(SpawnDirection spawnDir)
    {
        if (spawnDir == SpawnDirection.Preemptive)
            return MoveType.Static;
        return (MoveType)Random.Range(1, 3);
    }

    private SpawnDirection GetRandomSpawnDir()
    {
        if (currentDifficult == MapDifficulty.BoxOnly) return SpawnDirection.Preemptive;
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