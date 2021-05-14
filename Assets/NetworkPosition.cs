using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPosition : MonoBehaviour
{
    private static NetworkPosition instance;
    [SerializeField] internal Unit_AutoDrive autoDriver;

    private void Awake()
    {
        instance = this;
        autoDriver.gameObject.SetActive(false);
    }
    public static void ConnectPlayer(Unit_Player player) {
        instance.autoDriver.SetInfo(player);
        instance.autoDriver.gameObject.SetActive(true);
    }


    public static NetworkPosition GetInst() => instance;
}
