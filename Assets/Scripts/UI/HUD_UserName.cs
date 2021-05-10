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
    //  public string playerName = "ㅇㅇ";
    // public CharacterType selectedCharacter = CharacterType.HARUHI;

    [SerializeField]Image readySprite;
    [SerializeField]Text nameText;
    [SerializeField]Image charPortrait;
    Image teamColorImage;
    string playerName = "ㅇㅇ";
    CharacterType selectedCharacter = CharacterType.HARUHI;
    Team myTeam = Team.HOME;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        teamColorImage = GetComponent<Image>();
        teamColorImage.enabled = false;
    }
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        playerName = pv.Owner.NickName;
        isReady = false;
        UpdateUI();
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_JOINED, new EventObject() { stringObj=pv.Owner.UserId, goData = gameObject });
    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);

    }

    private void OnGamemodeChanged(EventObject arg0)
    {
        if (pv.IsMine) {
            GameMode currMode = (GameMode)arg0.objData;
            if (currMode == GameMode.TEAM) {
                myTeam = (ConnectedPlayerManager.GetMyIndex() % 2 == 0) ? Team.HOME : Team.AWAY;
            }
            pv.RPC("SetTeam", RpcTarget.AllBuffered, (int)myTeam);
        }

    }
    [PunRPC]
    public void SetTeam(int teamNumber)
    {
        myTeam =(Team)teamNumber;
        UpdateUI();
    }
    [PunRPC]
    public void ToggleTeam()
    {
        myTeam = (myTeam == Team.HOME) ? Team.AWAY : Team.HOME;
        UpdateUI();
    }
    [PunRPC]
    public void ChangeCharacter(int character)
    {
        selectedCharacter = (CharacterType)character;
        UpdateUI();
    }

    [PunRPC]
    public void ChangeName(string text)
    {
        playerName = text;
        UpdateUI();
    }

    [PunRPC]
    public void ToggleReady()
    {
        isReady = !isReady;
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_TOGGLE_READY, new EventObject());
        UpdateUI();
    }


    public bool GetReady() {
        return isReady;
    }
    public void UpdateUI()
    {
        nameText.text = playerName;
        readySprite.color = (isReady) ? Color.green : Color.black;
        charPortrait.sprite = GameSession.unitDictionary[selectedCharacter].portraitImage;

        if (GameSession.gameMode == GameMode.TEAM)
        {
            teamColorImage.enabled = true;
            teamColorImage.color = ConstantStrings.GetColorByHex(ConstantStrings.team_color[myTeam == Team.HOME ? 0 : 1]);
        }
        else
        {
            teamColorImage.enabled = false;
        };

        if (pv.IsMine)
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
            hash.Add("CHARACTER", selectedCharacter);
            hash.Add("TEAM", myTeam);
            pv.Owner.SetCustomProperties(hash);
        }

    }
}
