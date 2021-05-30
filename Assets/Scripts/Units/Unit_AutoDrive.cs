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
    Dictionary<int, object> componentDictionary = new Dictionary<int, object>();
    SkillManager skillManager;
    Unit_Movement movement;
    internal GameObject directionIndicator;
    float range = 9f;
    float attackRange = 15f;
    float escapePadding = 1f;

    public GameObject targetEnemy;
    int myInstanceID;
    [SerializeField] Unit_Player player;
   internal float aimAngle;
    Vector3 xWall, yWall;
    public AI_ATTACK_TYPE aiAttackType = AI_ATTACK_TYPE.STANDARD;

    public void SetInfo() {
        movement = player.movement;
        skillManager = player.skillManager;
        directionIndicator = player.driverIndicator;
        myInstanceID = player.gameObject.GetInstanceID();
        DetermineAttackType();
    }
    void DetermineAttackType() {
        switch (player.myCharacter)
        {
            case CharacterType.ASAKURA:
            case CharacterType.KIMIDORI:
                aiAttackType =  AI_ATTACK_TYPE.ANYTIME;
                break;
            case CharacterType.NAGATO:
            case CharacterType.KYOUKO:
            case CharacterType.TSURUYA:
                aiAttackType = AI_ATTACK_TYPE.STANDARD;
                break;
            case CharacterType.HARUHI:
            case CharacterType.KOIZUMI:
            case CharacterType.SASAKI:
            case CharacterType.KOIHIME:
            case CharacterType.KUYOU:
            case CharacterType.YASUMI:
                aiAttackType = AI_ATTACK_TYPE.SHORT;
                break;
            case CharacterType.MIKURU:
                aiAttackType = AI_ATTACK_TYPE.LONG;
                break;
            default:
                aiAttackType = AI_ATTACK_TYPE.STANDARD;
                break;
        }
        switch (aiAttackType)
        {
            case AI_ATTACK_TYPE.ANYTIME:
                attackRange = 999f;
                break;
            case AI_ATTACK_TYPE.SHORT:
                attackRange = range  * 0.5f;
                break;
            case AI_ATTACK_TYPE.LONG:
                attackRange = range *1.5f;
                break;
            case AI_ATTACK_TYPE.STANDARD:
                attackRange = range ;
                break;
        }
    }


    SortedDictionary<string,Unit_Player> playersOnMap;
    private void OnEnable()
    {
        SetInfo();
        foundObjects = new Dictionary<int, GameObject>();
        EventManager.StartListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StartListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
    }
    private void Start()
    {
        FindNearestPlayer();
    }
    public bool CanAttackTarget() {
        if (GameSession.gameModeInfo.isCoop) return true;
        if (targetEnemy == null) {
            FindNearestPlayer();
        }
        if (targetEnemy == null)
        {
            return false;
        }
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
                    HealthPoint hp = CacheComponent<HealthPoint>(tid, c.gameObject);
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
        playersOnMap = GameFieldManager.gameFields[player.fieldNo].playerSpawner.unitsOnMap;
        float nearestEnemyDist = 0f;
        foreach (var p in playersOnMap.Values) {
            if (p == null || !p.gameObject.activeInHierarchy) continue;
            if (p.gameObject.GetInstanceID() == myInstanceID) continue;
            if (GameSession.gameModeInfo.isTeamGame && p.myTeam == player.myTeam) continue;
            float dist = Vector2.Distance(movement.networkPos, p.gameObject.transform.position);
            if (dist < nearestEnemyDist || targetEnemy == null) {
                nearestEnemyDist = dist;
                targetEnemy = p.gameObject;
            }
        }
      //  Debug.Log("Players " + playersOnMap.Count + " / " + targetEnemy);
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
    T CacheComponent<T>(int tag, GameObject go) {
        if (!componentDictionary.ContainsKey(tag))
        {
            T comp = go.GetComponent<T>();
            componentDictionary.Add(tag, comp);
        }
        else {
            if (componentDictionary[tag] == null)
            {
                T comp = go.GetComponent<T>();
                componentDictionary[tag] = comp;
            }
        }
        return (T)componentDictionary[tag];
    }



    public Vector3 EvaluateMoves() {
        Vector3 move = Vector3.zero;
        foreach (GameObject go in foundObjects.Values) {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            int tid = go.GetInstanceID();
            Vector3 directionToTarget = go.transform.position - movement.networkPos;
            directionToTarget.Normalize();
            float distance = Vector3.Distance(go.transform.position, movement.networkPos) - Mathf.Max(go.transform.localScale.x, go.transform.localScale.y);
            if (distance > range) continue;
            float multiplier = GetMultiplier(distance);
            switch (go.tag) {
                case TAG_PLAYER:
                    Unit_Player unitPlayer = CacheComponent<Unit_Player>(tid, go);
                    if (GameSession.gameModeInfo.isTeamGame && unitPlayer.myTeam == player.myTeam) continue;
                    if (GameSession.gameModeInfo.isCoop) continue;
                    bool positive = ((skillManager.currStack > 0 ||
                        skillManager.skillInUse )||
                        skillManager.buffManager.GetTrigger(BuffType.InvincibleFromBullets)
                        &&
                        (!unitPlayer.buffManager.GetTrigger(BuffType.InvincibleFromBullets)));

                    move += positive ? directionToTarget * multiplier : -directionToTarget * multiplier;
                    break;
                case TAG_PROJECTILE:
                    move -= directionToTarget * multiplier;
                    break;
                case TAG_BUFF_OBJECT:
                    BoxCollider2D boxColl = CacheComponent<BoxCollider2D>(tid, go);
                    if (boxColl.enabled) {
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
        else if (move.magnitude <= 0.25f) {
            move = Vector3.zero;
        }
      //  Debug.Log("Final move " + move +" mag "+move.magnitude + " / "+move.sqrMagnitude);
        return move;
    }

    Vector3 GetAwayFromWalls_2()
    {
        Vector3 move = Vector3.zero;
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
public enum AI_ATTACK_TYPE { 
    ANYTIME,SHORT,LONG,STANDARD
}