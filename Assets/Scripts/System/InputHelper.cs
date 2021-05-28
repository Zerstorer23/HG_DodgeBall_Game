using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputHelper :MonoBehaviour
{
    public delegate float FloatFunction();
    public static FloatFunction GetInputHorizontal;
    public static FloatFunction GetInputVertical;

    public delegate Vector3 Vector3Function();
    public static Vector3Function GetTargetVector;

    public delegate bool skillFireFunc();
    public static skillFireFunc skillKeyFired;


    public static string padXaxis = "RHorizontal";
    public static string padYaxis = "RVertical";
    private void Awake()
    {
        SetInputFunctions();
    }
    void SetInputFunctions()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            GetInputHorizontal = Control_MobileStick.GetInputHorizontal;
            GetInputVertical = Control_MobileStick.GetInputVertical;
            GetTargetVector = GetTouchPosition;
            skillKeyFired = FireButtonDown_Mobile;
        }
        else
        {
            GetInputHorizontal = GetKeyInputHorizontal;
            GetInputVertical = GetKeyInputVertical;
            GetTargetVector = GetMousePosition;
            skillKeyFired = FireButtonDown_PC;
        }
    }
    float GetKeyInputHorizontal()
    {
        return Input.GetAxis("Horizontal");
    }

    float GetKeyInputVertical()
    {
        return Input.GetAxis("Vertical");
    }
    Vector3 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
    Vector3 GetTouchPosition()
    {
        return Camera.main.ScreenToWorldPoint(UI_TouchPanel.touchVector);
    }

    public static void SetAxisNames()
    {
        switch (UI_GamePadOptions.padType)
        {
            case PadType.PS4:
                padXaxis = "RHorizontal";
                padYaxis = "RVertical";
                break;
            case PadType.XBOX:
                padXaxis = "RHorizontalXbox";
                padYaxis = "RVerticalXbox";
                break;
        }
    }
    private bool FireButtonDown_PC()
    {
        return Input.GetAxis("Fire1") > 0
            || Input.GetKeyDown(KeyCode.Joystick1Button5)
            || Input.GetKeyDown(KeyCode.Joystick1Button7);
    }
    private bool FireButtonDown_Mobile()
    {
        return UI_TouchPanel.isTouching;
    }
}
