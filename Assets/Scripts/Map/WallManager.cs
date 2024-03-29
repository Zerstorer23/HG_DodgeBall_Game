﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
    public List<GameObject> sets = new List<GameObject>();
    int selectedIndex = -1;

    private void Awake()
    {
         sets.Clear();
        foreach (Transform i in transform)
        {
            sets.Add(i.gameObject);
        }
    }

    public void SelectRandomPreset() {
        int seed = (int)PhotonNetwork.CurrentRoom.CustomProperties[ConstantStrings.HASH_ROOM_RANDOM_SEED];
        selectedIndex = seed % sets.Count;
        sets[selectedIndex].SetActive(true);
    }


    private void OnDisable()
    {
        DisableWalls();
    }
    public void DisableWalls() {
        if (selectedIndex != -1)
        {
            sets[selectedIndex].SetActive(false);
            selectedIndex = -1;
        }
    }

}
