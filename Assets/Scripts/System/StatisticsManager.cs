using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class StatisticsManager : MonoBehaviourPun
{
    // Start is called before the first frame update
    private static StatisticsManager prStatManager;
    PhotonView pv;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }

    public static StatisticsManager instance
    {
        get
        {
            if (!prStatManager)
            {
                prStatManager = FindObjectOfType<StatisticsManager>();
                if (!prStatManager)
                {
                    Debug.LogWarning("There needs to be one active EventManger script on a GameObject in your scene.");
                }

            }

            return prStatManager;
        }
    }
    private Dictionary<StatTypes, Dictionary<string, int>> statLibrary;
    public  void Init()
    {
        statLibrary = new Dictionary<StatTypes, Dictionary<string, int>>();
        for (int i = 0; i < (int)StatTypes.END;  i++) {
            StatTypes head = (StatTypes)i;
            Dictionary<string, int> library = new Dictionary<string, int>();
            statLibrary.Add(head, library);
        }
    }
    public static void RPC_AddToStat(StatTypes stype ,string tag, int amount)
    {
        instance.pv.RPC("AddToStat", RpcTarget.AllBuffered,(int)stype, tag, amount);
    }
    [PunRPC]
    public void AddToStat(int statType, string tag, int amount) {
        if (statLibrary == null) {
            Init();
        }
        StatTypes head = (StatTypes)statType;
        if (head == StatTypes.END) return;

        if (!statLibrary[head].ContainsKey(tag))
        {
            statLibrary[head].Add(tag, amount);
        }
        else {
            statLibrary[head][tag] += amount;
        }
    }

    internal static string GetHighestPlayer(StatTypes header)
    {
        Dictionary<string, int> statBoard = instance.statLibrary[header];
/*        foreach (KeyValuePair<string, int> entry in statBoard) {
            Debug.Log(entry.Key + " / " + entry.Value);
        }*/
        if (statBoard.Count == 0) {
            return null;
        }else      if (statBoard.Count == 1) {
            return statBoard.First().Key;
        }
        var keyOfMaxValue = statBoard.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
        return keyOfMaxValue;
    }
    /*
        private void SortInfo(Image[] images, Text[] texts, string tag)
    {
        Dictionary<string, int> statBoard = new Dictionary<string, int>();
        foreach (string uid in UID_List)
        {
            int kill = StatisticsManager.GetStat(tag + uid);
            statBoard.Add(uid, kill);
        }

        var items = from pair in statBoard
                    orderby pair.Value descending
                    select pair;
        var listed = items.ToList();
        for (int i = 0; i < mostKilledSprites.Length; i++)
        {
            if (i < listed.Count)
            {
                UnitConfig u557 = unitDictionary[listed[i].Key];
                images[i].sprite = u557.myPortraitSprite;
                texts[i].text = listed[i].Value.ToString();
            }
        }
    }
     */


    public static int GetStat(StatTypes stype,string playerID) {
        if (instance.statLibrary[stype].ContainsKey(playerID))
        {
           return instance.statLibrary[stype][playerID];
        }
        else return 0;
    }


    internal static void SetStat(StatTypes stype, string playerID, int v)
    {
        if (instance.statLibrary[stype].ContainsKey(playerID))
        {
            instance.statLibrary[stype][playerID] = v;
        }
        else
        {
            instance.statLibrary[stype].Add(playerID, v);
        }
    }

}
public enum StatTypes { 
    GENERAL, KILL,EVADE,MINIGAME,SCORE
        ,END
}
