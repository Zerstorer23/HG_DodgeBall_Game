using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SubOptionsManager : MonoBehaviour
{
    [SerializeField] GameObject anonGame, changeTeam, addBot, removeBot;
    private void Awake()
    {

        EventManager.StartListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);

    }
/*    private void OnEnable()
    {
        OnGamemodeChanged(null);
    }*/
    private void OnGamemodeChanged(EventObject arg0)
    {
        GameModeConfig mode = arg0.Get<GameModeConfig>();
       StartCoroutine(WaitAndActive(mode));

    }
    IEnumerator WaitAndActive(GameModeConfig mode) {
        yield return new WaitForFixedUpdate();
        changeTeam.SetActive(mode.isTeamGame);
        addBot.SetActive(mode.allowBots);
        removeBot.SetActive(mode.allowBots);
    }
}
