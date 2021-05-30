using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD_UserName : MonoBehaviourPun
{
    public PhotonView pv;
    public bool isReady = false;

    [SerializeField] Image readySprite;
    [SerializeField] Text nameText;
    [SerializeField] Image charPortrait;
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
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_JOINED, new EventObject() { stringObj = pv.Owner.UserId, goData = gameObject });
    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);

    }

    private void OnGamemodeChanged(EventObject arg0)
    {
        if (pv.IsMine) {
            GameModeConfig currMode = (GameModeConfig)arg0.objData;
            if (currMode.isTeamGame) {
                myTeam = (ConnectedPlayerManager.GetMyIndex(PhotonNetwork.PlayerList) % 2 == 0) ? Team.HOME : Team.AWAY;
            }
            pv.RPC("SetTeam", RpcTarget.AllBuffered, (int)myTeam);
        }

    }
    [PunRPC]
    public void SetTeam(int teamNumber)
    {
        myTeam = (Team)teamNumber;
        if (pv.IsMine)
        {
            PushPlayerSetting(pv.Owner, "TEAM", myTeam);
        }
        UpdateUI();
    }
    [PunRPC]
    public void ToggleTeam()
    {
        myTeam = (myTeam == Team.HOME) ? Team.AWAY : Team.HOME;
        if (pv.IsMine)
        {
            PushPlayerSetting(pv.Owner, "TEAM", myTeam);
        }
        UpdateUI();
    }
    [PunRPC]
    public void ChangeCharacter(int character)
    {
        selectedCharacter = (CharacterType)character;
        if (pv.IsMine) {
            PushPlayerSetting(pv.Owner, "CHARACTER", selectedCharacter);
        }
        UpdateUI();
    }

    [PunRPC]
    public void ChangeName(string text)
    {
        playerName = text;
        if (pv.IsMine) {
            PhotonNetwork.LocalPlayer.NickName = playerName;
        }
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
        charPortrait.sprite = ConfigsManager.unitDictionary[selectedCharacter].portraitImage;

        if (GameSession.gameModeInfo.isTeamGame)
        {
            teamColorImage.enabled = true;
            teamColorImage.color = ConstantStrings.GetColorByHex(ConstantStrings.team_color[myTeam == Team.HOME ? 0 : 1]);
        }
        else
        {
            teamColorImage.enabled = false;
        };
    }
    public static void PushPlayerSetting(Player p, string key, object value) {
        ExitGames.Client.Photon.Hashtable hash = new ExitGames.Client.Photon.Hashtable();
        hash.Add(key, value);
        p.SetCustomProperties(hash);
        
    } 
}
