using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour {

    public string key;
    private Text Text;
  //  private TextMeshProUGUI TextPro;
    // Use this for initialization


    private void Awake()
    {
    //    Debug.Log("text init start");

        Text = GetComponent<Text> ();
       // TextPro = GetComponent<TextMeshProUGUI> ();
        key = Text.text;
        key = key.Replace("\r", "");
        
        EventManager.StartListening(MyEvents.EVENT_LOCALIZATION_LOADED, ReloadText);
        if (LocalizationManager.IsReady()) ReloadText(null);
     //   Debug.Log("text init end");
    }
    private void OnDestroy()
    {
        EventManager.StopListening(MyEvents.EVENT_LOCALIZATION_LOADED, ReloadText);
    }

    public void ReloadText(EventObject eo)
    {
        if (Text != null)
        {
            //   Debug.Log("Request local " + gameObject.name + " key " + key+" ready "+LocalizationManager.IsReady());
    /*        if (key.Length <= 1) {
                Debug.LogWarning("OBJ " + this.gameObject.name);
            }*/
            Text.text = LocalizationManager.Convert(key);
            return;
        }
       /* if (TextPro != null)
        {
            TextPro.text = LocalizationManager.Convert (key);
        }*/
    }
}