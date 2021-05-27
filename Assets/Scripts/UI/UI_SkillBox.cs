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
    [SerializeField] Text stackText;
    [SerializeField] Text colltimeText;
    public void SetSkillInfo(SkillManager skm) {
        skill = skm;
        portrait.sprite = ConfigsManager.unitDictionary[skm.myCharacter].portraitImage;
        desc.text = ConfigsManager.unitDictionary[skm.myCharacter].txt_skill_desc;
    
    }
    private void FixedUpdate()
    {
        if (skill == null) return;
        UpdateCooltime();
    }

    public void UpdateCooltime() {
        double remain = skill.remainingStackTime;
        double perc = remain / skill.cooltime;
        fillSprite.fillAmount =(float) perc;
        colltimeText.text = (skill.currStack == skill.maxStack) ? " " :remain.ToString("0.0");
        stackText.text = skill.currStack + "/" + skill.maxStack;    
    }
}
