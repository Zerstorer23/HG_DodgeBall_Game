using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_HUD : MonoBehaviour
{
    [SerializeField] Image HP_fillSprite;
   [SerializeField] Image MP_fillSprite;
   [SerializeField] Image teamIndicator;
   [SerializeField] Text hpText;
   [SerializeField] Text nameText;
   [SerializeField] Unit_Player player;
    float cooltime;
    int maxLife;
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

        cooltime = player.skillManager.GetCoolTime();

    }

    private void FixedUpdate()
    {
        SetMP();
        SetHP();
    }

    private void SetHP()
    {
        HP_fillSprite.fillAmount = (float)player.health.currentLife / player.health.GetMaxLife();
        hpText.text = player.health.currentLife.ToString("0");
    }

    private void SetMP()
    {
        if (player.skillManager.skillInUse)
        {
            MP_fillSprite.fillAmount = 0;
        }
        else {
            float remain = (float)player.skillManager.GetRemainingTime();
            cooltime = player.skillManager.GetCoolTime();
            //    Debug.Log("remain " + remain + " / " + cooltime + " = " + (Mathf.Max(0, remain) / cooltime));
            MP_fillSprite.fillAmount = 1 - ((Mathf.Max(0, remain)) / cooltime);
        }
    }
}
