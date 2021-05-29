using static ConstantStrings;
using UnityEngine;
using System;

public class HUDManager : MonoBehaviour
{

    [SerializeField] ScreenPanel[] panels;

    private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_SHOW_PANEL, EventOpenPanel);
        EventManager.StartListening(MyEvents.EVENT_POP_UP_PANEL, EventPopPanel);
        
    }

    private void EventPopPanel(EventObject v)
    {
        ScreenType screenType = (ScreenType)v.objData;
        Debug.Log("Changing to " + screenType);
        foreach (ScreenPanel panel in panels)
        {
            if (screenType == panel.mType) {
                panel.SetCanvasVisibility(true);
            }
        }
    }

    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_SHOW_PANEL, EventOpenPanel);
        EventManager.StopListening(MyEvents.EVENT_POP_UP_PANEL, EventPopPanel);

    }


 
    private void EventOpenPanel(EventObject v)
    {
        ScreenType screenType = (ScreenType)v.objData;
        foreach (ScreenPanel panel in panels) {
            panel.SetCanvasVisibility(screenType == panel.mType);
        }
    }


}

[System.Serializable]
public enum ScreenType { 
    PreGame,InGame,GameOver
}
