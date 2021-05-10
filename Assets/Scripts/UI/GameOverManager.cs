using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviourPun
{
    [SerializeField] Text returnMenuText;
    [Header("Winner")]
    [SerializeField] Text winnerName;
    [SerializeField] Image winnerImage;
    [Header("Sub")]
    [SerializeField] GameObject subWinnerPanel;
    [SerializeField] Text subWinnerName, subWinnerTitle;
    [SerializeField] Image subWinnerImage;

    [Header("Minigame")]
    [SerializeField] Text miniWinnerName, miniWinnerTitle;
    [SerializeField] Image miniWinnerImage;
    Player finalWinner;
    public PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public double startTime;
    public double timeoutWait = -1;


    public void SetPanel(string winnerID)
    {
        startTime = PhotonNetwork.Time;
        timeoutWait = 5;
        if (winnerID != null)
        {
            finalWinner = ConnectedPlayerManager.GetPlayerByID(winnerID);
            if (GameSession.gameMode == GameMode.TEAM)
            {
                Team winnerTeam = (Team)finalWinner.CustomProperties["TEAM"];
                Team myTeam = (Team)PhotonNetwork.LocalPlayer.CustomProperties["TEAM"];
                if (winnerTeam == myTeam)
                {
                    StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_WINS, 1);
                    finalWinner = PhotonNetwork.LocalPlayer;
                }
            }
            else if (winnerID == PhotonNetwork.LocalPlayer.UserId)
            {

                StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_WINS, 1);


            }


        }
        SetWinner();
        SetSubWinner();
        SetMiniWinner();
    }
    private void Update()
    {
        if (timeoutWait <= 0) return;
        double remain = (startTime + timeoutWait) - PhotonNetwork.Time;
        if (remain <= 0)
        {
            timeoutWait = -1f;
            StartCoroutine(WaitAndOut());
        }
        else
        {
            returnMenuText.text = remain.ToString("0") + " 초후 돌아갑니다...";
        }
    }
    IEnumerator WaitAndOut()
    {
        PhotonNetwork.RemoveRPCs(PhotonNetwork.LocalPlayer);
        yield return new WaitForFixedUpdate();
        if (PhotonNetwork.IsMasterClient)
        {
            pv.RPC("ShowPanel", RpcTarget.All);
        }

    }
    [PunRPC]
    void ShowPanel()
    {
        EventManager.TriggerEvent(MyEvents.EVENT_SHOW_PANEL, new EventObject() { objData = ScreenType.PreGame });
    }


    public GameObject minigamePanel;
    private void SetMiniWinner()
    {
        string winnerUID = StatisticsManager.GetHighestPlayer(StatTypes.MINIGAME);
        if (winnerUID == null)
        {
            minigamePanel.SetActive(false);
        }
        else
        {
            minigamePanel.SetActive(true);
            int score = StatisticsManager.GetStat(StatTypes.MINIGAME, winnerUID);
            Player player = ConnectedPlayerManager.GetPlayerByID(winnerUID);
            CharacterType character = GetPlayerCharacter(player);
            miniWinnerName.text = player.NickName;
            miniWinnerImage.sprite = GameSession.unitDictionary[character].portraitImage;
            miniWinnerTitle.text = string.Format("눈치병신: {0}패", score.ToString());
        }
    }

    private void SetSubWinner()
    {
        string winnerUID = StatisticsManager.GetHighestPlayer(StatTypes.SCORE);
        Player player = ConnectedPlayerManager.GetPlayerByID(winnerUID);
        if (player == null)
        {
            subWinnerPanel.SetActive(false);
        }
        else
        {

            subWinnerPanel.SetActive(true);
            CharacterType character = GetPlayerCharacter(player);
            subWinnerName.text = player.NickName;
            subWinnerImage.sprite = GameSession.unitDictionary[character].portraitImage;


            int score = StatisticsManager.GetStat(StatTypes.SCORE, winnerUID);
            int kill = StatisticsManager.GetStat(StatTypes.KILL, winnerUID);
            int evade = StatisticsManager.GetStat(StatTypes.EVADE, winnerUID);
            subWinnerTitle.text = string.Format("최고득점: {0}점 {1}킬 {2}회피", score.ToString(), kill.ToString(), evade.ToString());
        }

    }

    public CharacterType GetPlayerCharacter(Player player)
    {
        if (!player.CustomProperties.ContainsKey("CHARACTER")) return CharacterType.NONE;
        CharacterType character = (CharacterType)player.CustomProperties["CHARACTER"];
        if (character == CharacterType.NONE)
        {
            if (!player.CustomProperties.ContainsKey("ACTUAL_CHARACTER")) return CharacterType.NONE;
            character = (CharacterType)player.CustomProperties["ACTUAL_CHARACTER"];
        }
        return character;
    }

    private void SetWinner()
    {
        if (finalWinner != null)
        {

            CharacterType character = GetPlayerCharacter(finalWinner);
            winnerName.text = finalWinner.NickName;
            winnerImage.sprite = GameSession.unitDictionary[character].portraitImage;
            if (GameSession.gameMode == GameMode.TEAM)
            {
                if (!finalWinner.CustomProperties.ContainsKey("TEAM")) return ;
                Team winnerTeam = (Team)finalWinner.CustomProperties["TEAM"];
                winnerName.color = ConstantStrings.GetColorByHex(ConstantStrings.team_color[winnerTeam == Team.HOME ? 0 : 1]);
                winnerName.text = string.Format("{0}님의 {1}팀", finalWinner.NickName, ConstantStrings.team_name[winnerTeam == Team.HOME ? 0 : 1]);
            }
            else
            {
                winnerName.color = ConstantStrings.GetColorByHex("#3AFF00");
            }
        }
        else
        {
            winnerName.text = "무승부...";
        }
    }


}
