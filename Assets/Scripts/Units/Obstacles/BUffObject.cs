using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BUffObject : MonoBehaviourPun
{
    BuffData buff;
    PhotonView pv;
    [SerializeField] Text nameText;
    [SerializeField] Image buffImage;
    [SerializeField] BuffConfig buffConfig;
    [SerializeField] Canvas canvas;
    BoxCollider2D boxCollider;
    string objectName;
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
        boxCollider.enabled = true;
        canvas.enabled = true;
        transform.SetParent(GameSession.GetBulletHome());
        // fieldNumber = (int)pv.InstantiationData[0];
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinished);

    }
    void ParseBuffConfig(int index) {
        buffConfig = ConfigsManager.instance.buffConfigs[index];
        buff = buffConfig.Build();
        objectName = buffConfig.buff_name;
        nameText.text = objectName;
        buffImage.sprite = buffConfig.spriteImage;
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
        EventManager.StopListening(MyEvents.EVENT_GAME_FINISHED, OnFieldFinished);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BuffManager buffManager = collision.gameObject.GetComponent<BuffManager>();
        if (buffManager == null) return;
        EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj = string.Format("{0}님이 {1} 효과를 받았습니다..!", buffManager.pv.Owner.NickName, objectName) });
        if (buffManager.pv.IsMine)
        {
            buffManager.pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)buff.buffType, buff.modifier, buff.duration);
            GooglePlayManager.IncrementAchievement(GPGSIds.achievement_buff_object_activated, 1);
        }
        boxCollider.enabled = false;
        canvas.enabled = false;
        if (pv.IsMine) {
            deathRoutine = WaitAndDie();
            StartCoroutine(deathRoutine);
        }
        
 
    }
    IEnumerator deathRoutine;
    IEnumerator WaitAndDie() {
        yield return new WaitForSeconds(1f);
        PhotonNetwork.Destroy(pv);
    }


}
