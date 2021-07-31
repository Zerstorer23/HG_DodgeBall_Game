using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static ConstantStrings;
using static GameFieldManager;

public class Unit_AutoDrive : MonoBehaviour
{
    internal GameObject directionIndicator;

    public Unit_Player targetEnemy;
    public Unit_Player player;
    internal float aimAngle;
    public BotType botType = BotType.Easy;

    public bool secondPrediction = true;
    public IEvaluationMachine machine;
    public void StartBot(BotType bType)//bool useBot, bool isNormalBot)
    {

        gameObject.SetActive(bType != BotType.None);
        botType = bType;
        switch (bType)
        {
            case BotType.None:
                return;
            case BotType.Easy:
                machine = new Bot_Easy(this);
                break;
            case BotType.Normal:
                machine = new Bot_Normal(this);
                break;
            case BotType.Hard:
                machine = new IEvaluationMachine(this);
                break;
        }
        directionIndicator = player.driverIndicator;
    }


    SortedDictionary<string, Unit_Player> playersOnMap;
    double spawnTime;
    private void OnEnable()
    {
        EventManager.StartListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StartListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
        spawnTime = PhotonNetwork.Time;
    }

    public bool CanAttackTarget()
    {
        if (PhotonNetwork.Time < (spawnTime + 1d)) return false;
        if (!machine.IsFireSkillDecision(player.skillManager)) return false;
    //    if (player.myCharacter == CharacterType.SPECTATOR) return false;
        FindNearestPlayer();
        if (targetEnemy == null)
        {
            return false;
        }
        return machine.IsInAttackRange(targetEnemy.gameObject);
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

    public float testrange = 10f;

    private void OnDrawGizmos()
    {
        if (player.controller.IsLocal)
        {
/*            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, testrange);*/
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, machine.range_Search);
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(machine.predictedPosition,0.6f);
            DrawApproachStatus();
            DrawFoundObjects();
            DrawEnemy();
        }
    }
    // Update is called once per frame
    void DrawEnemy() {
        if (targetEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetEnemy.transform.position, 1f);
        }
    }
    void DrawFoundObjects() {
        foreach (GameObject go in machine.foundObjects.Values)
        {
            if (IsInactive(go))
            {
                continue;
            }
            Gizmos.DrawWireSphere(go.transform.position, 0.5f);
        }
    }
    void DrawApproachStatus() {
        Gizmos.color = (machine.doApproach) ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position + lastEvaluatedVector, 0.6f);
    }

    void FindNearestPlayer()
    {
        targetEnemy = null;
        playersOnMap = gameFields[player.fieldNo].playerSpawner.unitsOnMap;
        float nearestEnemyDist = float.MaxValue;
        foreach (var p in playersOnMap.Values)
        {
            if (!PlayerIsAttackable(p)) continue;
            float dist = Vector2.Distance(player.movement.networkPos, p.gameObject.transform.position);
            if (dist < nearestEnemyDist)
            {
                nearestEnemyDist = dist;
                targetEnemy = p;
            }
        }
        //  Debug.Log("Players " + playersOnMap.Count + " / " + targetEnemy);
    }
   public bool PlayerIsAttackable(Unit_Player p) {
        if (IsInactive(p)) return false;
        if (p.gameObject.GetInstanceID() == machine.myInstanceID) return false;
        if (GameSession.gameModeInfo.isTeamGame && p.myTeam == player.myTeam) return false;
        if (p.buffManager.GetTrigger(BuffType.InvincibleFromBullets)) return false; 
        if (p.buffManager.GetTrigger(BuffType.MirrorDamage)) return false;
        return true;
    }

    public Vector3 lastEvaluatedVector = Vector3.zero;
    private void FixedUpdate()
    {
        machine.RemoveObjects();
        machine.FindNearByObjects();
        machine.FindNearByPlayers();
    }
    private void Update()
    {
        lastEvaluatedVector = machine.EvaluateMoves();
    }


    public float EvaluateAim()
    {
        FindNearestPlayer();
        if (targetEnemy == null)
        {
            return player.movement.aimAngle;
        }
        aimAngle = machine.PredictAim(player,targetEnemy); 
        aimAngle = machine.DiffuseAim (aimAngle);
       // Debug.LogWarning("Aim " + aimAngle+" original "+GameSession.GetAngle(sourcePosition, targetPosition));
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
public enum BotType
{
    None, Easy,Normal, Hard
}