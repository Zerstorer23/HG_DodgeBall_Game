using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_ConnPlayers : MonoBehaviour
{
    public UniversalPlayer[] debugPlayers;
    private void FixedUpdate()
    {
        debugPlayers = PlayerManager.GetPlayers();
    }
}
