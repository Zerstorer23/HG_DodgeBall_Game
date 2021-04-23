using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    public Transform[] map_transforms;
    public CharacterSpawner charSpawner;
    public static float xMin, xMax, yMin, yMax, xMid, yMid;

    private static GameSession prGameSession;

    public static GameSession instance
    {
        get
        {
            if (!prGameSession)
            {
                prGameSession = FindObjectOfType<GameSession>();
                if (!prGameSession)
                {
                }
            }

            return prGameSession;
        }
    }
    public static GameSession GetInst() {
        return instance;
    }
    private void Awake()
    {
        xMin = map_transforms[0].position.x;
        xMax = map_transforms[1].position.x;
        yMin = map_transforms[0].position.y;
        yMax = map_transforms[1].position.y;
        xMid = (xMin + xMax) / 2;
        yMid = (yMin + yMax) / 2;
    }
    public static Vector3 GetRandomPosOnMap(float boundOffset = 0) {

        float randX = Random.Range(xMin+boundOffset, xMax- boundOffset);
        float randY = Random.Range(yMin+ boundOffset, yMax- boundOffset);
        return new Vector3(randX, randY,0);
    }


    }
