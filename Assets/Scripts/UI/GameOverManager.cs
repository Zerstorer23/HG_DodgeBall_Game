using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviourPunCallbacks
{
   [SerializeField] GameObject panel;
   [SerializeField] Text returnMenuText;
    [Header("Winner")]
    [SerializeField] Text winnerName;
    [SerializeField] Image winnerImage;
    [Header("Sub")]
    [SerializeField] Text subWinnerName, subWinnerTitle;
    [SerializeField] Image subWinnerImage;

    [Header("Minigame")]
    [SerializeField] Text miniWinnerName,miniWinnerTitle;
    [SerializeField] Image miniWinnerImage;
    Player finalWinner;
    bool gameFinished = false;
    public PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public float timeoutWait = 5f;
     float remainTime = 5f;


    [PunRPC]
    public void ShowPanel(string winnerID) {
        if (gameFinished) return;
        if (winnerID != null) {
            finalWinner = ConnectedPlayerManager.GetPlayerByID(winnerID);
        }
        EventManager.TriggerEvent(MyEvents.EVENT_GAME_FINISHED, null);

        gameFinished = true;
        panel.SetActive(true);
        SetWinner();
        SetSubWinner();
        SetMiniWinner();
        remainTime = timeoutWait;
    }

    public GameObject minigamePanel;
    private void SetMiniWinner()
    {
        string winnerUID = StatisticsManager.GetHighestPlayer(StatTypes.MINIGAME);
        if (winnerUID == null) {
            minigamePanel.SetActive(false);
        }
        else {
            minigamePanel.SetActive(true);
            int score = StatisticsManager.GetStat(StatTypes.MINIGAME, winnerUID);
            Player player = ConnectedPlayerManager.GetPlayerByID(winnerUID);
            CharacterType character = (CharacterType)player.CustomProperties["CHARACTER"];
            miniWinnerName.text = player.NickName;
            miniWinnerImage.sprite = EventManager.unitDictionary[character].portraitImage;
            miniWinnerTitle.text = string.Format("눈치병신: {0}패", score.ToString());
        }
    }

    private void SetSubWinner()
    {
        string winnerUID = StatisticsManager.GetHighestPlayer(StatTypes.SCORE);
        Player player = ConnectedPlayerManager.GetPlayerByID(winnerUID);
        CharacterType character = (CharacterType)player.CustomProperties["CHARACTER"];
        subWinnerName.text = player.NickName;
        subWinnerImage.sprite = EventManager.unitDictionary[character].portraitImage;


        int score = StatisticsManager.GetStat(StatTypes.SCORE, winnerUID);
        int kill = StatisticsManager.GetStat(StatTypes.KILL, winnerUID);
        int evade = StatisticsManager.GetStat(StatTypes.EVADE, winnerUID);
        subWinnerTitle.text = string.Format("최고득점: {0}점 {1}킬 {2}회피", score.ToString(), kill.ToString(), evade.ToString());

    }
    private void Update()
    {
        if (!gameFinished) return;
        remainTime -= Time.deltaTime;
        returnMenuText.text = remainTime.ToString("0") + " 초후 메뉴로 돌아갑니다...";
        if (remainTime <= 0 && PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            gameFinished = false;
            Debug.Log("Leave room");
            PhotonNetwork.RemoveBufferedRPCs();
            PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnLeftRoom()
    {
       PhotonNetwork.LoadLevel(0);
    }
    private void SetWinner()
    {
        if (finalWinner != null) {

            CharacterType character = (CharacterType)finalWinner.CustomProperties["CHARACTER"];
            winnerName.text = finalWinner.NickName;
            winnerImage.sprite = EventManager.unitDictionary[character].portraitImage;
        }
        else {
            winnerName.text = "무승부...";
        }
    }

    public bool IsOver() {
        return gameFinished;
    }
}
