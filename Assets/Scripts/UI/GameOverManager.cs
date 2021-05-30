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
        CheckGoogleEvents();
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
            GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_pve_time, (int)gameTime);
            if (gameTime > prevScore) {
                miniWinnerName.text = string.Format("<color=#ff00ff>[신기록]{0}초</color>", gameTime.ToString("0.0"));
                PlayerPrefs.SetFloat(ConstantStrings.PREFS_TIME_RECORD, prevScore);
                PlayerPrefs.Save();
            }
        }
    }
    private void SetScoreInfo()
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
            CharacterType character = GameSession.GetPlayerCharacter(player);
            subWinnerName.text = player.NickName;
            subWinnerImage.sprite = ConfigsManager.unitDictionary[character].portraitImage;


            int score = StatisticsManager.GetStat(StatTypes.SCORE, winnerUID);
            int kill = StatisticsManager.GetStat(StatTypes.KILL, winnerUID);
            int evade = StatisticsManager.GetStat(StatTypes.EVADE, winnerUID);
            subWinnerTitle.text = string.Format("최고득점: {0}점 {1}킬 {2}회피", score.ToString(), kill.ToString(), evade.ToString());
        }

    }



    private void SetWinnerInfo()
    {
        Debug.Log("Received winner " + finalWinner);
        if (finalWinner != null)
        {

            CharacterType character = GameSession.GetPlayerCharacter(finalWinner);
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

    private void CheckGoogleEvents()
    {
        if (Application.platform != RuntimePlatform.Android) return;
        string localID = PhotonNetwork.LocalPlayer.UserId;
        int kills = StatisticsManager.GetStat(StatTypes.KILL, localID);
        int evades = StatisticsManager.GetStat(StatTypes.EVADE, localID);
        GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_evasions, evades);
        GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_kills, kills);
        GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_highest_score, StatisticsManager.GetStat(StatTypes.SCORE, localID));
        GooglePlayManager.IncrementAchievement(GPGSIds.achievement_total_kills, kills);
        GooglePlayManager.IncrementAchievement(GPGSIds.achievement_total_evades, evades);
        CharacterType characterType = GameSession.GetPlayerCharacter(PhotonNetwork.LocalPlayer);
        string killStatID = null;
        string pickStatID = null;
        string winID = null;
        switch (characterType)
        {
            case CharacterType.NAGATO:
                killStatID = GPGSIds.event_nagato_kill;
                pickStatID = GPGSIds.event_nagato_pick;
                winID = GPGSIds.achievement_nagato_win;
                break;
            case CharacterType.HARUHI:
                killStatID = GPGSIds.event_haruhi_kill;
                pickStatID = GPGSIds.event_haruhi_picks;
                winID = GPGSIds.achievement_haruhi_win;
                break;
            case CharacterType.MIKURU:
                killStatID = GPGSIds.event_mikuru_kill;
                pickStatID = GPGSIds.event_mikuru_pick;
                winID = GPGSIds.achievement_mikuru_win;
                break;
            case CharacterType.KOIZUMI:
                killStatID = GPGSIds.event_koizumi_kill;
                pickStatID = GPGSIds.event_koizumi_pick;
                winID = GPGSIds.achievement_koizumi_win;
                break;
            case CharacterType.KUYOU:
                killStatID = GPGSIds.event_kuyou_kill;
                pickStatID = GPGSIds.event_kuyou_pick;
                winID = GPGSIds.achievement_kuyou_win;
                break;
            case CharacterType.ASAKURA:
                killStatID = GPGSIds.event_asakura_kill;
                pickStatID = GPGSIds.event_asakura_pick;
                winID = GPGSIds.achievement_asakura_win;
                break;
            case CharacterType.KYOUKO:
                killStatID = GPGSIds.event_kyouko_kill;
                pickStatID = GPGSIds.event_kyouko_pick;
                winID = GPGSIds.achievement_kyouko_win;
                break;
            case CharacterType.KIMIDORI:
                killStatID = GPGSIds.event_kimidori_kill;
                pickStatID = GPGSIds.event_kimidori_pick;
                winID = GPGSIds.achievement_kimidori_win;
                break;
/*            case CharacterType.KYONMOUTO:
                killStatID = GPGSIds.event_haruhi_kill;
                pickStatID = GPGSIds.event_haruhi_picks;
                break;*/
            case CharacterType.SASAKI:
                killStatID = GPGSIds.event_sasaki_kill;
                pickStatID = GPGSIds.event_sasaki_pick;
                winID = GPGSIds.achievement_sasaki_win;
                break;
            case CharacterType.TSURUYA:
                killStatID = GPGSIds.event_tsuruya_kill;
                pickStatID = GPGSIds.event_tsuruya_pick;
                winID = GPGSIds.achievement_tsuruya_win;
                break;
            case CharacterType.KOIHIME:
                killStatID = GPGSIds.event_koizumi_kill;
                pickStatID = GPGSIds.event_koizumi_pick;
                winID = GPGSIds.achievement_koizumi_win;
                break;
            case CharacterType.YASUMI:
                killStatID = GPGSIds.event_haruhi_kill;
                pickStatID = GPGSIds.event_haruhi_picks;
                winID = GPGSIds.achievement_yasumi_win;
                break;
        }
        GooglePlayManager.IncrementEvent(killStatID, (uint)kills);
        GooglePlayManager.IncrementEvent(pickStatID, 1);
        if (finalWinner == PhotonNetwork.LocalPlayer) {
            GooglePlayManager.IncrementAchievement(GPGSIds.achievement_total_wins, 1);
            GooglePlayManager.IncrementAchievement(winID, 1);
        }
    }


}
