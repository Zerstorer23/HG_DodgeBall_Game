﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_HUD : MonoBehaviour
{
    [SerializeField] Image HP_fillSprite;
   [SerializeField] Image MP_fillCooltime;
   [SerializeField] Image MP_fillStack;
   [SerializeField] Image teamIndicator;
   [SerializeField] Text hpText;
   [SerializeField] Text nameText;
   [SerializeField] Unit_Player player;
    float cooltime;
    int maxLife;
    SkillManager skill;
    private void OnEnable()
    {
        bool isTeamGame = GameSession.gameMode == GameMode.TEAM;
        if (player.pv.IsMine)
        {
            nameText.enabled = false;
        }
        else
        {
            nameText.enabled = true;
            nameText.text = player.pv.Owner.NickName;
        }
        if (isTeamGame)
        {
            teamIndicator.enabled = true;
            teamIndicator.color = ConstantStrings.GetColorByHex(ConstantStrings.team_color[player.myTeam==Team.HOME ? 0 : 1]);
        }
        else
        {
            teamIndicator.enabled = false;
        }

        skill = player.skillManager;
        maxLife = player.health.GetMaxLife();
    }

    private void FixedUpdate()
    {
        SetMP();
        SetHP();
    }

    private void SetHP()
    {
        HP_fillSprite.fillAmount = (float)player.health.currentLife / maxLife;
        hpText.text = player.health.currentLife.ToString("0");
    }

    private void SetMP()
    {
        float stackFill = (float)skill.currStack / skill.maxStack;
        MP_fillStack.fillAmount = stackFill;
        float coolFill = 1 - (skill.remainingStackTime / skill.GetCoolTime());
        MP_fillCooltime.fillAmount = stackFill + (1f / player.skillManager.maxStack) * coolFill;
    }
}