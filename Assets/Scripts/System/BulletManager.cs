using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using static GameFieldManager;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class BulletManager : MonoBehaviourPun
{
    public Transform Home_Bullets;
    private static BulletManager instance;
    public float startSpawnAfter = 2f;
    double startTime;

    public int activeMax = 30;
    [Header("Spawner setting")]
    public int currentSpawned = 0;
    public float minDelay, maxDelay;
    public float minDuration, maxDuration;

    [Header("Projectile settings")]
    public float maxProjSpeed = 15f;
    public float maxProjRotateSpeed = 120f;
    [SerializeField] float minProjSize = 0.5f, maxProjSize = 1.5f;
    [Header("Box settings")]
    public float maxWidth = 10f;
    public float spawnDelay = 3f;

    PhotonView pv;

    int[] mapDifficulties = { 0, 12, 16, 32 };
    public int modPerPerson = 5;
    public float modPerStep = 0.5f;

    MapDifficulty currentDifficult;
    private void Awake()
    {

        pv = GetComponent<PhotonView>();
        instance = this;
        EventManager.StartListening(MyEvents.EVENT_SPAWNER_EXPIRE, OnSpawnerExpired);
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStart);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_SPAWNER_EXPIRE, OnSpawnerExpired);
        EventManager.StopListening(MyEvents.EVENT_GAME_STARTED, OnGameStart);
        EventManager.StopListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
    }
    public bool doSpawning;
    private void OnGameStart(EventObject obj)
    {
        doSpawning = true;
        StartEngine();
    }

    private void OnGameEnd(EventObject arg0)
    {
        doSpawning = false;
    }

    private void StartEngine()
    {
        Hashtable roomSetting = PhotonNetwork.CurrentRoom.CustomProperties;
        MapDifficulty mapDiff =(MapDifficulty)roomSetting[ConstantStrings.HASH_MAP_DIFF];
        currentDifficult = mapDiff;
        float baseNum = mapDifficulties[(int)currentDifficult];
        float modifier = 1+ ConnectedPlayerManager.GetPlayerDictionary().Count / modPerPerson * modPerStep;

        Debug.Log("MOdifier base"+baseNum+" mod " + modifier);
        activeMax =(int)( baseNum * modifier);
        startTime = PhotonNetwork.Time;
        Debug.Log("Engine set "+ activeMax);
    }
    public static BulletManager GetInstance() {
        return instance;
    }

    // Update is called once per frame
    void Update()
    {
/*            Debug.Log("mc "+PhotonNetwork.IsMasterClient);
            Debug.Log("spawn "+ doSpawning);
          Debug.Log("time "+ PhotonNetwork.Time+" at "+ startTime);*/
        if (!PhotonNetwork.IsMasterClient || !doSpawning) return;
        if (PhotonNetwork.Time <= startTime + startSpawnAfter) return;
        CheckSpawnerSpawns();

    }

    private void CheckSpawnerSpawns()
    {
        if (currentSpawned >= activeMax ) return;
        while (currentSpawned < activeMax)
        {
            SpawnDirection spawnDir = GetRandomSpawnDir();
            SpawnerType moveType = GetRandomMoveType(spawnDir);
            ReactionType reaction = GetRandomReactionType();
            InstantiateSpanwer(spawnDir, moveType, reaction);
            pv.RPC("IncrementSpawned", RpcTarget.AllBuffered);
        }
    }

    private ReactionType GetRandomReactionType()
    {
        return (ReactionType)Random.Range(0, (int)ReactionType.None);
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
    private void InstantiateSpanwer(SpawnDirection spawnDir, SpawnerType moveType, ReactionType reaction)
    {
        if (!doSpawning) return;
        //Projectile
        switch ((SpawnerType)moveType)
        {
            case SpawnerType.Static:
                InstantiateBox();
                break;
            case SpawnerType.Curves:
            case SpawnerType.Straight:
                Vector3 randPos = GetRandomBoundaryPos();
                UnityEngine.GameObject spawner = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BulletSpawner", randPos, Quaternion.identity,0);
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

        UnityEngine.GameObject box = PhotonNetwork.InstantiateRoomObject("Prefabs/Units/BoxObstacle", randPos, Quaternion.identity,0);
        box.GetComponent<PhotonView>().RPC("SetInformation", RpcTarget.AllBuffered, randW,spawnDelay);
        box.transform.SetParent(transform);
    }
    void SetProjectileInformation(UnityEngine.GameObject spawner, SpawnDirection spawnDir, SpawnerType moveType, ReactionType reaction)
    {
        float moveSpeed = Random.Range(5f, maxProjSpeed);
        float rotateSpeed = Random.Range(5f, maxProjRotateSpeed);
        float blockSize = Random.Range(minProjSize, maxProjSize);
        spawner.GetComponent<PhotonView>().RPC("SetProjectile", RpcTarget.AllBuffered, (int)spawnDir, (int)moveType, (int)reaction,blockSize, moveSpeed, rotateSpeed);
    }
    void SetProjectileBehaviour(UnityEngine.GameObject spawner, Vector3 randPos)
    {
        //Behaviour
        int headingDirection = (int)GetHeadingAngle(randPos);
        float angleRange;//= Random.Range(minProjRotateScale, masProjRotateSpeed); 
        if (headingDirection <= 3)
        {
            angleRange = Random.Range(45f, 120f);
        }
        else
        {
            angleRange = Random.Range(0f, 45f);
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

    private SpawnerType GetRandomMoveType(SpawnDirection spawnDir)
    {
        if (spawnDir == SpawnDirection.Preemptive)
            return SpawnerType.Static;
        return (SpawnerType)Random.Range(1, 3);
    }
    public float boxProbability = 0.25f;
    private SpawnDirection GetRandomSpawnDir()
    {
        if (currentDifficult == MapDifficulty.BoxOnly) return SpawnDirection.Preemptive;
        if(Random.Range(0,1f) <= boxProbability) return SpawnDirection.Preemptive;
        return (SpawnDirection)Random.Range(1, (int)SpawnDirection.None);
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
public enum SpawnDirection
{
    Preemptive, Straight, Spiral, None
}

//이동방식
public enum SpawnerType
{
    Static, Curves, Straight
}
public enum ReactionType { 
    Die,Bounce,None
}

public enum Directions
{
    W = 0, E = 1, N = 2, S = 3, NW, NE, SW, SE,
}