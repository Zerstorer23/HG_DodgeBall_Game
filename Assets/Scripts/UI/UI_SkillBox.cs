using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillBox : MonoBehaviour
{
    SkillManager skill;
    [SerializeField] Image portrait;
    [SerializeField] Image fillSprite;
    [SerializeField] Text desc;
    [SerializeField] Text colltimeText;
    GameSession session;
    public void SetSkillInfo(SkillManager skm) {
        skill = skm;
        portrait.sprite = GameSession.unitDictionary[skm.myCharacter].portraitImage;
        desc.text = GameSession.unitDictionary[skm.myCharacter].txt_skill_desc;
    
    }
    private void Update()
    {
        if (skill == null) return;
        UpdateCooltime();
    }

    public void UpdateCooltime() {
        double remain = Math.Max(skill.GetRemainingTime(), 0);
        double perc = remain / skill.GetCoolTime();
        fillSprite.fillAmount =(float) perc;
        colltimeText.text = remain.ToString("0.0");
    
    }
}
