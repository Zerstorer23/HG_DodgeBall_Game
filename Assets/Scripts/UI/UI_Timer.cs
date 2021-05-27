using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Timer : MonoBehaviour
{
    Text timeText;
    private void Awake()
    {
        timeText = GetComponent<Text>();
    }
    public static double startTime;
    private void OnEnable()
    {
        startTime = PhotonNetwork.Time;
    }
    private void FixedUpdate()
    {
        double curr = PhotonNetwork.Time;
        timeText.text = (curr - startTime).ToString("00.0");
    }
}
