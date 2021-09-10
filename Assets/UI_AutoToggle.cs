using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AutoToggle : MonoBehaviour
{
   [SerializeField] Text text;
   [SerializeField] GameObject buttonObj;
    private void OnEnable()
    {
        if (!GameSession.auto_drive_enabled && !GameSession.jeopdae_enabled)
        {
            buttonObj.SetActive(false);
            return;
        }
        else {
            buttonObj.SetActive(true);
            UpdateUI();
        
        }
    }
    public void OnClickToggle() {
        GameSession.auto_drive_toggled = !GameSession.auto_drive_toggled;
        UpdateUI();
    }

    void UpdateUI() {
        text.text = (GameSession.auto_drive_toggled) ? "ON" : "OFF";
    }
}
