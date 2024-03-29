﻿using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;
using static GameFieldManager;

public partial class IEvaluationMachine
{
    RandomDirectionMaker rdm = new RandomDirectionMaker();

    public int collideCount = 0;

    protected Vector3 lastMove;
    protected double lastEvalTime;
    protected double lazyEvalInterval = 0.7d;
    public virtual Vector3 EvaluateMoves()
    {
        //1. Heuristic
        Vector3 move = Vector3.zero;
        dangerList.Clear();
        collideCount = 0;

        foreach (GameObject go in foundObjects.Values)
        {
            if (IsInactive(go)) continue;
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
                    if (distance < range_Knn)
                    {
                        dangerList.Add(go);
                    }
                    if (distance <= 2.5f)
                    {
                        collideCount++;
                    }
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
        if (dangerList.Count == 0 &&
            PhotonNetwork.Time <= (lastEvalTime + lazyEvalInterval)) {
            return lastMove;
        }
        move += GetAwayFromWalls();
        //move += GetToCapturePoint();

        //2. KNNs

        move += Drive_KNN();
        // Debug.Log("Wall move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        else if (move.magnitude <= 0.25f)
        {
            move = Vector3.zero;
        }

        move = AbsoluteEvasion(move);
    //    move = PreventExtremeMove(move);
        lastMove = move;
        lastEvalTime = PhotonNetwork.Time;
        //Debug.Log("Final move " + move +" mag "+move.magnitude + " / "+move.sqrMagnitude);
        return move;
    }
    protected Vector3 lastRecPos;
    protected double lastRecTime;
    protected float threshold = 0.05f;
    public virtual Vector3 PreventExtremeMove(Vector3 v)
    {

        if (PhotonNetwork.Time > lastRecTime + 0.5)
        {
            lastRecPos = player.movement.networkPos;
            lastRecTime = PhotonNetwork.Time;
        }
        Vector3 newPos = player.movement.networkPos + player.movement.GetMovementSpeed() * Time.deltaTime * v;
        float dist = Vector2.Distance(lastRecPos, newPos);
        if (dist <= threshold
           && collideCount < 2
           && dangerList.Count < 2
            )
        {
            return Vector3.zero;
        }
        else
        {
            return v;
        }
    }
    public Vector3 predictedPosition;
    public virtual float PredictAim(Unit_Player source, Unit_Player target)
    {
        Vector3 targetPosition = target.transform.position;
        Vector3 sourcePosition = source.transform.position;
        if (doPredictedAim)
        {
            float distance = Vector2.Distance(source.transform.position, target.transform.position);
            float predictionTime = distance / source.skillManager.ai_projectileSpeed;
             predictedPosition =target.movement.GetPredictedPositionAfter(predictionTime);
           // Debug.Log("Predict " + (predictedPosition-target.transform.position) + " after " + predictionTime);
            return GameSession.GetAngle(sourcePosition, predictedPosition);
        }
        else {
            return GameSession.GetAngle(sourcePosition, targetPosition); 
        }
    }
    public virtual float DiffuseAim(float angle) {
        return angle;
    }

    protected Vector3 EvaluateBuff(GameObject go, int tid, Vector3 directionToTarget, float distance)
    {
        BuffObject bObj = cachedComponent.Get<BuffObject>(tid, go);
        if (bObj.status == BuffObjectStatus.Enabled)
        {
            if (bObj.buffConfig.buffType == BuffType.Boom)
            {
                return -directionToTarget * GetMultiplier(distance) * 2;
            }
            else
            {
                return directionToTarget * GetMultiplier(distance) * 2;
            }

        }
        else if (bObj.status == BuffObjectStatus.Starting) {
            return KeepConstantDistance(directionToTarget, distance, 2f, true);
        }
        return Vector3.zero;
       
    }

    public virtual void DetermineAttackType(CharacterType thisCharacter = CharacterType.NONE)
    {
        if (thisCharacter == CharacterType.NONE) {
            thisCharacter = player.myCharacter;
        }
        var config = ConfigsManager.unitDictionary[thisCharacter];
        attackRange = config.attackRange;
        isKamikazeSkill = config.isKamikaze;
        doPredictedAim = config.DoPredictionShot;
        skillInterval = (doPredictedAim) ? 0.2d : 0.5d;
    }
    public virtual Vector3 EvaluatePlayer(GameObject go, int tid, Vector3 directionToTarget, float distance)
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
            else
            {
                doApproach = !player.FindAttackHistory(tid);
            }
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



    protected Vector3 EscapePlayer(Vector3 directionToTarget, float distance, Unit_Player enemyPlayer)
    {
        float multiplier;
        switch (enemyPlayer.myCharacter)
        {
            case CharacterType.NAGATO:
            case CharacterType.HARUHI:
            case CharacterType.KOIZUMI:
            case CharacterType.SASAKI:
            case CharacterType.KOIHIME:
            case CharacterType.KYONKO:
            case CharacterType.KUYOU:
            case CharacterType.KYOUKO:
            case CharacterType.KYONMOUTO:
            case CharacterType.T:
            case CharacterType.KYON:
            case CharacterType.MORI:
                multiplier = -GetMultiplier(distance / 5f);
                break;
            case CharacterType.KIMIDORI:
                return KeepConstantDistance(directionToTarget, distance, 4f, true);
            case CharacterType.MIKURU:
                Vector3 RandomDir = rdm.PollRandom();
                return RandomDir * GetMultiplier(distance / 5f);
            default:
                multiplier = -GetMultiplier(distance);
                break;
        }
        return directionToTarget * multiplier;
    }

    protected Vector3 KeepConstantDistance(Vector3 directionToTarget, float distance, float preferredDistance, bool approachToPreferredDistance) {
        float multiplier = 0;
        if (distance < (preferredDistance - 0.5f))
        {
            multiplier = -GetMultiplier(distance / 5f);
        }
        else if (distance > (preferredDistance + 0.5f))
        {
            multiplier = GetMultiplier(distance / 5f);
        }
        if (approachToPreferredDistance) multiplier *= -1f;
        return directionToTarget * multiplier;

    }
    protected Vector3 ApproachPlayer(Vector3 direction, float distance)
    {
        float multiplier;
        switch (player.myCharacter)
        {
            case CharacterType.NAGATO:
            case CharacterType.HARUHI:
            case CharacterType.KOIZUMI:
            case CharacterType.KUYOU:
            case CharacterType.KYOUKO:
            case CharacterType.T:
            case CharacterType.SASAKI:
            case CharacterType.KOIHIME:
            case CharacterType.TSURUYA:
                if (isKamikazeSkill)
                {
                    multiplier = GetMultiplier(distance / 2.5f);
                }
                else
                {
                    multiplier = GetMultiplier(distance / 1.5f);
                }
                break;
            case CharacterType.KIMIDORI:
                multiplier = GetMultiplier(4f);
                break;
            default:
                multiplier = GetMultiplier(distance);
                break;
        }

        return direction * multiplier;
    }

    public virtual Vector3 EvaluateProjectile(int tid, GameObject go, float distance, Vector3 directionToTarget)
    {
        if (isKamikazeSkill && skillManager.SkillInUse())
        {
            return Vector3.zero;
        }
        HealthPoint hp = cachedComponent.Get<HealthPoint>(tid, go);
        Projectile_Movement pMove = (Projectile_Movement)hp.movement;
        // float speedMod = GetSpeedModifier(player.movement.GetMovementSpeed(), pMove.moveSpeed);
        if (pMove.characterUser == CharacterType.NAGATO)
        {
            if (!IsApproaching(pMove, distance)) return Vector3.zero;
            Vector3 dir = PerpendicularEscape(pMove, directionToTarget) * GetMultiplier(distance / 3f);
            //  Debug.Log("Move to " + dir);
            return dir;
        }

        if (pMove.moveSpeed > player.movement.GetMovementSpeed())
        {
            if (!IsApproaching(pMove, distance)) return Vector3.zero;
            /*   
               return -directionToTarget * GetMultiplier(distance / speedDiff);*/
            float speedDiff = pMove.moveSpeed * 1.5f / player.movement.GetMovementSpeed();
            if (pMove.moveType == MoveType.Straight)
            {
                Vector3 dir = PerpendicularEscape(pMove, directionToTarget) * GetMultiplier(distance / speedDiff);
                return dir;
            }
            else
            {
                //Debug.Log("Back evasion");
                return -directionToTarget * GetMultiplier(distance / speedDiff);
            }
        }
        else
        {
            return -directionToTarget * GetMultiplier(distance);
        }
    }
    public virtual Vector3 GetAwayFromWalls()
    {
        if (isKamikazeSkill && skillManager.SkillInUse())
        {
            return Vector3.zero;
        }
        int activeMax = gameFields[player.fieldNo].bulletSpawner.activeMax;
        Vector3 move = Vector3.zero;
        float xBound = (movement.networkPos.x < movement.mapSpec.xMid) ? movement.mapSpec.xMin : movement.mapSpec.xMax;
        float yBound = (movement.networkPos.y < movement.mapSpec.yMid) ? movement.mapSpec.yMin : movement.mapSpec.yMax;

        walls[0] = new Vector3(movement.networkPos.x, yBound);
        walls[1] = new Vector3(xBound, movement.networkPos.y);
        float mod = 3f;// (activeMax == 0) ? 1f : 2f;
        for (int i = 0; i < 2; i++) {
            if (Vector2.Distance(walls[i], movement.networkPos) <= range_Search)
            {
                move += EvaluateToPoint(walls[i], false, mod);
            }
        }
        //   Debug.Log("Center " + (dirToCenter * GetMultiplier(centerDist)) + " bound " + (-dirToBound * GetMultiplier(boundDist)) + " move " + move);
        return move;
    }
    public virtual Vector3 GetToCapturePoint()
    {
        if (GameSession.gameModeInfo.gameMode != GameMode.TeamCP) return Vector3.zero;
         GameField_CP cpField = (GameField_CP)gameFields[0];
        Map_CapturePoint nearestCP = cpField.cpManager.GetNearestValidPoint(player.myTeam, player.transform.position);
        if (nearestCP == null) return Vector3.zero;
        float distance = Vector2.Distance(player.transform.position, nearestCP.transform.position);
        if (distance <= 1.5f) return Vector3.zero;
        Vector3 move = (nearestCP.transform.position - player.transform.position) /GetMultiplier(distance);//.normalized*1.25f;
        return move;
    }
    public virtual float GetMultiplier(float x)
    {
        float y = (1 / Mathf.Pow(x + 2, 2)) * 48;
        //  float y = Mathf.Pow((range - x), 3);
        return y;
    }

    protected bool IsApproaching(Projectile_Movement pMove, float distance) {
        if (distance < range_Collision) return true;
        Vector3 nextPos = pMove.GetNextPosition();
        float nextDistance = Vector2.Distance(nextPos, pMove.transform.position);
        return nextDistance <= distance;   
    }
}
