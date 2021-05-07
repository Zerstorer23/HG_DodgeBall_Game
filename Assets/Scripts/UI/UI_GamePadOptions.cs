using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_GamePadOptions : MonoBehaviour
{
    [SerializeField] GameObject optionPanel;
    [SerializeField] Toggle psToggle, xboxToggle;
    public static bool useGamepad = false;
    public static PadType padType;
  /*  private void Awake()
    {
        EventManager.StartListening(MyEvents.EVENT_POP_UP_PANEL, OnPanelOpen);
    }

    private void OnPanelOpen(EventObject arg0)
    {
        if ((ScreenType)arg0.objData == ScreenType.Settings) {
            if (arg0.boolObj)
            {
                InitInfo();

            }
        
        }
    }

    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_POP_UP_PANEL, OnPanelOpen);
    }*/
    private void Start()
    {
        InitInfo();
    }
    public void InitInfo() {
        useGamepad = (Input.GetJoystickNames().Length > 0);
        optionPanel.SetActive(useGamepad);
        if (useGamepad)
        {
            padType = (PadType)PlayerPrefs.GetInt(ConstantStrings.PREFS_MY_PAD, 0);
        }
    }

    public void OnClickToggle_Pad()
    {
        if (psToggle.isOn)
        {
            padType = PadType.PS4;
        }
        else
        {
            padType = PadType.XBOX;
        }
        PlayerPrefs.SetInt(ConstantStrings.PREFS_MY_PAD, (int)padType);
    }

}

public enum PadType
{
    PS4, XBOX
}