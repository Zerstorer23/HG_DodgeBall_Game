using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;
using Random = UnityEngine.Random;

public class HUD_UserName : MonoBehaviourPun
{
    public PhotonView pv;
    public bool isReady = false;
    public UI_PlayerLobbyManager lobbyManager;
    [SerializeField] Image readySprite;
    [SerializeField] Text nameText;
    [SerializeField] Image charPortrait;
    Image teamColorImage;
    string playerName = "ㅇㅇ";
    CharacterType selectedCharacter = CharacterType.HARUHI;
    Team myTeam = Team.HOME;
    public Controller controller;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        teamColorImage = GetComponent<Image>();
        controller = GetComponent<Controller>();
        teamColorImage.enabled = false;
    }
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StartListening(MyEvents.EVENT_JEOPDAE_ENABLE, CheckAutoReady);
        playerName = controller.Owner.NickName;

        bool isBot = (bool)pv.InstantiationData[0];
        string uid = (string)pv.InstantiationData[1];
        controller.SetControllerInfo(isBot, uid);
        isReady = controller.Owner.IsBot;
        CheckAutoReady();
        UpdateUI();
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_JOINED, new EventObject() { stringObj = uid, goData = gameObject });
    }

    private void CheckAutoReady(EventObject eo = null)
    {
        if (!GameSession.jeopdae_enabled) return;
        StartCoroutine(ReadyCoroutine());
        StartCoroutine(ChooseCharacter());

    }
    IEnumerator ReadyCoroutine() {
        float randTime = Random.Range(2f, 4f);
        yield return new WaitForSeconds(randTime);
        lobbyManager.OnClick_Ready();
    }
    IEnumerator ChooseCharacter() {
        float rand = Random.Range(0f, 1f);
        CharacterType charSelection = ConfigsManager.GetRandomCharacterExcept(CharacterType.MIKURU);
        if (rand <= 0.33)
        {
            float randTime = Random.Range(0.5f, 2f);
            yield return new WaitForSeconds(randTime);
            pv.RPC("ChangeCharacter", RpcTarget.AllBuffered, (int)charSelection);
        }

    }


    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StopListening(MyEvents.EVENT_JEOPDAE_ENABLE, CheckAutoReady);

    }

    private void OnGamemodeChanged(EventObject arg0)
    {
        ResetTeam();
    }
    public void ResetTeam() {
        if (!gameObject.activeInHierarchy) return;
        if (controller.IsMine)
        {
            if (GameSession.gameModeInfo.isTeamGame)
            {
                int index = PlayerManager.GetMyIndex(controller.Owner, PlayerManager.GetPlayers());
                myTeam = (Team)(index % 2 + 1);
            }
            pv.RPC("SetTeam", RpcTarget.AllBuffered, (int)myTeam);
        }
    }

    [PunRPC]
    public void SetTeam(int teamNumber)
    {
        myTeam = (Team)teamNumber;
        if (controller.IsMine)
        {
           controller.Owner.SetCustomProperties("TEAM", myTeam);
        }
        UpdateUI();
    }
    [PunRPC]
    public void ToggleTeam()
    {
        myTeam = (myTeam == Team.HOME) ? Team.AWAY : Team.HOME;
        if (controller.IsMine)
        {
            controller.Owner.SetCustomProperties("TEAM", myTeam);
        }
        UpdateUI();
    }
    [PunRPC]
    public void ChangeCharacter(int character)
    {
        selectedCharacter = (CharacterType)character;
        if (controller.IsMine)
        {
            controller.Owner.SetCustomProperties("CHARACTER", selectedCharacter);
        }
        UpdateUI();
    }

    [PunRPC]
    public void ChangeName(string text)
    {
        playerName = text;
        if (controller.IsMine)
        {
            controller.Owner.NickName = playerName;
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
            teamColorImage.color = GetColorByHex(team_color[(int)myTeam]);
        }
        else
        {
            teamColorImage.enabled = false;
        };
    }

}
