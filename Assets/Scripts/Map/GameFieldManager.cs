using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFieldManager : MonoBehaviourPun
{
    Transform mapTransform;
    public Transform[] map_transforms;
    public Vector3 mapSize;
    public float mapStepsize = 10f;
    public int mapStepPerPlayer = 5;
    public static float xMin, xMax, yMin, yMax, xMid, yMid;

    private static GameFieldManager instance;


    private void Awake()
    {
        instance = this;
        mapTransform = GetComponent<Transform>();
        mapSize = mapTransform.localScale;
        InitialiseMapSize();
        EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeathTriggered);
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);

    }


    private void OnDestroy()
    {

        EventManager.StartListening(MyEvents.EVENT_REQUEST_SUDDEN_DEATH, OnSuddenDeathTriggered);
        EventManager.StopListening(MyEvents.EVENT_GAME_FINISHED, OnGameFinished);
    }
    private void OnGameFinished(EventObject obj)
    {
        if (suddenDeathNumerator != null)
        {
            StopCoroutine(suddenDeathNumerator);
        }
    }


    private void OnSuddenDeathTriggered(EventObject obj)
    {
        if (suddenDeathNumerator != null) {
            StopCoroutine(suddenDeathNumerator);
        }
        startTime = PhotonNetwork.Time;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = "맵 크기가 줄어듭니다!!" });
        suddenDeathNumerator = ResizeMapByTime();
        StartCoroutine(suddenDeathNumerator);
    }

    public static GameFieldManager GetInst() => instance;

    public float originalSize;
    public double startTime;
    public double resizeOver = 60d;
    public float minResizeWidth = 10f;
    IEnumerator suddenDeathNumerator;

    public IEnumerator ResizeMapByTime()
    {

        bool doRoutine = true;
        double elapsedTime =0;
        while (doRoutine)
             {
            float newLength = minResizeWidth + (originalSize - minResizeWidth) * (float)(1 - (elapsedTime / resizeOver));
            mapTransform.localScale = new Vector3(newLength, newLength);

        
             yield return new WaitForFixedUpdate();
          ///  yield return new WaitForSeconds(0.05f);
            xMin = map_transforms[0].position.x;
            xMax = map_transforms[1].position.x;
            yMin = map_transforms[0].position.y;
            yMax = map_transforms[1].position.y;
            xMid = (xMin + xMax) / 2;
            yMid = (yMin + yMax) / 2;
            elapsedTime = PhotonNetwork.Time - startTime;
            doRoutine = elapsedTime < resizeOver;
        }

    }
    void OnDrawGizmos()
    {
        // Green
        DrawRect(new Vector3(xMax - xMin, yMax - yMin), 5f);
    }

    void OnDrawGizmosSelected()
    {
        // Orange
        Gizmos.color = new Color(1.0f, 1.0f, 1.0f);
        DrawRect(new Vector3(xMax-xMin , yMax - yMin), 5f);
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
    }
    public void InitialiseMapSize(int numPlayer =0)
    {
        float modifiedLength = mapStepsize * (numPlayer / mapStepPerPlayer);
        Vector3 newSize = mapSize + new Vector3(modifiedLength, modifiedLength);
        mapTransform.localScale = newSize;
        originalSize = newSize.x;
        xMin = map_transforms[0].position.x;
        xMax = map_transforms[1].position.x;
        yMin = map_transforms[0].position.y;
        yMax = map_transforms[1].position.y;
        xMid = (xMin + xMax) / 2;
        yMid = (yMin + yMax) / 2;
        ResizePlayerMap(numPlayer);
    }
    int w;
    int h;
    void ResizePlayerMap(int numPlayer) {
        w = 1;
        h = 1;
        bool multWidth = false;
        while (w * h < numPlayer) {
            if (multWidth) {
                w++;
            }
            else {
                h++;
            }
            multWidth = !multWidth;
        }
       Debug.Log(w + "," + h + " can store " + numPlayer);
    }


    public Vector3 GetRandomPlayerSpawnPosition(int pNumber) {
        int x = pNumber % w;
        int y = pNumber / w;
      //  Debug.Log(pNumber+ " Found " + x + "," + y);
        return GetRandomPositionNear(x, y);
    
    }
    public Vector3 GetRandomPositionNear(int x, int y) {
        float width = (xMax - xMin) / w;
        float height = (yMax - yMin) /h;

        float xOffset = width / 2;
        float yOffset = height / 2;

        float randX = Random.Range(-width / 4, width / 4);
        float randY = Random.Range(-height / 4, height / 4);
        //    Debug.Log("Width units " + width + "," + height);
        // Debug.Log("Offset units " + xOffset + "," + yOffset);
      //     Debug.Log("rand units " + randX + "," + randY);
        // Debug.Log("start units " + xMin + "," + yMin);
        return new Vector3(xMin + xOffset +  width * x + randX, yMin+ yOffset+ height * y + randY);
    }


}
