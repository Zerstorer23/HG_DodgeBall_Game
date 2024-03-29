﻿using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;
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

    UniversalPlayer finalWinner;


    public double timeoutWait = -1;


    public void SetPanel(UniversalPlayer receivedWinner)
    {
        timeoutWait = 5;
        finalWinner = receivedWinner;
        if (finalWinner != null)
        {
            if (GameSession.gameModeInfo.isTeamGame)
            {
                Team winnerTeam = finalWinner.GetProperty<Team>("TEAM");
                Team myTeam = (Team)PhotonNetwork.LocalPlayer.CustomProperties["TEAM"];
                if (winnerTeam == myTeam)
                {
                    StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_WINS, 1);
                    finalWinner = PlayerManager.GetPlayerByID(PhotonNetwork.LocalPlayer.UserId);
                }
            }
            else if (finalWinner.uid == PhotonNetwork.LocalPlayer.UserId)
            {
                StatisticsManager.instance.AddToLocalStat(ConstantStrings.PREFS_WINS, 1);
            }


        }
        SetWinnerInfo();
        SetScoreInfo();
        SetGameInfo();
        AudioManager.PlayEndingVoice();
       GameSession.instance.StartCoroutine(CheckGoogleEvents());
    }
    private void FixedUpdate()
    {
        timeoutWait -= Time.fixedDeltaTime;
        if (timeoutWait <= 0)
        {
            StartCoroutine(WaitAndOut());
        }
        else
        {
            returnMenuText.text =  LocalizationManager.Convert("_end_return_after", timeoutWait.ToString("0"));
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
        float gameTime = (float)(PhotonNetwork.Time - UI_Timer.startTime);
        miniWinnerName.text = LocalizationManager.Convert("_end_seconds",gameTime.ToString("0.0"));
        if (GameSession.gameModeInfo.gameMode == GameMode.PVE)
        {
            float prevScore = PlayerPrefs.GetFloat(PREFS_TIME_RECORD, 0f);
            GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_pve_time, (int)gameTime);
            if (gameTime > prevScore)
            {
                miniWinnerName.text = LocalizationManager.Convert("_end_new_records", gameTime.ToString("0.0"));
                try
                {
                    PlayerPrefs.SetFloat(PREFS_TIME_RECORD, prevScore);
                    PlayerPrefs.Save();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e.Message);
                }
            }
        }
    }
    private void SetScoreInfo()
    {
        string winnerUID = StatisticsManager.GetHighestPlayer(StatTypes.SCORE);
        UniversalPlayer player = PlayerManager.GetPlayerByID(winnerUID);
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
            subWinnerTitle.text = LocalizationManager.Convert("_end_highest_scores", score.ToString(), kill.ToString(), evade.ToString());
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
                if (!finalWinner.HasProperty("TEAM")) return;
                Team winnerTeam = finalWinner.GetProperty< Team>("TEAM");
                winnerName.color = GetColorByHex(team_color[(int)winnerTeam]);
                winnerName.text = LocalizationManager.Convert("_end_assoc_team", finalWinner.NickName, team_name[(int)winnerTeam]);
            }
            else
            {
                winnerName.color = GetColorByHex("#3AFF00");
            }
        }
        else
        {
            winnerName.text = LocalizationManager.Convert("_end_draw");
        }
    }

    private IEnumerator CheckGoogleEvents()
    {
        if (Application.platform != RuntimePlatform.Android) yield break;
#if UNITY_ANDROID
        string localID = PhotonNetwork.LocalPlayer.UserId;
        int kills = StatisticsManager.GetStat(StatTypes.KILL, localID);
        int evades = StatisticsManager.GetStat(StatTypes.EVADE, localID);
        float highestScore = StatisticsManager.GetStat(StatTypes.SCORE, localID);
        string killStatID = null;
        string pickStatID = null;
        string winID = null;
        CharacterType characterType = GameSession.GetPlayerCharacter(PlayerManager.LocalPlayer);
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
        if (killStatID != null) {

            GooglePlayManager.IncrementEvent(killStatID, (uint)kills);
            GooglePlayManager.IncrementEvent(pickStatID, 1);
            yield return new WaitForSeconds(1f);
        }
        GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_evasions, evades);
        GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_kills, kills);
        GooglePlayManager.AddToLeaderboard(GPGSIds.leaderboard_highest_score, (int)highestScore);
        yield return new WaitForSeconds(1f);
        GooglePlayManager.IncrementAchievement(GPGSIds.achievement_total_kills, kills);
        GooglePlayManager.IncrementAchievement(GPGSIds.achievement_total_evades, evades);
        yield return new WaitForSeconds(1f);

        if (finalWinner.IsLocal)
        {
            GooglePlayManager.IncrementAchievement(GPGSIds.achievement_total_wins, 1);
            if (winID != null) {
                GooglePlayManager.IncrementAchievement(winID, 1);
            }
        }
#endif
    }


}
