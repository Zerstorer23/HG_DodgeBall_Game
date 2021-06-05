﻿using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviourPun
{
    public UnitType unitType;
    public PhotonView pv;
    HealthPoint healthPoint;
    [SerializeField] BuffIndicator buffIndicator;


    //COMMON
    [SerializeField] List<BuffData> Buffs_active = new List<BuffData>();
    Unit_Player unitPlayer;
    Dictionary<BuffType, float> buffDictionary = new Dictionary<BuffType, float>();
    Dictionary<BuffType, int> buffTriggers = new Dictionary<BuffType, int>();
    Dictionary<BuffType, int> stats = new Dictionary<BuffType, int>();
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        healthPoint = GetComponent<HealthPoint>();
        unitPlayer = GetComponent<Unit_Player>();
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
                UpdateBuffIndicator(Buffs_active[i].buffType,false);
                Buffs_active.RemoveAt(i);
                i--;
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
            case BuffType.MirrorDamage:
                SetTrigger(buff.buffType, true);
                ToggleStat(BuffType.NumDamageReceivedWhileBuff, true);
                break;
            default:
                SetBuff(buff.buffType, buff.modifier, true);
                break;
        }

        if (buff.duration > 0) {
            buff.StartTimer();
            Buffs_active.Add(buff);
            UpdateBuffIndicator(buff.buffType, true);
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
            case BuffType.MirrorDamage:
                HandleMirroringBuff(buff);
                break;
            default:
                SetBuff(buff.buffType, buff.modifier, false);
                break;
        }
    }

    private void HandleMirroringBuff(BuffData buff)
    {
        SetTrigger(buff.buffType, false);
        if (pv.IsMine) {
            int numDamage = GetStat(BuffType.NumDamageReceivedWhileBuff);
            if (numDamage <= 0)
            {
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject() { stringObj =" 반사 실패 패널티 -1" });
                healthPoint.pv.RPC("ChangeHP", RpcTarget.AllBuffered, -1);
            }
        }
        ToggleStat(BuffType.NumDamageReceivedWhileBuff, false);
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
    public void ToggleStat(BuffType type, bool enable = true)
    {
        if (enable)
        {
            if (stats.ContainsKey(type))
            {
                stats[type] = 0;
            }
            else
            {
                stats.Add(type, 0);
            }
        }
        else {
            if (stats.ContainsKey(type))
            {
                stats.Remove(type);
            }
        }

    }
    public void AddStat(BuffType type, int amount)
    {
        if (stats.ContainsKey(type))
        {
            stats[type] += amount;
        }
        
    }

    private int GetStat(BuffType type) {
        if (!stats.ContainsKey(type))
        {
            stats.Add(type, 0);
        }
        return stats[type];
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
    public float HasBuff(BuffType type) {
        if (!buffDictionary.ContainsKey(type))
        {
            return 0;
        }
        return (buffDictionary[type] - 1f);
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
        //Debug.Log("Num trigger " + buffTriggers[type] + " tpye " + type);
        return buffTriggers[type] > 0;
    }

    void UpdateBuffIndicator(BuffType changedBuff, bool enable) {
        if (!pv.IsMine && unitPlayer!= null && unitPlayer.myCharacter == CharacterType.YASUMI)
        {
            return;
        }
        buffIndicator.UpdateUI(changedBuff, enable);   
    }

    private void RemoveAllBuff()
    {
        for (int i = 0; i < Buffs_active.Count; i++)
        {
            RemoveBuff(Buffs_active[i]);
        }
        Buffs_active.Clear();
        buffIndicator.ClearBuffs();
    }



}
