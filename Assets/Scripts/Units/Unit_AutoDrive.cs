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
    float range = 9f;
    float attackRange = 9f;
    float escapePadding = 1f;

    public GameObject targetEnemy;
    int myInstanceID;
    [SerializeField] Unit_Player player;
   internal float aimAngle;

    Vector3 xWall, yWall;
    public void SetInfo() {
        movement = player.movement;
        skillManager = player.skillManager;
        directionIndicator = player.driverIndicator;
        myInstanceID = player.gameObject.GetInstanceID();
    
    }


    SortedDictionary<string,Unit_Player> playersOnMap;
    private void OnEnable()
    {
        SetInfo();
        foundObjects = new Dictionary<int, GameObject>();
        EventManager.StartListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StartListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
        playersOnMap = GameFieldManager.gameFields[player.fieldNo].playerSpawner.unitsOnMap;
    }
    private void Start()
    {
        FindNearestPlayer();
    }
    public bool CanAttackTarget() {
        if (GameSession.gameModeInfo.isCoop) return true;
        if (targetEnemy == null) return false;
        return (Vector2.Distance(targetEnemy.transform.position,transform.position) <= attackRange) ;
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
        Gizmos.DrawWireSphere(xWall, 1f);
        Gizmos.DrawWireSphere(yWall, 1f);
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

        foreach (var key in keys) {
            GameObject go = foundObjects[key];
            if (go == null || !go.activeInHierarchy)
            {
                foundObjects.Remove(key);
            }
            else if(go.tag != TAG_BOX_OBSTACLE)
            {
                float dist = Vector2.Distance(movement.networkPos, go.transform.position);
                if (dist > (range + escapePadding))
                {
                    //   Debug.Log("Remove for out of dist " + dist + " / " + (range + escapePadding));
                    foundObjects.Remove(key);
                }
            }
        }
    }
    void FindNearestPlayer() {
        float nearestEnemyDist = 0f;
        foreach (var p in playersOnMap.Values) {
            if (p == null || !p.gameObject.activeInHierarchy) continue;
            if (p.gameObject.GetInstanceID() == myInstanceID) continue;
            float dist = Vector2.Distance(movement.networkPos, p.gameObject.transform.position);
           // if (dist <= Mathf.Epsilon) continue;
            if (dist < nearestEnemyDist || targetEnemy == null) {
                nearestEnemyDist = dist;
                targetEnemy = p.gameObject;
            }
        }
    }
    Vector3 lastVector = Vector3.zero;
    private void FixedUpdate()
    {
        RemoveObjects();
        FindNearByObjects();
        lastVector = EvaluateMoves();
        //EvaluateAim();
    }

    public float EvaluateAim()
    {
        FindNearestPlayer();
        Vector3 targetPosition = (targetEnemy == null) ? lastVector : targetEnemy.transform.position;
        Vector3 sourcePosition = (targetEnemy == null) ? Vector3.zero : movement.networkPos;
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
                    move += (skillManager.currStack > 0 || skillManager.skillInUse) ? directionToTarget * multiplier : -directionToTarget * multiplier;
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
          //  Debug.Log("Inter move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        }

        move += GetAwayFromWalls_2();
       // Debug.Log("Wall move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }
        else if (move.sqrMagnitude <= 0.03f) {
            move = Vector3.zero;
        }
      //  Debug.Log("Final move " + move +" mag "+move.magnitude + " / "+move.sqrMagnitude);
        return move;
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
    Vector3 GetAwayFromWalls_2()
    {
        Vector3 move = Vector3.zero;
/*        if (
            Mathf.Abs(movement.networkPos.x - movement.mapSpec.xMid) < Mathf.Epsilon
            &&
            Mathf.Abs(movement.networkPos.y - movement.mapSpec.yMid) < Mathf.Epsilon
         )
        {
            return move;
        }*/
     
        float xBound = (movement.networkPos.x < movement.mapSpec.xMid) ? movement.mapSpec.xMin : movement.mapSpec.xMax;
        float yBound = (movement.networkPos.y < movement.mapSpec.yMid) ? movement.mapSpec.yMin : movement.mapSpec.yMax;

        xWall = new Vector3(movement.networkPos.x, yBound);
        if (Vector2.Distance(xWall, movement.networkPos) <= range) {
            move += EvaluateToPoint(xWall, false, 2f);
        }

        yWall= new Vector3(xBound,  movement.networkPos.y);
        if (Vector2.Distance(yWall, movement.networkPos) <= range)
        {
            move += EvaluateToPoint(yWall, false, 2f);
        }

     //   Debug.Log("Center " + (dirToCenter * GetMultiplier(centerDist)) + " bound " + (-dirToBound * GetMultiplier(boundDist)) + " move " + move);
        return move;
    }
    Vector3 EvaluateToPoint(Vector3 point, bool positive, float flavour = 1f)
    {
        Vector3 dirToPoint = point - movement.networkPos;
        dirToPoint.Normalize();
        float dist = Vector2.Distance(point, movement.networkPos);
        Vector3 direction = dirToPoint * GetMultiplier(dist) * flavour;
        if (!positive) direction *= -1f;
        return direction;
    }

    float GetMultiplier(float x) {
        float y = (1 / Mathf.Pow(x+ 2,2)) * 48;
        return y;
    }
}
