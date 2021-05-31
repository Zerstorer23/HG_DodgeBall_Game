using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
  [SerializeField]  GameObject[] WallSets;
    int selectedIndex = -1;
    
    private void OnEnable()
    {
        bool enableWalls = GameSession.gameModeInfo.gameMode == GameMode.PVP;
        if (enableWalls) {
            selectedIndex = Random.Range(0, WallSets.Length);
            WallSets[selectedIndex].SetActive(true);        
        }

    }
    private void OnDisable()
    {
        if (selectedIndex != -1) {
            WallSets[selectedIndex].SetActive(false);
            selectedIndex = -1;
        }
    }

}
