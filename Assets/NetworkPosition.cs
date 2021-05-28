using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPosition : MonoBehaviour
{
    private static NetworkPosition instance;

    private void Awake()
    {
        instance = this;

    }
    public static NetworkPosition GetInst() => instance;
}
