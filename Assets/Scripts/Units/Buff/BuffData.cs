using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
public class BuffData

{
    public BuffType buffType;
    public float modifier;
    public double duration;
    public string triggerByID;
    double endTime;
    bool timerStarted = false;

    public BuffData(BuffType bType, float mod, double _duration, string trigger) {
        buffType = bType;
        modifier = mod;
        duration = _duration;
        triggerByID = trigger;
    }
    public double StartTimer() {
        endTime = PhotonNetwork.Time + duration;
        timerStarted = true;
        return endTime;
    }

    public bool IsBuffFinished() {
        if (!timerStarted) return false;
        return PhotonNetwork.Time >= endTime;
    }

}
[System.Serializable]
public enum BuffType
{ 
    None,MoveSpeed
}



