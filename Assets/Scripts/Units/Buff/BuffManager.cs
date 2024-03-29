﻿using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviourPun
{
    public UnitType unitType;
    public PhotonView pv;
    internal HealthPoint healthPoint;
    [SerializeField] BuffIndicator buffIndicator;
    public Controller controller;

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
        controller = GetComponent<Controller>();
    }

    private void Update()
    {
        CheckBuffDeactivations();
    }
    private void OnEnable()
    {
        if (unitPlayer.myCharacter == CharacterType.YASUMI)
        {
            pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)BuffType.HideBuffs, 1f, -1d);
        }
    }
    private void OnDisable()
    {
        RemoveAllBuff();
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
    private void DeactivateNbuffs(BuffType type, int count) {
        int deactivated = 0;
        for (int i = 0; i < Buffs_active.Count; i++)
        {
            if ((Buffs_active[i]).buffType == type)
            {
                RemoveBuff(Buffs_active[i]);
                UpdateBuffIndicator(Buffs_active[i].buffType, false);
                Buffs_active.RemoveAt(i);
                i--;
            }
            deactivated++;
            if (deactivated >= count && count >= 0) {
                return;
            }
        }
    }

    [PunRPC]
    void AddBuff(int bType, float mod, double _duration)
    {
        BuffData buff = new BuffData((BuffType) bType,  mod,  _duration);
      //  buff.PrintContent();
        switch (buff.buffType)
        {
            case BuffType.None:
                break;
            case BuffType.HealthPoint:
                healthPoint.HealHP(1);
                break;
            case BuffType.Boom:
                HandleBoom();
                break;
            case BuffType.CameraShake:
                if (controller.IsMine) {
                    pv.RPC("HandleCameraShake", RpcTarget.All, controller.uid);
                }
                break;
            case BuffType.InvincibleFromBullets:
            case BuffType.HideBuffs:
            case BuffType.BlockSkill:
                SetTrigger(buff.buffType, true);
                break;
            case BuffType.MirrorDamage:
                SetTrigger(buff.buffType, true);
                ToggleStat(BuffType.NumDamageReceivedWhileBuff, true);
                break;
            case BuffType.OnFire:
                HandleFire(buff);
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

    private void HandleFire(BuffData buff)
    {
        if (GetTrigger(BuffType.InvincibleFromBullets)) return;
        SetTrigger(buff.buffType, true);
        int numFire = CountTrigger(BuffType.OnFire);
        if (numFire >= 12)
        {
            LoseHealthByBuff(LocalizationManager.Convert("_game_hp_loss")); 
            DeactivateNbuffs(BuffType.OnFire, 6);
        }
    }

    private void HandleBoom()
    {
        if (controller.IsMine)
        {
            if (GetTrigger(BuffType.InvincibleFromBullets)) return;
        }
        LoseHealthByBuff(LocalizationManager.Convert("_game_hp_loss"));
    }

    public void RemoveBuff(BuffData buff)
    {
        switch (buff.buffType)
        {
            case BuffType.None:
                break;
            case BuffType.InvincibleFromBullets:
            case BuffType.HideBuffs:
            case BuffType.BlockSkill:
            case BuffType.OnFire:
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
        if (controller.IsMine) {
            int numDamage = GetStat(BuffType.NumDamageReceivedWhileBuff);
            if (numDamage <= 0)
            {
                LoseHealthByBuff(LocalizationManager.Convert("_game_reflection_penalty"));
            }
        }
        ToggleStat(BuffType.NumDamageReceivedWhileBuff, false);
    }
    public void LoseHealthByBuff(string message) {
        if (controller.IsMine)
        {
            healthPoint.HealHP(-1);
            if (controller.IsLocal)
            {
                EventManager.TriggerEvent(MyEvents.EVENT_SEND_MESSAGE, new EventObject(message));
            }
        }
    }

    [PunRPC]
    public void HandleCameraShake(string activatorID) {
        if (PlayerManager.LocalPlayer.uid == (activatorID) || controller.IsBot) return;
        UniversalPlayer caller = PlayerManager.GetPlayerByID(activatorID);
        if (caller != null && caller.GetProperty("TEAM", Team.NONE) == PlayerManager.LocalPlayer.GetProperty("TEAM", Team.NONE)) return;
        float duration = 1.5f;
        MainCamera.instance.DoShake(time: duration);
        MainCamera.instance.DoRotation(time: duration);
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
        return buffTriggers[type] > 0;
    }
    public int CountTrigger(BuffType type)
    {
        if (!buffTriggers.ContainsKey(type))
        {
            return 0;
        }
        return buffTriggers[type];
    }
    void UpdateBuffIndicator(BuffType changedBuff, bool enable) {
        if (!controller.IsLocal && unitPlayer!= null && unitPlayer.buffManager.GetTrigger(BuffType.HideBuffs))
        {
            return;
        }
        buffIndicator.UpdateUI(changedBuff, enable);   
    }

    private void RemoveAllBuff()
    {
        Buffs_active.Clear();
        buffDictionary.Clear();
        buffTriggers.Clear();
        stats.Clear();
        buffIndicator.ClearBuffs();
    }



}
