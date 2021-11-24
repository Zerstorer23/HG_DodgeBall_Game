using Photon.Pun;
using UnityEngine;
using static ConstantStrings;

public class Bot_JeopDae : IEvaluationMachine {
    public Bot_JeopDae(Unit_AutoDrive autoDriver) : base(autoDriver)
    {
        this.autoDriver = autoDriver;
        this.player = autoDriver.player;
        myInstanceID = player.gameObject.GetInstanceID();
        movement = player.movement;
        skillManager = player.skillManager;
        lazyEvalInterval = 0.8d;
        SetRange(15f);
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
            PhotonNetwork.Time <= (lastEvalTime + lazyEvalInterval))
        {
            return lastMove;
        }
        move += GetAwayFromWalls();
        move += GetToCapturePoint();

        //2. KNNs

        move += Drive_KNN();
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        else if (move.magnitude <= 0.25f)
        {
            move = Vector3.zero;
        }

        move = AbsoluteEvasion(move);
        lastMove = move;
        lastEvalTime = PhotonNetwork.Time;
        lazyEvalInterval = Random.Range(0.5f, 1.1f);
        return move;
    }


}
