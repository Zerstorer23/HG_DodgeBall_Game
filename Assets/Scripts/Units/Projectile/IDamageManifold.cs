using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageManifold 
{
    bool FindAttackHistory(int tid);
    bool CheckDuplicateDamage(int tid);
    void Reset();
}
public class DamageQueue : IDamageManifold
{
    List<int> damageRecords = new List<int>();
    public bool CheckDuplicateDamage(int tid)
    {
        if (damageRecords.Count > 0 && damageRecords[damageRecords.Count-1] == tid)
        {
            return false;
        }
        else
        {
            damageRecords.Add(tid);
            return true;
        }
    }

    public bool FindAttackHistory(int tid)
    {
        return (damageRecords.Count > 0 && damageRecords.Contains(tid));
    }

    public void Reset()
    {
        damageRecords.Clear();
    }
}
public class DamageOnce : IDamageManifold
{
    HashSet<int> damageRecords = new HashSet<int>();
    public bool CheckDuplicateDamage(int tid)
    {
        if (damageRecords.Contains(tid))
        {
            return false;
        }
        else
        {
            damageRecords.Add(tid);
            return true;
        }
    }

    public bool FindAttackHistory(int tid)
    {
        return damageRecords.Contains(tid);
    }

    public void Reset()
    {
        damageRecords.Clear();
    }
}
public class DamageTimed : IDamageManifold
{
    Dictionary<int, double> damageRecords = new Dictionary<int, double>();

    public bool FindAttackHistory(int tid)
    {
        return damageRecords.ContainsKey(tid);
    }

    bool IDamageManifold.CheckDuplicateDamage(int tid)
    {
        if (damageRecords.ContainsKey(tid))
        {
            if (PhotonNetwork.Time - damageRecords[tid] >= 0.7f)
            {
                damageRecords[tid] = PhotonNetwork.Time;
                return true;
            }
            return false;
        }
        else
        {
            damageRecords.Add(tid, PhotonNetwork.Time);
            return true;
        }
    }

    void IDamageManifold.Reset()
    {
        damageRecords.Clear();
    }
}
/*public class DamageInAndOut: IDamageManifold
{
    HashSet<int> damageRecords = new HashSet<int>();
    public bool CheckDuplicateDamage(int tid)
    {
        if (damageRecords.Contains(tid))
        {
            return false;
        }
        else
        {
            damageRecords.Add(tid);
            return true;
        }
    }

    public void Reset()
    {
        damageRecords.Clear();
    }
}*/
public enum DamageManifoldType
{
    Once, Queue, Timed
        //,InAndOout
}