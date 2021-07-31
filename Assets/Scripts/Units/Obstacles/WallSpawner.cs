using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;

public class WallSpawner : MonoBehaviour
{
    [SerializeField] GameField gameField;
    WallManager wallmanager;
    List<GameObject> createdObjs = new List<GameObject>();
    string rootFolder = "Prefabs/MapFactors/";
    private void Awake()
    {
        wallmanager = GetComponentInChildren<WallManager>();
    }
    public void SetWalls()
    {
        wallmanager.DisableWalls();
        bool enableWalls = (GameSession.gameModeInfo.gameMode == GameMode.PVP || GameSession.gameModeInfo.gameMode == GameMode.TeamCP);
        if (GameSession.instance != null && GameSession.instance.devMode) enableWalls = false;
        if (enableWalls)
        {
            int choice = (int)PhotonNetwork.CurrentRoom.CustomProperties[ConstantStrings.HASH_SUB_MAP_OPTIONS];
            if (GameSession.gameModeInfo.gameMode == GameMode.PVP && choice == 1)
            {
                CreateMap(10);
            }
            else
            {
                wallmanager.SelectRandomPreset();
            }
        }

    }

    public void CreateMap(int max) {
        if (!PhotonNetwork.IsMasterClient) return;
        createdObjs.Clear();
        for (int i = 0; i < max; i++)
        {
            int seed = Random.Range(0, 2);
            if (seed == 0)
            {
                CreateBoundary();
            }
            else if (seed == 1)
            {
                CreatePortal();
                i++;
            }
            else {
                CreateConveyer();
            }
        }

    }

    private void CreateBoundary()
    {
        Vector3 randomPos = gameField.GetRandomPosition(3f);
        Quaternion randomRot = Quaternion.Euler(new Vector3(0,0,Random.Range(0,360)));
        float length = Random.Range(5f, 30f);
        var go = PhotonNetwork.InstantiateRoomObject(rootFolder + "Wall", randomPos, randomRot, 0,new object[] { 
            gameField.fieldNo,length
        });
        createdObjs.Add(go);
    }

    private void CreatePortal()
    {
        Vector3 randomPos = gameField.GetRandomPosition(1f);
        var go1 = PhotonNetwork.InstantiateRoomObject(rootFolder + "Teleporter", randomPos, Quaternion.identity, 0, new object[] {
        gameField.fieldNo
        });
        PhotonView startPV = go1.GetComponent<PhotonView>();
       
        randomPos = gameField.GetRandomPosition(1f);
        var go2 = PhotonNetwork.InstantiateRoomObject(rootFolder + "Teleporter", randomPos, Quaternion.identity, 0, new object[] {
        gameField.fieldNo
        });
        PhotonView endPV = go2.GetComponent<PhotonView>();
        startPV.RPC("LinkPortal", RpcTarget.AllBuffered, endPV.ViewID);
        endPV.RPC("LinkPortal", RpcTarget.AllBuffered, startPV.ViewID);
        createdObjs.Add(go1);
        createdObjs.Add(go2);
    }

    private void CreateConveyer()
    {
        Vector3 randomPos = gameField.GetRandomPosition(3f);
        Quaternion randomRot = Quaternion.Euler(new Vector3(0, 0, Random.Range(0, 360)));
        float length = Random.Range(15f, 45f);
        var go = PhotonNetwork.InstantiateRoomObject(rootFolder + "ConveryBelt", randomPos, randomRot, 0, new object[] {
        gameField.fieldNo,length
        });
        createdObjs.Add(go);
    }

    private void OnDisable()
    {
        foreach (var go in createdObjs) {
            PhotonNetwork.Destroy(go);
        }
        createdObjs.Clear();
    }
}
