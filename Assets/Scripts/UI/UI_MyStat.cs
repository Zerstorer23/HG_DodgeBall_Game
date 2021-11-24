using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_MyStat : MonoBehaviour
{
    [SerializeField] Text killStat;
    [SerializeField] Text winStat;
    [SerializeField] Text evadeStat;
    // Start is called before the first frame update
    void Start()
    {
        int kills = PlayerPrefs.GetInt(ConstantStrings.PREFS_KILLS,0);  
        int wins = PlayerPrefs.GetInt(ConstantStrings.PREFS_WINS,0);  
        int evades = PlayerPrefs.GetInt(ConstantStrings.PREFS_EVADES,0);

        killStat.text = LocalizationManager.Convert("_stat_kills")+"\t" + kills;
        winStat.text = LocalizationManager.Convert("_stat_wins") + "\t" + wins;
        evadeStat.text = LocalizationManager.Convert("_stat_evades") + "\t" + evades;
    }


}
