using Photon.Pun;
using UnityEngine;
using static ConstantStrings;

public class Bot_Normal : IEvaluationMachine {
    public Bot_Normal(Unit_AutoDrive autoDriver) : base(autoDriver)
    {
        this.autoDriver = autoDriver;
        this.player = autoDriver.player;
        myInstanceID = player.gameObject.GetInstanceID();
        movement = player.movement;
        skillManager = player.skillManager;
        lazyEvalInterval = 0.33d;
        SetRange(8f);
    }
    public override void DetermineAttackType(CharacterType thisCharacter = CharacterType.NONE)
    {
        if (thisCharacter == CharacterType.NONE)
        {
            thisCharacter = player.myCharacter;
        }
        var config = ConfigsManager.unitDictionary[thisCharacter];
        attackRange = config.attackRange + Random.Range(-3f,3f);

        /*
         * 범위 무작위
         */
        isKamikazeSkill = config.isKamikaze;
        doPredictedAim = false;
        lazyEvalInterval = 1;
        skillInterval = 1;
    }
    public override Vector3 EvaluateMoves()
    {
        //1. Heuristic
        Vector3 move = Vector3.zero;
        dangerList.Clear();
        collideCount = 0;
        if (PhotonNetwork.Time <= (lastEvalTime + lazyEvalInterval))
        {
            return lastMove;
        }
        foreach (GameObject go in foundObjects.Values)
        {
            if (IsInactive(go)) continue;
            float seed = Random.Range(0f, 1f);
            if (seed < 0.4f) continue;

            /*
             * 40%는 무시
             */

            int tid = go.GetInstanceID();
            Vector3 directionToTarget = go.transform.position - player.movement.networkPos;
            directionToTarget.Normalize();
            float distance = Vector2.Distance(go.transform.position, player.movement.networkPos) - GetRadius(go.transform.localScale);
            //   if (distance > range) continue;
            float multiplier = 0f;
            switch (go.tag)
            {
                case TAG_PLAYER:
                    move += EvaluatePlayer(go, tid, directionToTarget, distance);
                    break;
                case TAG_PROJECTILE:
                    move += EvaluateProjectile(tid, go, distance, directionToTarget);
                    break;
                case TAG_BUFF_OBJECT:
                    move += EvaluateBuff(go, tid, directionToTarget, distance);
                    break;
                case TAG_BOX_OBSTACLE:
                    multiplier = GetMultiplier(distance);
                    collideCount++;
                    move -= directionToTarget * multiplier;
                    break;
            }
        }
       
        move += GetAwayFromWalls();
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        else if (move.magnitude <= 0.25f)
        {
            move = Vector3.zero;
        }
        lastMove = move;
        return move;
    }
    public override Vector3 EvaluatePlayer(GameObject go, int tid, Vector3 directionToTarget, float distance)
    {
        Unit_Player enemyPlayer = cachedComponent.Get<Unit_Player>(tid, go);
        if (!IsPlayerDangerous(enemyPlayer)) return Vector3.zero;
        bool skillAvailable = skillManager.SkillIsReady() && !isRecharging;
        bool skillInUse = skillManager.SkillInUse();
        if (isKamikazeSkill && skillInUse)
        {
            if (player.myCharacter == CharacterType.SASAKI)
            {
                doApproach = true;
            }
            /*
             절대 한번에 두번공격 못하게 history코드 삭제
             */
        }
        else
        {
            doApproach = skillAvailable;
        }
        if (player.myCharacter == CharacterType.KIMIDORI)
        {
            doApproach = true;
        }
        if (doApproach)
        {
            return ApproachPlayer(directionToTarget, distance);
        }
        else
        {
            return EscapePlayer(directionToTarget, distance, enemyPlayer);
        }


    }

    public override float DiffuseAim(float angle)
    {
        return angle + Random.Range(-90f, 90f); 
    }

}
