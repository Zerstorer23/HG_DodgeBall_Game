using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MobileAuto : MonoBehaviour
{
    // Start is called before the first frame update

    public void OnClick_Auto (){

        if (!GameSession.auto_drive_enabled) return;
        GameSession.auto_drive_toggled = !GameSession.auto_drive_toggled;
    }
}
