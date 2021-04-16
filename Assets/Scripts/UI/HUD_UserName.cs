using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_UserName : MonoBehaviourPun
{
    public PhotonView pv;
    public bool isReady = false;
    public string playerName = "ㅇㅇ";
    public CharacterType selectedCharacter = CharacterType.HARUHI;


    [SerializeField]Image readySPrite;
    [SerializeField]Text nameText;
    [SerializeField]Image charPortrait;


    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void OnEnable()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_JOINED, new EventObject() { stringObj=pv.Owner.UserId, gameObject = gameObject });
    }
    private void OnDisable()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_LEFT, new EventObject() { stringObj = pv.Owner.UserId, gameObject = gameObject });
    }

    [PunRPC]
    public void ChangeCharacter(int character)
    {
        selectedCharacter =(CharacterType) character;
        UpdateUI();
    }
    [PunRPC]
    public void ChangeName(string text)
    {
        playerName = text;
        PhotonNetwork.NickName = text;
        UpdateUI();
    }

    [PunRPC]
    public void ToggleReady()
    {
        isReady = !isReady; 
        UpdateUI();
    }
    public bool GetReady() {
        return isReady;
    }

    public void UpdateUI()
    {
        nameText.text = playerName;
        readySPrite.color = (isReady) ? Color.green : Color.black;
        charPortrait.sprite = MenuManager.unitDictionary[selectedCharacter].portraitImage;

        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add("CHARACTER", selectedCharacter);
        pv.Owner.SetCustomProperties(hash);
    }
}
