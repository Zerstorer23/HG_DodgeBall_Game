using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_AimOption : MonoBehaviour
{
    public static bool aimManual = false;
    Toggle toggle;
    public void Initialise() {
        toggle = GetComponent<Toggle>();

        aimManual = PlayerPrefs.GetInt(ConstantStrings.PREFS_MANUAL_AIM, 0) != 0;
        toggle.isOn = aimManual;
    }
    public void OnToggleChanged() {
        aimManual = toggle.isOn;
        PlayerPrefs.SetInt(ConstantStrings.PREFS_MANUAL_AIM, (aimManual) ? 1 : 0);
    }
}
