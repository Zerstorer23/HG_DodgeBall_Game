using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
    float range;
    float attackRange = 10f;
    float collisionRange;
    float escapePadding = 1f;

    public GameObject targetEnemy;
    int myInstanceID;
    [SerializeField] Unit_Player player;
    internal float aimAngle;
    Vector3 xWall, yWall;
    public AI_ATTACK_TYPE aiAttackType = AI_ATTACK_TYPE.STANDARD;

    public List<GameObject> watchList = new List<GameObject>();
    List<Vector3> collisionList = new List<Vector3>();
    public bool secondPrediction = true;
    public void SetInfo()
    {
        movement = player.movement;
        skillManager = player.skillManager;
        directionIndicator = player.driverIndicator;
        myInstanceID = player.gameObject.GetInstanceID();
        range = 15f;
        collisionRange = range / 2f + GetRadius(transform.localScale) + 0.4f;
        DetermineAttackType();
    }
    void DetermineAttackType()
    {
        switch (player.myCharacter)
        {
            case CharacterType.KIMIDORI:
                aiAttackType = AI_ATTACK_TYPE.ANYTIME;
                break;
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
            case CharacterType.NAGATO:
            case CharacterType.ASAKURA:
            case CharacterType.KYOUKO:
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
                attackRange = range * 0.5f;
                break;
            case AI_ATTACK_TYPE.LONG:
                attackRange = range * 2f;
                break;
            case AI_ATTACK_TYPE.STANDARD:
                attackRange = range;
                break;
        }
    }


    SortedDictionary<string, Unit_Player> playersOnMap;
    private void OnEnable()
    {
        SetInfo();
        foundObjects = new Dictionary<int, GameObject>();
        EventManager.StartListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StartListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
    }

    public bool CanAttackTarget()
    {
        if (GameSession.gameModeInfo.isCoop) return true;
        if (targetEnemy == null)
        {
            FindNearestPlayer();
        }
        if (targetEnemy == null)
        {
            return false;
        }
        return (Vector2.Distance(targetEnemy.transform.position, transform.position) <= attackRange);
    }
    private void OnDisable()
    {

        EventManager.StopListening(MyEvents.EVENT_BOX_SPAWNED, OnBoxSpawned);
        EventManager.StopListening(MyEvents.EVENT_BOX_ENABLED, OnBoxEnabled);
    }

    private void OnBoxSpawned(EventObject arg0)
    {
        foundObjects.Add(arg0.goData.GetInstanceID(), arg0.goData);
    }

    private void OnBoxEnabled(EventObject arg0)
    {
        foundObjects.Remove(arg0.goData.GetInstanceID());
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        /*        foreach (GameObject go in foundObjects.Values)
                {
                    if (go == null || !go.activeInHierarchy)
                    {
                        continue;
                    }
                    Gizmos.DrawWireSphere(go.transform.position, 0.5f);
                }*/

        Gizmos.DrawWireSphere(transform.position + lastEvaluatedVector, 0.5f);
        Gizmos.DrawWireSphere(xWall, 1f);
        Gizmos.DrawWireSphere(yWall, 1f);
        if (targetEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetEnemy.transform.position, 1f);
        }
        Gizmos.color = Color.cyan;
        foreach (var go in watchList)
        {
            if (go == null) continue;
            Gizmos.DrawWireSphere(go.transform.position, 0.6f);
        }
        Gizmos.color = Color.green;
        foreach (var go in collisionList)
        {
            Gizmos.DrawWireSphere(go, 0.7f);
        }
    }
    // Update is called once per frame


    void FindNearByObjects()
    {
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
                        Unit_Player unitPlayer = CacheComponent<Unit_Player>(tid, c.gameObject);
                        if (GameSession.gameModeInfo.isTeamGame && unitPlayer.myTeam == player.myTeam) continue;
                        if (GameSession.gameModeInfo.isCoop) continue;
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

        foreach (var key in keys)
        {
            GameObject go = foundObjects[key];
            if (go == null || !go.activeInHierarchy)
            {
                foundObjects.Remove(key);
            }
            else if (go.tag != TAG_BOX_OBSTACLE)
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
    void FindNearestPlayer()
    {
        playersOnMap = GameFieldManager.gameFields[player.fieldNo].playerSpawner.unitsOnMap;
        float nearestEnemyDist = 0f;
        foreach (var p in playersOnMap.Values)
        {
            if (p == null || !p.gameObject.activeInHierarchy) continue;
            if (p.gameObject.GetInstanceID() == myInstanceID) continue;
            if (GameSession.gameModeInfo.isTeamGame && p.myTeam == player.myTeam) continue;
            float dist = Vector2.Distance(movement.networkPos, p.gameObject.transform.position);
            if (dist < nearestEnemyDist || targetEnemy == null)
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
        RemoveObjects();
        FindNearByObjects();
        //EvaluateAim();
        FindNearestPlayer();
        lastEvaluatedVector = EvaluateMoves();
    }
    /*    private void Update()
        {

        }*/

    public float EvaluateAim()
    {
        FindNearestPlayer();
        Vector3 targetPosition = (targetEnemy == null) ? lastEvaluatedVector : targetEnemy.transform.position;
        Vector3 sourcePosition = (targetEnemy == null) ? Vector3.zero : movement.networkPos;
        aimAngle = GameSession.GetAngle(sourcePosition, targetPosition);
        directionIndicator.transform.localPosition = GetAngledVector(aimAngle, 1.4f); // new Vector3(dX, dY);
        directionIndicator.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);
        return aimAngle;
    }
    T CacheComponent<T>(int tag, GameObject go)
    {
        if (!componentDictionary.ContainsKey(tag))
        {
            T comp = go.GetComponent<T>();
            componentDictionary.Add(tag, comp);
        }
        else
        {
            if (componentDictionary[tag] == null)
            {
                T comp = go.GetComponent<T>();
                componentDictionary[tag] = comp;
            }
        }
        return (T)componentDictionary[tag];
    }



    Vector3 EvaluateMoves()
    {
        Vector3 move = Vector3.zero;
        watchList = new List<GameObject>();
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            int tid = go.GetInstanceID();
            Vector3 directionToTarget = go.transform.position - movement.networkPos;
            directionToTarget.Normalize();
            float distance = Vector3.Distance(go.transform.position, movement.networkPos) - GetRadius(go.transform.localScale);
            if (distance > range) continue;

            float multiplier = GetMultiplier(distance);
            switch (go.tag)
            {
                case TAG_PLAYER:
                    Unit_Player unitPlayer = CacheComponent<Unit_Player>(tid, go);
                    bool positive = ((skillManager.currStack > 0 ||
                        skillManager.skillInUse) ||
                        skillManager.buffManager.GetTrigger(BuffType.InvincibleFromBullets)
                        &&
                        (!unitPlayer.buffManager.GetTrigger(BuffType.InvincibleFromBullets))
                        );
                    multiplier *= 2;
                    move += positive
                        ? directionToTarget * multiplier :
                        -directionToTarget * multiplier;
                    break;
                case TAG_PROJECTILE:
                    HealthPoint hp = CacheComponent<HealthPoint>(tid, go);
                    Projectile_Movement pMove = (Projectile_Movement)hp.movement;
                    float targetSpeed = pMove.moveSpeed;
                    float targetAngle = pMove.eulerAngle;
                   // float speedMod = GetSpeedModifier(player.movement.GetMovementSpeed(), pMove.moveSpeed);
                    if (pMove.characterUser == CharacterType.NAGATO)
                    {
                        move -= RotateClockWise(directionToTarget * multiplier * 5f);
                        break;
                    }

                    if (targetSpeed > player.movement.moveSpeed)
                    {
                        OBJECT_APPROACH_STATUS approach = IsApproaching(pMove, distance);
                        //   if (approach == OBJECT_APPROACH_STATUS.AWAY) continue;

                        if (distance < (range / 3))
                        {
                            watchList.Add(go);
                        }
                        //Debug.Log(approach);
                        //move += directionToTarget * multiplier * 2;
                        Vector3 direction;
                        if (pMove.moveType == MoveType.Straight)
                        {
                            Debug.Log("Side evasion");
                            direction = RotateClockWise(GetAngledVector(targetAngle, multiplier) * 2f);
                        }
                        else
                        {
                            Debug.Log("Back evasion");
                            direction = directionToTarget * multiplier *3f;
                        }

                        move -= direction;

                    }
                    else
                    {
                        move -= directionToTarget * multiplier ;
                    }

                    break;
                case TAG_BUFF_OBJECT:
                    BoxCollider2D boxColl = CacheComponent<BoxCollider2D>(tid, go);
                    if (boxColl.enabled)
                    {
                        move += directionToTarget * multiplier * 2;
                    }
                    break;
                case TAG_BOX_OBSTACLE:
                    move -= directionToTarget * multiplier;
                    if (distance < (range / 3))
                    {
                        watchList.Add(go);
                    }
                    break;
            }
            //  Debug.Log("Inter move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        }

        move += GetAwayFromWalls_2();
        // Debug.Log("Wall move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        if (move.magnitude > 1f)
        {
            move.Normalize();
            if (secondPrediction)
            {
                //    move = FindEmptySpace(move);
            }
        }
        else if (move.magnitude <= 0.25f)
        {
            move = Vector3.zero;
        }


        //  Debug.Log("Final move " + move +" mag "+move.magnitude + " / "+move.sqrMagnitude);
        return move;
    }
    Vector3 RotateClockWise(Vector3 direction)
    {
        return new Vector3(direction.y, -direction.x);
    }

    OBJECT_APPROACH_STATUS IsApproaching(Projectile_Movement projMove, float dist)
    {
        float offset = 90f;
        if ((dist
            // -GetRadius(projMove.transform.localScale)
            ) < collisionRange) return OBJECT_APPROACH_STATUS.COLLIDING;
        float myAngle = GetAngleBetween(movement.networkPos, projMove.transform.position);
        float angleDiff = Mathf.Abs(myAngle - projMove.eulerAngle);
        //Debug.Log(projMove.gameObject.name+": "+angleDiff);
        return ((angleDiff > (180 - offset)) && (angleDiff < (180 + offset))) ? OBJECT_APPROACH_STATUS.APPROACHING : OBJECT_APPROACH_STATUS.AWAY;
    }
    Vector3 FindEmptySpace(Vector3 heuristicDirection)
    {
        collisionList = new List<Vector3>();
        if (watchList.Count == 0) return heuristicDirection;
        float duration = 1f;
        int searched = 0;
        foreach (var go in watchList)
        {
            collisionList.Add(go.transform.position);
            HealthPoint hp = CacheComponent<HealthPoint>(go.GetInstanceID(), go);
            if (hp != null && hp.movement != null)
            {
                ((Projectile_Movement)hp.movement).GetExpectedPosition(collisionList, transform.position, collisionRange, duration);
            }
            searched++;
        }
        float originalAngle = GetAngleBetween(Vector3.zero, RotateClockWise(heuristicDirection));
        float save = originalAngle;
        float length = movement.GetMovementSpeed() * Time.fixedDeltaTime;
        Vector3 advisedDirection = Vector3.zero;
        Debug.Log("Collision points " + collisionList.Count);
        for (int i = 0; i < 360 / 60; i++)
        {
            bool collided = false;
            originalAngle += 60;
            advisedDirection = GetAngledVector(originalAngle, length);
            Vector3 expectedPos = transform.position + advisedDirection;
            foreach (var collisionPoint in collisionList)
            {
                searched++;
                if (Vector2.Distance(expectedPos, collisionPoint) < 1.5f)
                {
                    //Debug.LogWarning("Expected collision at " + originalAngle);
                    collided = true;
                    break;
                }
            }
            if (!collided) break;
        }
        if (advisedDirection == Vector3.zero)
        {
            // Debug.LogError("no move");
            return heuristicDirection;
        }
        Debug.LogWarning(searched + ": " + save + " => " + originalAngle);
        return advisedDirection.normalized;
    }

    Vector3 GetAwayFromWalls_2()
    {
        Vector3 move = Vector3.zero;
        float xBound = (movement.networkPos.x < movement.mapSpec.xMid) ? movement.mapSpec.xMin : movement.mapSpec.xMax;
        float yBound = (movement.networkPos.y < movement.mapSpec.yMid) ? movement.mapSpec.yMin : movement.mapSpec.yMax;

        xWall = new Vector3(movement.networkPos.x, yBound);
        if (Vector2.Distance(xWall, movement.networkPos) <= range)
        {
            move += EvaluateToPoint(xWall, false, 2f);
        }

        yWall = new Vector3(xBound, movement.networkPos.y);
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
    public float GetSpeedModifier(float mySpeed, float targetSpeed)
    {
        if (mySpeed > targetSpeed) return 1f;
        return Mathf.Pow(((targetSpeed - mySpeed)), 2);
    }

    float GetMultiplier(float x)
    {
        float y = (1 / Mathf.Pow(x + 2, 2)) * 48;
        //  float y = Mathf.Pow((range - x), 3);
        return y;
    }
}
public enum AI_ATTACK_TYPE
{
    ANYTIME, SHORT, LONG, STANDARD
}
public enum OBJECT_APPROACH_STATUS
{
    APPROACHING, AWAY, COLLIDING
}