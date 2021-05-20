﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_RemainingPlayerDisplay : MonoBehaviour
{
    Text displayText;
    private void Awake()
    {
        displayText = GetComponent<Text>();
        EventManager.StartListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerUpdate);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerUpdate);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_PLAYER_SPAWNED, OnPlayerUpdate);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_DIED, OnPlayerUpdate);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAFrame());
    }
    // Update is called once per frame
    void OnPlayerUpdate(EventObject eo)
    {
        if(gameObject.activeInHierarchy)
            StartCoroutine(WaitAFrame());
    }
    IEnumerator WaitAFrame() {
        yield return new WaitForFixedUpdate();
        int remain = GameFieldManager.GetRemainingPlayerNumber();
        displayText.text = "남은 플레이어 : " + remain;
    }

}