﻿using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class Map_CapturePoint : MonoBehaviourPun
{
    Map_CapturePointManager cpManager;
    public int captureIndex;
    public Team owner = Team.NONE;
    public float captureProgress;
    float captureSpeed = 10f; //10
    float captureThreshold = 100f;
    float radius = 7.25f;
    GameField gameField;
    SortedDictionary<string, Unit_Player> players;

    public bool captureByHP = true;
    Color homeColor, awayColor;
    private void Awake()
    {
        gameField = GetComponentInParent<GameField_CP>();
        cpManager = GetComponentInParent<Map_CapturePointManager>();
        homeColor = GetColorByHex(team_color[(int)(Team.HOME) - 1]);
        awayColor = GetColorByHex(team_color[(int)(Team.AWAY) - 1]);
    }
    private void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
  
    }
    private void OnEnable()
    {
        owner = Team.NONE;
        captureProgress = 0;
        UpdateBanner();
        EventManager.StartListening(MyEvents.EVENT_CP_CAPTURED, OnPointCaptured);
    }
    private void OnDisable()
    {
        EventManager.StopListening(MyEvents.EVENT_CP_CAPTURED, OnPointCaptured);
    }

    private void OnPointCaptured(EventObject arg0)
    {
        int captured = arg0.intObj;
        /* if (IsBypassing(captured))
         {
             captureProgress = 0;
             UpdateFill();
         }
         else */
        if (captured == captureIndex)
        {
            capturingText.enabled = false;
            Team team = arg0.Get<Team>();
            if (team == Team.HOME)
            {
                captureProgress = captureThreshold;
            }
            else if (team == Team.AWAY)
            {
                captureProgress = -captureThreshold;
            }
            UpdateFill();
        }
        else {
            Restore();
        }
       // UpdateBanner();
    }

    private bool IsBypassing(int captured)
    {
        if (owner == Team.HOME)
        {
            return (captured < captureIndex);
        }
        else if (owner == Team.AWAY)
        {
            return (captured > captureIndex);
        }
        return false;
    }

   public Team dominantTeam = Team.NONE;
   public int dominantPoint = 0;
    private void FixedUpdate()
    {
        if (IsOpen())
        {
            if (PhotonNetwork.IsMasterClient)
            {
                CheckDominance();
            }
            UpdateProgress();
            CheckCapture();
            UpdateFill();
        }
    }

    private void Restore()
    {
        if (owner == Team.NONE)
        {
            captureProgress = 0f;
        }
        else if (owner == Team.AWAY)
        {
            captureProgress = -captureThreshold;
        }
        else {
            captureProgress = captureThreshold;
        }
        UpdateBanner();
        UpdateFill();
    }

    void CheckDominance() {
        players = gameField.playerSpawner.unitsOnMap;
        Team dominance = Team.NONE;
        int homeHP = 0;
        int awayHP = 0;

        foreach (var player in players)
        {
            if (player.Value == null || !player.Value.gameObject.activeInHierarchy) continue;
            float dist = Vector2.Distance(transform.position, player.Value.transform.position);
            if (dist < radius)
            {
                Player p = gameField.playerSpawner.playersOnMap[player.Key];
                Team team = (Team)ConnectedPlayerManager.GetPlayerProperty(p, "TEAM", Team.NONE);
                if (team == Team.HOME)
                {
                    homeHP += player.Value.health.currentLife;
                }
                else if (team == Team.AWAY)
                {
                    awayHP += player.Value.health.currentLife;
                }

                if (!captureByHP) {
                    if (homeHP > 0 && awayHP > 0) {
                        dominance = Team.NONE;
                        //Neutral deadlock
                        break;
                    }
                }
            }
        }
       
        if (captureByHP) {
            if (homeHP == awayHP)
            {
                dominance = Team.NONE;
            }
            else if (homeHP > awayHP)
            {
                dominance = Team.HOME;
            }
            else
            {
                dominance = Team.AWAY;
            }
        }

        if (dominance == Team.NONE)
        {
            if (dominance != dominantTeam)
            {
                dominantTeam = dominance;
                dominantPoint = 0;
                //Send RPC.. set neutral
                photonView.RPC("ChangeCapturingStatus", RpcTarget.AllBuffered, (int)dominantTeam, dominantPoint);
            }
        } else
        {
            int diff =Math.Abs(homeHP - awayHP);
            if (dominance != dominantTeam || dominantPoint != diff)
            {
                dominantTeam = dominance;
                dominantPoint = diff;
                photonView.RPC("ChangeCapturingStatus", RpcTarget.AllBuffered, (int)dominantTeam, dominantPoint);
            }
        }


    }
    [PunRPC]
    void ChangeCapturingStatus(int majorTeam, int numCapturing) {
        dominantTeam = (Team)majorTeam;
        dominantPoint = numCapturing;
        UpdateBanner();
    }

    [SerializeField] Image ownerFlag, fillSprite, lockImage, boundary;
    [SerializeField] Text capturingText;

   
   public void UpdateBanner() {

        if (owner == Team.NONE)
        {
            ownerFlag.color = Color.gray;
            boundary.color = ownerFlag.color;
        }
        else {
            ownerFlag.color = GetColorByHex(team_color[((int)owner) - 1]);
            boundary.color = ownerFlag.color;
        }
        lockImage.enabled = !IsOpen();
    }
    private void UpdateFill()
    {
        fillSprite.fillAmount = (Mathf.Abs(captureProgress )/ captureThreshold);
    }
    void UpdateProgress() {

        if (dominantTeam == Team.NONE)
        {
            capturingText.enabled = false;
            float amount = 2f* captureSpeed * Time.fixedDeltaTime;
            float max = 0;
            if (owner == Team.AWAY)
            {
                max = -captureThreshold;
            }
            else if (owner == Team.HOME)
            {
                max = captureThreshold;
            }

            if (captureProgress < max)
            {
                captureProgress += amount;
                if (captureProgress > max)
                {
                    captureProgress = max;
                }
            }
            else if(captureProgress > max)
            {
                 captureProgress -= amount;
                if (captureProgress < max)
                {
                    captureProgress = max;
                }
            }
        }
        else
        {
            capturingText.enabled = true;
            float amount = captureSpeed * Time.fixedDeltaTime * dominantPoint;
            if (dominantTeam == Team.HOME && cpManager.IsValidCapturePoint(Team.HOME,captureIndex))
            {
                captureProgress += amount;
            }
            else if (dominantTeam == Team.AWAY && cpManager.IsValidCapturePoint(Team.AWAY, captureIndex))
            {
                captureProgress -= amount;
            }
            if (captureProgress == 0)
            {
                fillSprite.color = Color.gray;
            }
            else if (captureProgress > 0)
            {
                fillSprite.color = homeColor;
            }
            else
            {

                fillSprite.color = awayColor;
            }
        }
    }
    void CheckCapture()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (captureProgress > captureThreshold && owner != Team.HOME) {
            owner = Team.HOME;
            photonView.RPC("NotifyCapture", RpcTarget.AllBuffered, (int)owner);
        }else if(captureProgress < -captureThreshold && owner != Team.AWAY)
        {
            owner = Team.AWAY;
            photonView.RPC("NotifyCapture", RpcTarget.AllBuffered, (int)owner);
        }
        else if (
            (
            (owner == Team.HOME && captureProgress < 0)
            || (owner == Team.AWAY && captureProgress > 0)
            )
            && owner != Team.NONE)
        {
            owner = Team.NONE;
            photonView.RPC("NotifyCapture", RpcTarget.AllBuffered, (int)owner);
        }

    }
    bool IsOpen() {
        if (cpManager.serialCaptureRequired)
        {
            return cpManager.homeNext == captureIndex || cpManager.awayNext == captureIndex;
        }
        else {
            return true;
        }
    
    }

    [PunRPC]
    void NotifyCapture(int newTeam) {
        owner = (Team)newTeam;
        EventManager.TriggerEvent(MyEvents.EVENT_CP_CAPTURED, new EventObject(owner) { intObj = captureIndex });
        if (owner == Team.NONE)
        {
            EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject(string.Format("{0}번 지역이 중립화되었습니다.",(captureIndex+1))));
        }
        else {
            EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject(string.Format("{0}번 지역이 {1}팀에게 점령되었습니다.",(captureIndex+1),team_name[(int)owner-1])));
        }
    }
}