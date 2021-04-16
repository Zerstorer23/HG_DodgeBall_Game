using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;
using Photon.Pun;
using UnityEngine.UI;

public class PingDIsplay : MonoBehaviourPun
{
  [SerializeField] Text pingValueText;
  
    void Update()
    {
        pingValueText.text=  PhotonNetwork.GetPing()+"ms";
        if (PhotonNetwork.IsMasterClient)
        {
            pingValueText.color = Color.red;
        }
        else
        {
            pingValueText.color = Color.green;

        }

    }
}
