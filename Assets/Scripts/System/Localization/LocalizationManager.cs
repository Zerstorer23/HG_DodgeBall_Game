using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class LocalizationManager : MonoBehaviour {

    private static LocalizationManager prLocalisationManager;
    public const char VAR_TOKEN = '$'; 
    private Dictionary<string, string> localizedText;

    //public string fileName;

    // Use this for initialization
    public static LocalizationManager instance
    {
        get
        {
            if (!prLocalisationManager)
            {
                prLocalisationManager = FindObjectOfType<LocalizationManager>();
                if (!prLocalisationManager)
                {
                    Debug.LogWarning("There needs to be one active LocalizationManager script on a GameObject in your scene.");
                }
        /*        else
                {
                    instance.Init();
                }*/
            }
            return prLocalisationManager;
        }
    }
    private string missingTextString = "Localized text not found";
    bool isReady = false;

    public static void Init()
    {
        if (instance.localizedText == null)
        {
            instance.localizedText = new Dictionary<string, string>();
            Debug.Log("Initialised");
        }
    }

    public static void LoadLocalizedText()
    {
        if (instance == null) {
            Init();
        }
        string fileName = instance.GetLocalLanguage();
        instance.localizedText = new Dictionary<string, string>();
        //string filePath = Path.Combine(Application.streamingAssetsPath, fileName);
        var jsonTextFile = Resources.Load<TextAsset>("Texts/"+fileName);

        if (jsonTextFile != null)
        {
            string dataAsJson = jsonTextFile.text; //json파일을 읽어서 string으로 뽑음
            LocalizationData loadedData = JsonUtility.FromJson<LocalizationData>(dataAsJson);    //deserialization

            //전체 아이템들에 대해서
            for (int i = 0; i < loadedData.items.Count; i++)
            {
          //      Debug.Log(loadedData.items[i].Output()+" / "+ StringToByte(loadedData.items[i].key));
                instance.localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }

            //localizationText 데이터 불러오기 완료
       //     Debug.Log("Data loaded. Dictionary containts :" + instance.localizedText.Count + " entries");
            instance.isReady = true;
            EventManager.TriggerEvent(MyEvents.EVENT_LOCALIZATION_LOADED, null);
        }
        else
        {
            //파일이 존재하지않음
            Debug.LogError("Cannot find file");
        }

    }

    string GetLocalLanguage() {
        SystemLanguage sl =(SystemLanguage) Application.systemLanguage;
        Debug.Log("LANGUAGE: "+sl);
        if (sl == SystemLanguage.Korean)
        {
            return "text_ko_kr";
        }
        else
        {
            return "text_en_us";

        }

}

    public static string Convert(string key)
    {

        string result = key;// instance.missingTextString;
        if (instance.localizedText.ContainsKey(key))
        {
            result = instance.localizedText[key];
        //    Debug.Log("Found [" + key + "] / " + instance.localizedText.Count + " / " + StringToByte(key));
        }
        else {
            /*    key = Cleanse(key);
              Debug.LogWarning("Cleansed / "+ key+" /" + StringToByte(key));
              if (instance.localizedText.ContainsKey(key))
               {
                   result = instance.localizedText[key];
               }
               else
               {*/
            Debug.LogWarning("Not found [" + key + "] / " + instance.localizedText.Count + " / " + StringToByte(key));
          //  }
        }
        result = result.Replace("\\n", "\n");
        return result;

    }
    public static string Convert(string key, params string[] variables)
    {

        string rawText = key;// instance.missingTextString;
        if (instance.localizedText.ContainsKey(key))
        {
            rawText = instance.localizedText[key];
        }
        else
        {
            Debug.LogWarning("Not found [" + key + "] / " + instance.localizedText.Count + " / " + StringToByte(key));
        }
        string[] splitText = rawText.Split(VAR_TOKEN);
        string resultText = "";
        for (int i = 0; i < splitText.Length; i++) {
            if (i < variables.Length)
            {
                resultText += splitText[i] + variables[i];
            }
            else {
                resultText += splitText[i];
            }
       //     Debug.Log(resultText);
        }

        resultText = resultText.Replace("\\n", "\n");
        return resultText;

    }
    public static string ConvertFormat(string key, params string[] variables)
    {
        
        string result = key;// instance.missingTextString;
        if (instance.localizedText.ContainsKey(key))
        {
            result = instance.localizedText[key];
        }
        else
        {
            Debug.LogWarning("Not found [" + key + "] / " + instance.localizedText.Count + " / " + StringToByte(key));
        }
        result = string.Format(result, variables);
        result = result.Replace("\\n", "\n");
        return result;

    }
    public static string StringToByte(string str) {
        byte[] e = System.Text.Encoding.UTF8.GetBytes(str);
        string ss = "";
        for (int i = 0; i < e.Length; i++) {
            ss += e[i].ToString() + " ";
        }
        //Debug.Log(str + " / Length " + e.Length);
        return ss;
    }
    private static string Cleanse(string str)
    {
        byte[] e = System.Text.Encoding.UTF8.GetBytes(str);
        byte[] last = new byte[e.Length - 1];
        for (int i = 0; i < last.Length; i++)
        {
            last[i] = e[i];
        }
        return System.Text.Encoding.UTF8.GetString(last);
    }

    public static bool IsReady() {
        return instance.isReady;
    }

    
}
public enum SystemLanguage
{
    Afrikaans = 0,
    Arabic = 1,
    Basque = 2,
    Belarusian = 3,
    Bulgarian = 4,
    Catalan = 5,
    Chinese = 6,
    Czech = 7,
    Danish = 8,
    Dutch = 9,
    English = 10,
    Estonian = 11,
    Faroese = 12,
    Finnish = 13,
    French = 14,
    German = 15,
    Greek = 16,
    Hebrew = 17,
    Hungarian = 18,
    Hugarian = 18,
    Icelandic = 19,
    Indonesian = 20,
    Italian = 21,
    Japanese = 22,
    Korean = 23,
    Latvian = 24,
    Lithuanian = 25,
    Norwegian = 26,
    Polish = 27,
    Portuguese = 28,
    Romanian = 29,
    Russian = 30,
    SerboCroatian = 31,
    Slovak = 32,
    Slovenian = 33,
    Spanish = 34,
    Swedish = 35,
    Thai = 36,
    Turkish = 37,
    Ukrainian = 38,
    Vietnamese = 39,
    Unknown = 40,
}