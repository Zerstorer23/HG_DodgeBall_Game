using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameField : MonoBehaviour
{
    public int fieldNo = 0;
    public MapSpec mapSpec = new MapSpec();
    [SerializeField] public PlayerSpawner playerSpawner;
    [SerializeField] public BulletManager bulletSpawner;
    [SerializeField] public BuffObjectSpawner buffSpawner;
    Transform mapTransform;
    public Transform[] map_transforms;
    public Vector3 mapSize;
    public bool suddenDeathCalled = false;

 
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeathTriggered);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);

    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeathTriggered);
        EventManager.StopListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);
        suddenDeathCalled = false;
        fieldWinner = null;
    }


    private void OnGameFinished(EventObject obj)
    {
        if (suddenDeathNumerator != null)
        {
            StopCoroutine(suddenDeathNumerator);
        }
        gameObject.SetActive(false);
    }


    private void OnSuddenDeathTriggered(EventObject obj)
    {
        if (suddenDeathNumerator != null)
        {
            StopCoroutine(suddenDeathNumerator);
        }
        startTime = PhotonNetwork.Time;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = "맵 크기가 줄어듭니다!!" });
        suddenDeathNumerator = ResizeMapByTime();
        StartCoroutine(suddenDeathNumerator);
    }


    public float originalSize;
    public double startTime;
    public double resizeOver = 60d;
    public float minResizeWidth = 10f;
    IEnumerator suddenDeathNumerator;

    public IEnumerator ResizeMapByTime()
    {

        bool doRoutine = true;
        double elapsedTime = 0;
        while (doRoutine)
        {
            float newLength = minResizeWidth + (originalSize - minResizeWidth) * (float)(1 - (elapsedTime / resizeOver));
            mapTransform.localScale = new Vector3(newLength, newLength);


            yield return new WaitForFixedUpdate();
            ///  yield return new WaitForSeconds(0.05f);
            mapSpec.xMin = map_transforms[0].position.x;
            mapSpec.xMax = map_transforms[1].position.x;
            mapSpec.yMin = map_transforms[0].position.y;
            mapSpec.yMax = map_transforms[1].position.y;
            mapSpec.xMid = (mapSpec.xMin + mapSpec.xMax) / 2;
            mapSpec.yMid = (mapSpec.yMin + mapSpec.yMax) / 2;
            elapsedTime = PhotonNetwork.Time - startTime;
            doRoutine = elapsedTime < resizeOver;
        }

    }



    public GameMode fieldType;
    
    public void InitialiseMap(int id = 0) {
        fieldNo = id;
        switch (fieldType)
        {
            case GameMode.PVP:
                InitialiseMapSize();
                break;
            case GameMode.TEAM:
                InitialiseMapSize();
                break;
            case GameMode.PVE:
                break;
            case GameMode.Tournament:
                InitialiseMapSize();
                break;
        }

    }

    internal void StartEngine()
    {
        playerSpawner.StartEngine();
        bulletSpawner.StartEngine();
        buffSpawner.StartEngine();
    }

    private void InitialiseMapSize()
    {
        mapTransform = GetComponent<Transform>();
        mapSize = mapTransform.localScale;
        int numPlayer = PhotonNetwork.CurrentRoom.PlayerCount;
        float modifiedLength = GameFieldManager.instance.mapStepsize * (numPlayer / GameFieldManager.instance.mapStepPerPlayer);
        Vector3 newSize = mapSize + new Vector3(modifiedLength, modifiedLength);
        mapTransform.localScale = newSize;
        originalSize = newSize.x;
        mapSpec.xMin = map_transforms[0].position.x;
        mapSpec.xMax = map_transforms[1].position.x;
        mapSpec.yMin = map_transforms[0].position.y;
        mapSpec.yMax = map_transforms[1].position.y;
        mapSpec.xMid = (mapSpec.xMin + mapSpec.xMax) / 2;
        mapSpec.yMid = (mapSpec.yMin + mapSpec.yMax) / 2;
        ResizePlayerMap(numPlayer);
    }
    int w;
    int h;
    void ResizePlayerMap(int numPlayer)
    {
        w = 1;
        h = 1;
        bool multWidth = false;
        while (w * h < numPlayer)
        {
            if (multWidth)
            {
                w++;
            }
            else
            {
                h++;
            }
            multWidth = !multWidth;
        }
        Debug.Log(w + "," + h + " can store " + numPlayer);
    }


    public Vector3 GetRandomPlayerSpawnPosition(int pNumber)
    {
        int x = pNumber % w;
        int y = pNumber / w;
        //  Debug.Log(pNumber+ " Found " + x + "," + y);
        return GetPoissonPositionNear(x, y);

    }
    public Vector3 GetRandomPosition(float boundOffset = 0)
    {
        float randX = Random.Range(mapSpec.xMin + boundOffset, mapSpec.xMax - boundOffset);
        float randY = Random.Range(mapSpec.yMin + boundOffset, mapSpec.yMax - boundOffset);
        return new Vector3(randX, randY, 0);

    }
    public Vector3 GetRandomPositionNear(Vector3 center, float window, float boundOffset = 0)
    {

        float randX = Random.Range(-window, window);
        float randY = Random.Range(-window, window);
        if (randX <= (mapSpec.xMin + boundOffset)) randX = mapSpec.xMin + boundOffset;
        if (randX >= (mapSpec.xMax - boundOffset)) randX = mapSpec.xMax - boundOffset;
        if (randY <= (mapSpec.yMin + boundOffset)) randY = mapSpec.yMin + boundOffset;
        if (randY <= (mapSpec.yMin + boundOffset)) randY = mapSpec.yMin + boundOffset;
        return new Vector3(randX, randY, 0);
    }
    public Vector3 GetPoissonPositionNear(int x, int y)
    {
        float width = (mapSpec.xMax - mapSpec.xMin) / w;
        float height = (mapSpec.yMax - mapSpec.yMin) / h;

        float xOffset = width / 2;
        float yOffset = height / 2;

        float randX = Random.Range(-width / 4, width / 4);
        float randY = Random.Range(-height / 4, height / 4);
        //    Debug.Log("Width units " + width + "," + height);
        // Debug.Log("Offset units " + xOffset + "," + yOffset);
        //     Debug.Log("rand units " + randX + "," + randY);
        // Debug.Log("start units " + xMin + "," + yMin);
        return new Vector3(mapSpec.xMin + xOffset + width * x + randX, mapSpec.yMin + yOffset + height * y + randY);
    }

    public Player fieldWinner = null;

    internal void NotifyFieldWinner(Player winner)
    {
        fieldWinner = winner;
        GameFieldManager.CheckGameFinished();
    }
    /*    void OnDrawGizmos()
{
   // Green
   DrawRect(new Vector3(mapSpec.xMax - mapSpec.xMin, mapSpec.yMax - mapSpec.yMin), 5f);
}

void OnDrawGizmosSelected()
{
   // Orange
   Gizmos.color = new Color(1.0f, 1.0f, 1.0f);
   DrawRect(new Vector3(mapSpec.xMax - mapSpec.xMin, mapSpec.yMax - mapSpec.yMin), 5f);
}
private void DrawRect(Vector2 size, float thikness)
{
   var matrix = Gizmos.matrix;
   Gizmos.matrix = transform.localToWorldMatrix;

   //top cube
   Gizmos.DrawCube(Vector3.up * size.y / 2, new Vector3(size.x, thikness, 0.01f));

   //bottom cube
   Gizmos.DrawCube(Vector3.down * size.y / 2, new Vector3(size.x, thikness, 0.01f));

   //left cube
   Gizmos.DrawCube(Vector3.left * size.x / 2, new Vector3(thikness, size.y, 0.01f));

   //right cube
   Gizmos.DrawCube(Vector3.right * size.x / 2, new Vector3(thikness, size.y, 0.01f));

   Gizmos.matrix = matrix;
}*/

}
