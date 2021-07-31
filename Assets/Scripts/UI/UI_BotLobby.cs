using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;
public class UI_BotLobby : MonoBehaviourPunCallbacks
{
    List<HUD_UserName> botPanels = new List<HUD_UserName>();
    UI_PlayerLobbyManager lobbyManager;
    private void Awake()
    {
        lobbyManager = GetComponent<UI_PlayerLobbyManager>();
    }



    private new void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StopListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_JOINED, OnGamemodeChanged);
        EventManager.StopListening(MyEvents.EVENT_PLAYER_LEFT, OnGamemodeChanged);
    }
    private new void OnEnable()
    {
        botPanels.Clear();
        EventManager.StartListening(MyEvents.EVENT_GAME_STARTED, OnGameStarted);
        EventManager.StartListening(MyEvents.EVENT_GAMEMODE_CHANGED, OnGamemodeChanged);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_JOINED, OnGamemodeChanged);
        EventManager.StartListening(MyEvents.EVENT_PLAYER_LEFT, OnGamemodeChanged);
    }
    private void OnGameStarted(EventObject arg0)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (var panel in botPanels)
            {
                PhotonNetwork.Destroy(panel.pv);
            }
        }
    }
    private void OnGamemodeChanged(EventObject arg0)
    {
        GameModeConfig info = GameSession.gameModeInfo;
        if (!info.CheckBotGame())
        {
            foreach (var panel in botPanels)
            {
                lobbyManager.RemovePlayer(panel.controller.uid); 
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(panel.pv);
                }
            }
            botPanels.Clear();
            if (PhotonNetwork.IsMasterClient) {
                photonView.RPC("RemoveAllBots", RpcTarget.AllBuffered);
            }
        }
    }
    public void DestoryBotsPanel() {
        if (PhotonNetwork.IsMasterClient) {

            foreach (var entry in botPanels)
            {
                PhotonNetwork.Destroy(entry.gameObject);
            }
        }
        botPanels.Clear();
    }

    [PunRPC]
    public void RemoveAllBots()
    {
        PlayerManager.RemoveAllBots();

    }


    int maxBots = 6;
    public void OnAddBot()
    {
        if (!PhotonNetwork.IsMasterClient || !GameSession.gameModeInfo.CheckBotGame())
        {
            return;
        }
        if (PlayerManager.GetBotCount()+1 > maxBots) return;
        string newID = PlayerManager.PollBotID();
        photonView.RPC("AddBotPlayer", RpcTarget.AllBuffered, newID);
    }
    public void OnRemoveBot()
    {
        if (!PhotonNetwork.IsMasterClient )
        {
            return;
        }
        UniversalPlayer[] bots = PlayerManager.GetBotPlayers();
        if (bots.Length > 0)
        {
            photonView.RPC("RemoveBotPlayer", RpcTarget.AllBuffered, bots[0].uid);
        }
    }


    [PunRPC]
    public void AddBotPlayer(string uid)
    {
        UniversalPlayer botPlayer = new UniversalPlayer(uid);
        if (PhotonNetwork.IsMasterClient)
        {
            botPlayer.SetCustomProperties("SEED", UnityEngine.Random.Range(0, 133));
            botPlayer.SetCustomProperties("TEAM", (int)Team.HOME);
            botPlayer.SetCustomProperties("CHARACTER", (int)CharacterType.NONE);
        }
        PlayerManager.AddBotPlayer(botPlayer);
        InstantiateBotPanel(botPlayer);
    }
    

    [PunRPC]
    public void RemoveBotPlayer(string uid)
    {
        Debug.LogWarning("Remove bot " + uid);
        PlayerManager.RemoveBotPlayer(uid);
        for (int i = 0; i < botPanels.Count; i++)
        {
            HUD_UserName panel = botPanels[i];
            if (panel.controller.Equals(uid))
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Destroy(panel.gameObject);
                }
                lobbyManager.RemovePlayer(uid);
                botPanels.RemoveAt(i);
                lobbyManager.RebalanceTeam();
                return;
            }
        }


    }
    private void InstantiateBotPanel(UniversalPlayer botPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var go = PhotonNetwork.Instantiate(PREFAB_STARTSCENE_PLAYERNAME, Vector3.zero, Quaternion.identity, 0, new object[] { true, botPlayer.uid });

        var info = go.GetComponent<HUD_UserName>();
        botPanels.Add(info);
        string name = botPlayer.NickName;
        CharacterType character = botPlayer.GetProperty("CHARACTER", (GameSession.instance.devMode) ? GameSession.instance.debugChara : CharacterType.NONE);
        Team myTeam = botPlayer.GetProperty("TEAM", Team.HOME);
        info.pv.RPC("ChangeName", RpcTarget.AllBuffered, name);
        info.pv.RPC("ChangeCharacter", RpcTarget.AllBuffered, (int)character);
        info.pv.RPC("SetTeam", RpcTarget.AllBuffered, (int)myTeam);
        /*        charSelector.SetInformation(character);
                UpdateReadyStatus();*/
    }
}
