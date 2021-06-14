﻿using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static ConstantStrings;
using static GameFieldManager;

public class Unit_AutoDrive : MonoBehaviour
{
    internal GameObject directionIndicator;

    public GameObject targetEnemy;
    public Unit_Player player;
    internal float aimAngle;

    public bool secondPrediction = true;
    IEvaluationMachine machine = new IEvaluationMachine();
    public void StartBot(bool useBot, bool isNormalBot)
    {
        gameObject.SetActive(useBot);
        if (!useBot) return;
        if (isNormalBot)
        {
            machine = new Bot_Normal();
        }
        else {
            machine = new IEvaluationMachine();
        }
        directionIndicator = player.driverIndicator;
        machine.SetInformation(this, 20f);
    }


    SortedDictionary<string, Unit_Player> playersOnMap;
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StartListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
    }

    public bool CanAttackTarget()
    {
        if (GameSession.gameModeInfo.isCoop) return true;
        if (player.myCharacter == CharacterType.Taniguchi) return false;
        FindNearestPlayer();
        if (targetEnemy == null)
        {
            return false;
        }
        return machine.IsInAttackRange(targetEnemy);
    }
    private void OnDisable()
    {
        machine.Reset();
        EventManager.StopListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StopListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
    }

    private void OnBoxSpawned(EventObject arg0)
    {
        machine.AddFoundObject(arg0.goData.GetInstanceID(), arg0.goData);
    }

    private void OnBoxEnabled(EventObject arg0)
    {
        machine.RemoveFoundObject(arg0.goData.GetInstanceID());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, machine.range_Search);
        foreach (GameObject go in machine.foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            Gizmos.DrawWireSphere(go.transform.position, 0.5f);
        }
        Gizmos.color = (machine.doApproach) ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position + lastEvaluatedVector, 0.6f);
/*        Gizmos.DrawWireSphere(xWall, 1f);
        Gizmos.DrawWireSphere(yWall, 1f);*/
        if (targetEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetEnemy.transform.position, 1f);
        }
      /*  Gizmos.color = Color.green;
        for (int i = 0; i < 360; i++)
        {
            if (!blockedAngles[i])
            {
                Vector3 pos = transform.position + GetAngledVector(i, range_collision);
                Gizmos.DrawWireSphere(pos, 0.05f);
            }
        }*/

    }
    // Update is called once per frame


    
    void FindNearestPlayer()
    {
        playersOnMap = GameFieldManager.gameFields[player.fieldNo].playerSpawner.unitsOnMap;
        float nearestEnemyDist = float.MaxValue;
        foreach (var p in playersOnMap.Values)
        {
            if (p == null || !p.gameObject.activeInHierarchy) continue;
            if (p.gameObject.GetInstanceID() == machine.myInstanceID) continue;
            if (GameSession.gameModeInfo.isTeamGame && p.myTeam == player.myTeam) continue;
            if (p.buffManager.GetTrigger(BuffType.InvincibleFromBullets)) continue;
            if (p.buffManager.GetTrigger(BuffType.MirrorDamage)) continue;
            float dist = Vector2.Distance(player.movement.networkPos, p.gameObject.transform.position);
            if (dist < nearestEnemyDist)
            {
                nearestEnemyDist = dist;
                targetEnemy = p.gameObject;
            }
        }
        //  Debug.Log("Players " + playersOnMap.Count + " / " + targetEnemy);
    }
    public Vector3 lastEvaluatedVector = Vector3.zero;
    private void FixedUpdate()
    {
        machine.RemoveObjects();
        machine.FindNearByObjects();
    }
    private void Update()
    {
        lastEvaluatedVector = machine.EvaluateMoves();
    }

    public float EvaluateAim()
    {
        FindNearestPlayer();
        if (targetEnemy == null) {
            return player.movement.aimAngle;
        }
        Vector3 targetPosition = targetEnemy.transform.position;
        Vector3 sourcePosition = player.movement.networkPos;
        aimAngle = GameSession.GetAngle(sourcePosition, targetPosition);
        directionIndicator.transform.localPosition = GetAngledVector(aimAngle, 1.4f); // new Vector3(dX, dY);
        directionIndicator.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
        return aimAngle;
    }

    public float GetSpeedModifier(float mySpeed, float targetSpeed)
    {
        if (mySpeed > targetSpeed) return 1f;
        return Mathf.Pow(((targetSpeed - mySpeed)), 2);
    }


}