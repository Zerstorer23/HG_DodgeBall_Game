using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TournamentPanel : MonoBehaviour
{
    [SerializeField] Text winners;
    [SerializeField] Text losers;
    [SerializeField] Text nextOpponent;
    [SerializeField] Text returnText;

    double startTime;
    double timeoutWait;
    public void SetPanel(Player[] win,float delay)
    {
        startTime = PhotonNetwork.Time;
        timeoutWait = delay;
        string text = "";
        foreach (var p in win) {
            if (p.IsLocal)
            {
                text += "<color=#ff00ff>" + p.NickName + "</color> ";
            }
            else {

                text += p.NickName + " ";
            }
        }
        winners.text = text;
    }
    public void SetNext(Dictionary<int, List<Player>> dict) {
        
        nextOpponent.text = "";
        if (GameSession.LocalPlayer_FieldNumber != -1)
        {
            foreach (var p in dict[GameSession.LocalPlayer_FieldNumber]) {
                if (!p.IsLocal) {
                    nextOpponent.text = p.NickName;
                    return;
                }
            }
            
        }
    }

    private void FixedUpdate()
    {
        if (timeoutWait <= 0) return;
        double remain = (startTime + timeoutWait) - PhotonNetwork.Time;
        if (remain <= 0)
        {
            timeoutWait = -1f;
            gameObject.SetActive(false);
        }
        else
        {
            returnText.text = remain.ToString("0") + " 초후 다음 경기가 시작됩니다...";
        }
    }
}
