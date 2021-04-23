using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_HUD : MonoBehaviour
{
   [SerializeField] Image HP_fillSprite;
   [SerializeField] Image MP_fillSprite;
   [SerializeField] Text nameText;
   [SerializeField] Unit_Player player;
    float cooltime;
    int fullHP;
    private void OnEnable()
    {
        if (!player.pv.IsMine) {
            nameText.text = player.pv.Owner.NickName;
        }
        cooltime = player.skillManager.GetCoolTime();
    }
    private void Start()
    {
        fullHP = player.health.GetMaxLife();
    }
    private void Update()
    {
        SetMP();
        SetHP();
    }

    private void SetHP()
    {
        HP_fillSprite.fillAmount = (float)player.health.currentLife / fullHP;
    }

    private void SetMP()
    {
       
        float remain = (float)player.skillManager.GetRemainingTime();
        cooltime = player.skillManager.GetCoolTime();
    //    Debug.Log("remain " + remain + " / " + cooltime + " = " + (Mathf.Max(0, remain) / cooltime));
        MP_fillSprite.fillAmount = 1-((Mathf.Max(0, remain)) / cooltime);
    }
}
