﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Control_MobileStick : MonoBehaviour , IDragHandler,IPointerUpHandler,IPointerDownHandler
{
   [SerializeField] RectTransform stick;
    [SerializeField] RectTransform stickBackground;
    public float radius;

    private static  Control_MobileStick mobileStick;
    public  UI_MobileAuto mobileAuto;
    Vector2 offset;
    private void Awake()
    {
        radius = stickBackground.rect.width * 0.5f;
        offset = new Vector2(radius, radius);
        mobileStick = this;
    }
    private void OnEnable()
    {
        mobileAuto.gameObject.SetActive(
            GameSession.auto_drive_enabled
            && ( Application.platform == RuntimePlatform.Android)
            );
    }

    public void OnPointerDown(PointerEventData ped)
    {
            OnDrag(ped);
   
    }

    public void OnPointerUp(PointerEventData ped)
    {
        delta = Vector2.zero;
        stick.transform.localPosition = Vector2.zero + offset;
    }
    public Vector2 delta;

    public void OnDrag(PointerEventData ped)
    {
        Vector2 touchPoint = ped.position;
        Vector2 rectPos =stickBackground.anchoredPosition + offset;// stickBackground.position;//
        float dist = Vector2.Distance(rectPos, touchPoint);
        delta = touchPoint - rectPos;
        stick.localPosition = Vector2.ClampMagnitude(delta, radius) + offset;
        return;     
    }

    public static Vector2 GetInput()
    {
        return mobileStick.delta.normalized;
    }
    public static float GetInputHorizontal()
    {
        return mobileStick.delta.normalized.x;
    }
    public static float GetInputVertical()
    {
        return mobileStick.delta.normalized.y;
    }
}
