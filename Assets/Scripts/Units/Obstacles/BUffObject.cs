﻿using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffObject : MonoBehaviourPun
{
    BuffData buff;
    PhotonView pv;
    [SerializeField] Text timeText;
    [SerializeField] Image buffImage;
    [SerializeField] public BuffConfig buffConfig;
    [SerializeField] Canvas canvas;
    public BoxCollider2D boxCollider;
    string objectName;
    Sprite originalBuffImage;
   [SerializeField] Sprite buffMaskedImage;
    public BuffObjectStatus status = BuffObjectStatus.Starting;
    int fieldNumber = 0;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void OnEnable()
    {
        fieldNumber = (int)pv.InstantiationData[0];
        int index = (int)pv.InstantiationData[1];
        ParseBuffConfig(index);
        SetBuffStatus(BuffObjectStatus.Starting);
        transform.SetParent(GameSession.GetBulletHome());
        // fieldNumber = (int)pv.InstantiationData[0];
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinished);
        StartCoroutine(WaitAndActivate());
    }

    float waitTime = 3f;
    IEnumerator WaitAndActivate() {
        for (int i = 0; i < waitTime; i++) {
            timeText.text = (waitTime - i).ToString("0");
            yield return new WaitForSeconds(1f);
        }
        SetBuffStatus(BuffObjectStatus.Enabled);
    }

    void ParseBuffConfig(int index) {
        buffConfig = ConfigsManager.instance.buffConfigs[index];
        buff = buffConfig.Build();
        objectName = LocalizationManager.Convert(buffConfig.buff_name);
        originalBuffImage = buffConfig.spriteImage;
    }

    private void OnFieldFinished(EventObject obj)
    {
        if (obj.intObj != fieldNumber) return;
        if (pv.IsMine)
        {
            if (deathRoutine != null) StopCoroutine(deathRoutine);    
        
            PhotonNetwork.Destroy(pv);
        }
    }

    private void OnDisable()
    {
        status = BuffObjectStatus.Starting;
        EventManager.StopListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinished);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BuffManager buffManager = collision.gameObject.GetComponent<BuffManager>();
        if (buffManager == null) return;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = LocalizationManager.Convert("_msg_p_gets_buff", buffManager.controller.Owner.NickName, objectName) });
        if (buffManager.pv.IsMine)
        {
            buffManager.pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)buff.buffType, buff.modifier, buff.duration);
            GooglePlayManager.IncrementAchievement(GPGSIds.achievement_buff_object_activated, 1);
        }
        SetBuffStatus(BuffObjectStatus.Disabled);
        if (pv.IsMine) {
            deathRoutine = WaitAndDie();
            StartCoroutine(deathRoutine);
        }
    }
    void SetBuffStatus(BuffObjectStatus status) {
        if (status == BuffObjectStatus.Starting)
        {
            boxCollider.enabled = false;
            canvas.enabled = true;
            buffImage.sprite = buffMaskedImage;
        }
        else if (status == BuffObjectStatus.Enabled)
        {
            boxCollider.enabled = true;
            canvas.enabled = true;
            buffImage.sprite = originalBuffImage;
            timeText.text = "";
        }
        else {

            boxCollider.enabled = false;
            canvas.enabled = false;
        }
        this.status = status;

    }

    IEnumerator deathRoutine;
    IEnumerator WaitAndDie() {
        yield return new WaitForSeconds(1f);
        PhotonNetwork.Destroy(pv);
    }

    
}
public enum BuffObjectStatus { 
    Starting,Enabled,Disabled
}
