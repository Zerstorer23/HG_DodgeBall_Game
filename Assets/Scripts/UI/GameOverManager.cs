using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
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
    [SerializeField] Text miniWinnerName;

    Player finalWinner;


    public double startTime;
    public double timeoutWait = -1;


    public void SetPanel(Player receivedWinner)
    {
        startTime = PhotonNetwork.Time;
        timeoutWait = 5;
        finalWinner = receivedWinner;
        if (finalWinner != null)
        {
            if (GameSession.gameModeInfo.isTeamGame)
            {
                Team winnerTeam = (Team)finalWinner.CustomProperties["TEAM"];
                Team myTeam = (Team)PhotonNetwork.LocalPlayer.CustomProperties["TEAM"];
                if (winnerTeam == myTeam)
                {
                    StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_WINS, 1);
                    finalWinner = PhotonNetwork.LocalPlayer;
                }
            }
            else if (finalWinner.UserId == PhotonNetwork.LocalPlayer.UserId)
            {
                StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_WINS, 1);
            }


        }
        SetWinnerInfo();
        SetScoreInfo();
        SetGameInfo();
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
            GameSession.ShowMainMenu();
        }

    }


    private void SetGameInfo()
    {
        float gameTime =(float) (PhotonNetwork.Time - UI_Timer.startTime);
        miniWinnerName.text = string.Format("{0}초", gameTime.ToString("0.0"));
        if (GameSession.gameModeInfo.gameMode == GameMode.PVE) { 
            float prevScore = PlayerPrefs.GetFloat(ConstantStrings.PREFS_TIME_RECORD, 0f);
            if (gameTime > prevScore) {
                miniWinnerName.text = string.Format("<color=#ff00ff>[신기록]{0}초</color>", gameTime.ToString("0.0"));
                PlayerPrefs.SetFloat(ConstantStrings.PREFS_TIME_RECORD, prevScore);
            }
        }
    }

    /*
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
            miniWinnerImage.sprite = ConfigsManager.unitDictionary[character].portraitImage;
            miniWinnerTitle.text = string.Format("눈치병신: {0}패", score.ToString());
        }
     */
    private void SetScoreInfo()
    {
        string winnerUID = StatisticsManager.GetHighestPlayer(StatTypes.SCORE);
        Debug.Log("Subwinner id " + winnerUID);
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
            subWinnerImage.sprite = ConfigsManager.unitDictionary[character].portraitImage;


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

    private void SetWinnerInfo()
    {
        Debug.Log("Received winner " + finalWinner);
        if (finalWinner != null)
        {

            CharacterType character = GetPlayerCharacter(finalWinner);
            winnerName.text = finalWinner.NickName;
            winnerImage.sprite = ConfigsManager.unitDictionary[character].portraitImage;
            if (GameSession.gameModeInfo.isTeamGame)
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
