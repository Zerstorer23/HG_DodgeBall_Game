﻿using UnityEngine;
using static ConstantStrings;

public class Bot_Normal : IEvaluationMachine {


    public override void DetermineAttackType()
    {
        range_Search = 10f;
        attackRange = 5f;
    }
    public override Vector3 EvaluateMoves()
    {
        Vector3 move = Vector3.zero;
        dangerList.Clear();
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            int tid = go.GetInstanceID();
            Vector3 directionToTarget = go.transform.position - player.movement.networkPos;
            directionToTarget.Normalize();
            float distance = Vector2.Distance(go.transform.position, player.movement.networkPos) - GetRadius(go.transform.localScale);
            float multiplier = 0f;
            switch (go.tag)
            {
                case TAG_PLAYER:
                    move += EvaluatePlayer(go, tid, directionToTarget, distance);
                    break;
                case TAG_PROJECTILE:
                    move += EvaluateProjectile(tid, go, distance, directionToTarget);
                    break;
                case TAG_BOX_OBSTACLE:
                    multiplier = GetMultiplier(distance);
                    move -= directionToTarget * multiplier;
                    break;
            }
        }
        if (dangerList.Count > 0)
        {
            move += Drive_KNN();
        }
        // Debug.Log("Wall move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        else if (move.magnitude <= 0.25f)
        {
            move = Vector3.zero;
        }

        return move;
    }
    public override Vector3 EvaluatePlayer(GameObject go, int tid, Vector3 directionToTarget, float distance)
    {
        float multiplier = GetMultiplier(distance);
        bool skillInUse = skillManager.SkillInUse();
        if (!skillInUse)
        {
            multiplier *= -1f;
        }
        return directionToTarget * multiplier;
    }
    public override Vector3 EvaluateProjectile(int tid, GameObject go, float distance, Vector3 directionToTarget)
    {
        dangerList.Add(go);
        float multiplier = GetMultiplier(distance);
        return -directionToTarget * multiplier;
    }


}
