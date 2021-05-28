using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;
using static GameFieldManager;

public class Unit_AutoDrive : MonoBehaviour
{
    [SerializeField] Dictionary<int, GameObject> foundObjects = new Dictionary<int, GameObject>();
    SkillManager skillManager;
    Unit_Movement movement;
    internal GameObject directionIndicator;
    public float range;
    float escapePadding = 1f;

    public GameObject targetEnemy;
    int myInstanceID;
    [SerializeField] Unit_Player player;
   internal float aimAngle;

    public void SetInfo() {
        movement = player.movement;
        skillManager = player.skillManager;
        directionIndicator = player.driverIndicator;
        myInstanceID = player.gameObject.GetInstanceID();
    
    }
    private void Awake()
    {
        SetRange(9f);
    }
    public void SetRange(float a) {
        range = a;
    }
    private void OnEnable()
    {
        SetInfo();
        foundObjects = new Dictionary<int, GameObject>();
        EventManager.StartListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StartListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
    }
    private void OnDisable()
    {

        EventManager.StopListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StopListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
    }

    private void OnBoxSpawned(EventObject arg0)
    {
        foundObjects.Add(arg0.goData.GetInstanceID(),arg0.goData);
    }

    private void OnBoxEnabled(EventObject arg0)
    {
        foundObjects.Remove(arg0.goData.GetInstanceID());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            Gizmos.DrawWireSphere(go.transform.position, 0.5f);
        }
    }
    // Update is called once per frame


    void FindNearByObjects() {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(
            movement.networkPos, range, LayerMask.GetMask(TAG_PLAYER, TAG_PROJECTILE, TAG_BUFF_OBJECT));

        for (int i = 0; i < collisions.Length; i++)
        {
            Collider2D c = collisions[i];
            int tid = c.gameObject.GetInstanceID();
            if (foundObjects.ContainsKey(tid)) continue;

            switch (c.gameObject.tag)
            {
                case TAG_PLAYER:
                    if (tid != myInstanceID)
                    {
                        foundObjects.Add(tid, c.gameObject);
                    }
                    break;
                case TAG_PROJECTILE:
                    HealthPoint hp = c.gameObject.GetComponent<HealthPoint>();
                    if (!hp.damageDealer.isMapObject && hp.pv.IsMine) continue;
                    foundObjects.Add(tid, c.gameObject);
                    break;
                case TAG_BUFF_OBJECT:
                    foundObjects.Add(tid, c.gameObject);
                    break;
            }
        }
    }
    void RemoveObjects()
    {
        List<int> keys = new List<int>(foundObjects.Keys);
        float nearestEnemyDist = 0f;

        foreach (var key in keys) {
            GameObject go = foundObjects[key];
            if (go == null || !go.activeInHierarchy)
            {
                foundObjects.Remove(key);
            }
            else if(go.tag != TAG_BOX_OBSTACLE)
            {
                float dist = Vector2.Distance(movement.networkPos, go.transform.position);
                if (go.tag == TAG_PLAYER)
                {
                    if (dist < nearestEnemyDist || targetEnemy == null)
                    {
                        targetEnemy = go;
                        nearestEnemyDist = dist;
                    }
                }
                if (dist > (range + escapePadding))
                {
                    //   Debug.Log("Remove for out of dist " + dist + " / " + (range + escapePadding));
                    foundObjects.Remove(key);
                }
            }
        }
    }

    Vector3 lastVector = Vector3.zero;
    private void FixedUpdate()
    {
        RemoveObjects();
        FindNearByObjects();
        lastVector = EvaluateMoves();
        EvaluateAim();
    }

    public float EvaluateAim()
    {
        Vector3 targetPosition = (targetEnemy == null) ? lastVector : targetEnemy.transform.position;
        Vector3 sourcePosition = (targetEnemy == null) ? Vector3.zero : transform.position;
        aimAngle = GameSession.GetAngle(sourcePosition, targetPosition);
        directionIndicator.transform.localPosition = GetAngledVector(aimAngle, 1.4f); // new Vector3(dX, dY);
        directionIndicator.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
        return aimAngle;
    }


    public Vector3 EvaluateMoves() {
        Vector3 move = Vector3.zero;
        foreach (GameObject go in foundObjects.Values) {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            Vector3 directionToTarget = go.transform.position - movement.networkPos;
            directionToTarget.Normalize();
            float distance = Vector3.Distance(go.transform.position, movement.networkPos) - Mathf.Max(go.transform.localScale.x, go.transform.localScale.y);
            if (distance > range) continue;
            float multiplier = GetMultiplier(distance);
            switch (go.tag) {
                case TAG_PLAYER:
                    move += (skillManager.currStack > 0)? directionToTarget * multiplier : -directionToTarget * multiplier;
                    break;
                case TAG_PROJECTILE:
                    move -= directionToTarget * multiplier;
                    break;
                case TAG_BUFF_OBJECT:
                    if (go.GetComponent<BoxCollider2D>().enabled) {
                        move += directionToTarget * multiplier *2;
                    }
                    break;
                case TAG_BOX_OBSTACLE:
                    move -= directionToTarget * multiplier;
                    break;
            }
        }

        move += GetAwayFromWalls();
        if (move == Vector3.zero)
        {
            move = lastVector;
        }

        return move.normalized;
    }

    Vector3 GetAwayFromWalls() {
        Vector3 center = new Vector3(movement.mapSpec.xMid, movement.mapSpec.yMid);
        float dist = Vector2.Distance(center, movement.networkPos);
        float notDistance = movement.mapSpec.xMid - movement.mapSpec.xMin;
        Vector3 dir = center - movement.networkPos;
        dir.Normalize();
        dir = dir * GetMultiplier(notDistance - dist);
        
        return dir;
    }

    float GetMultiplier(float x) {
        float y = (1 / Mathf.Pow(x+ 2,2)) * 48 - 0.4f;
        return y;
    }
}
