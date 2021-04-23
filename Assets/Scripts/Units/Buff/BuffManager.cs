using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviourPun
{
    public UnitType unitType;
    public PhotonView pv;

    //COMMON
    [SerializeField] List<BuffData> Buffs_active = new List<BuffData>();
    [SerializeField] SpriteRenderer mySprite;
    internal float speedModifier = 1f;
    internal int numStun;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void Update()
    {
        CheckBuffDeactivations();
    }
    private void OnEnable()
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
                Buffs_active.RemoveAt(i);
                i--;
                UpdateBuffIndicator();
            }

        }
    }

    public void RPC_AddBuff(BuffData buff)
    {
        pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)buff.buffType,buff.modifier,buff.duration,buff.triggerByID);
    }
    [PunRPC]
    void AddBuff(int bType, float mod, double _duration, string trigger)
    {
        BuffData myBuff = new BuffData((BuffType) bType,  mod,  _duration, trigger);

        
        switch (myBuff.buffType)
        {
            case BuffType.None:
                break;
            case BuffType.MoveSpeed:
                ChangeSpeedModifier(myBuff.modifier, true);
                break;
        }
        myBuff.StartTimer();
        Buffs_active.Add(myBuff);
        UpdateBuffIndicator();
    }
    void UpdateBuffIndicator() {
        Debug.Log("Change color " + ((Buffs_active.Count > 0) ? Color.red : Color.white));
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


    public void RemoveBuff(BuffData buff)
    {
        switch (buff.buffType)
        {
            case BuffType.None:
                break;
            case BuffType.MoveSpeed:
                ChangeSpeedModifier(buff.modifier, false);
                break;
        }
    }


    internal void ChangeSpeedModifier(float _mod, bool enable)
    {
        if (enable)
        {
            speedModifier += _mod;
        }
        else {
            speedModifier -= _mod;
        }
    }
    public float GetSpeedModifier() {
        return Mathf.Max(speedModifier, 0.01f);
    
    }

    private void ApplyKnockBack()
    {
        numStun++;
    }

  



    private void RemoveKnockback()
    {
        numStun--;
    }

}
