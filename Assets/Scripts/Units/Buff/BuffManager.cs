using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviourPun
{
    public UnitType unitType;
    public PhotonView pv;
    HealthPoint healthPoint;

    //COMMON
    [SerializeField] List<BuffData> Buffs_active = new List<BuffData>();
    [SerializeField] SpriteRenderer mySprite;
    Dictionary<BuffType, float> buffDictionary = new Dictionary<BuffType, float>();
    Dictionary<BuffType, int> buffTriggers = new Dictionary<BuffType, int>();
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        healthPoint = GetComponent<HealthPoint>();
    }
    private void Update()
    {
        CheckBuffDeactivations();
    }
    private void OnEnable()
    {
        RemoveAllBuff();
        buffDictionary = new Dictionary<BuffType, float>();
    }


    private void CheckBuffDeactivations()
    {

        for (int i = 0; i < Buffs_active.Count; i++)
        {
            if ((Buffs_active[i]).IsBuffFinished())
            {
                RemoveBuff(Buffs_active[i]);
                Buffs_active.RemoveAt(i);
                i--;
                UpdateBuffIndicator();
            }

        }
    }

    [PunRPC]
    void AddBuff(int bType, float mod, double _duration)
    {
        BuffData buff = new BuffData((BuffType) bType,  mod,  _duration);
   //     buff.PrintContent();
        switch (buff.buffType)
        {
            case BuffType.None:
                break;
            case BuffType.HealthPoint:
                healthPoint.HealHP(1);
                break;
            case BuffType.InvincibleFromBullets:
                SetTrigger(buff.buffType, true);
                break;
            default:
                SetBuff(buff.buffType, buff.modifier, true);
                break;
        }

        if (buff.duration > 0) {
            buff.StartTimer();
            Buffs_active.Add(buff);
            UpdateBuffIndicator();
        }
    }
    public void RemoveBuff(BuffData buff)
    {
        switch (buff.buffType)
        {
            case BuffType.None:
                break;
            case BuffType.InvincibleFromBullets:
                SetTrigger(buff.buffType, false);
                break;
            default:
                SetBuff(buff.buffType, buff.modifier, false);
                break;
        }
    }
    private void SetTrigger(BuffType type, bool enable)
    {
        if (buffTriggers.ContainsKey(type))
        {
            if (enable)
            {
                buffTriggers[type]++;
            }
            else {
                if(buffTriggers[type] > 0)
                    buffTriggers[type]--;
            }
        }
        else
        {
            buffTriggers.Add(type, (enable)?1:0);
        }
    }

    private void SetBuff(BuffType type, float amount, bool enable) {

        if (buffDictionary.ContainsKey(type))
        {
            buffDictionary[type] += (enable) ? amount : -amount;
        }
        else { 
            float buffAmount = (enable) ? 1f + amount : 1f - amount;
            buffDictionary.Add(type, buffAmount);        
        }
    }
    public float GetBuff(BuffType type) {
        if (!buffDictionary.ContainsKey(type))
        {
            buffDictionary.Add(type, 1f);
        }
        return Mathf.Max(buffDictionary[type],0.01f);    
    }
    public bool GetTrigger(BuffType type)
    {
        if (!buffTriggers.ContainsKey(type))
        {
            buffTriggers.Add(type, 0);
        }
        return buffTriggers[type] > 0;
    }

    void UpdateBuffIndicator() {
        mySprite.color = (Buffs_active.Count > 0) ? Color.red : Color.white;    
    }

    private void RemoveAllBuff()
    {
        for (int i = 0; i < Buffs_active.Count; i++)
        {
            RemoveBuff(Buffs_active[i]);
        }
        Buffs_active.Clear();
        UpdateBuffIndicator();
    }



}
