using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action_Player_AddBuff : SkillAction
{
    public override float Activate()
    {
        BuffData buff = (BuffData)GetParam(SkillParams.BuffData);
        parent.caster.GetComponent<BuffManager>().pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)buff.buffType, buff.modifier, buff.duration);
        return 0f;
    }
}
public class Action_Player_InvincibleBuff : SkillAction
{
    public override float Activate()
    {
        if (GameFieldManager.CheckSuddenDeathCalled(parent.castingPlayer.fieldNo)) return 0f;
        if (GameSession.gameModeInfo.gameMode == GameMode.Tournament) return 0f;
        float duration = (float)GetParam(SkillParams.Duration);
        parent.caster.GetComponent<BuffManager>().pv.RPC("AddBuff", RpcTarget.AllBuffered, (int)BuffType.InvincibleFromBullets, 0f, (double)duration);
        return 0f;
    }
}