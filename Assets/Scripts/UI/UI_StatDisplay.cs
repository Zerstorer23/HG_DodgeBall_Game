using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StatDisplay : MonoBehaviour
{
    Text displayText;
    Unit_Player myPlayer;

    private void Awake()
    {
        displayText = GetComponent<Text>();
    }
    public void SetPlayer(Unit_Player p) {
        myPlayer = p;
    }

    void Update()
    {
        if (myPlayer == null) return;
        int kills = myPlayer.kills;
        int evades = myPlayer.evasion;
        displayText.text = string.Format("{0}...{1}킬 {2}회피", PhotonNetwork.NickName, kills.ToString(), evades.ToString());
    }
}
