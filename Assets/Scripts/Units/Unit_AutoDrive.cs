using Photon.Pun;
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
    [SerializeField] Dictionary<int, GameObject> foundObjects = new Dictionary<int, GameObject>();
    Dictionary<int, object> componentDictionary = new Dictionary<int, object>();
    SkillManager skillManager;
    Unit_Movement movement;
    internal GameObject directionIndicator;
    float findRange;
    public float range_knn = 5f;
    public float range_collision = 3f;
    float attackRange = 10f;
    float escapePadding = 1f;

    public GameObject targetEnemy;
    int myInstanceID;
    [SerializeField] Unit_Player player;
    internal float aimAngle;
    Vector3 xWall, yWall;
    public AI_ATTACK_TYPE aiAttackType = AI_ATTACK_TYPE.STANDARD;

    public bool secondPrediction = true;

    bool isKamikazeSkill = false;
    public List<GameObject> collisionList = new List<GameObject>();
    public void SetInfo()
    {
        movement = player.movement;
        skillManager = player.skillManager;
        directionIndicator = player.driverIndicator;
        myInstanceID = player.gameObject.GetInstanceID();
        findRange = 20f;
        range_knn = (gameFields[player.fieldNo].bulletSpawner.activeMax <= 4) ? findRange : 5f;
        DetermineAttackType();
    }
    void DetermineAttackType()
    {
        switch (player.myCharacter)
        {
            case CharacterType.KIMIDORI:
            case CharacterType.MIKURU:
                attackRange = 999f;
                break;
            case CharacterType.TSURUYA:
                attackRange = 12;
                break;
            case CharacterType.HARUHI:
                attackRange = 5.5f;
                isKamikazeSkill = true;
                break;
            case CharacterType.KUYOU:
                attackRange = 5.6f;
                isKamikazeSkill = true;
                break;
            case CharacterType.KOIZUMI:
            case CharacterType.KOIHIME:
                attackRange = 10f;
                isKamikazeSkill = true;
                break;
            case CharacterType.SASAKI:
                attackRange = 8f;
                isKamikazeSkill = true;
                break;
            case CharacterType.YASUMI:
                attackRange = 4f;
                break;
            case CharacterType.NAGATO:
                attackRange = 6f;
                break;
            case CharacterType.ASAKURA:
            case CharacterType.KYOUKO:
                attackRange = 20f;
                break;
            case CharacterType.KYONMOUTO:
                attackRange = 5f;
                break;
            default:
                attackRange = findRange;
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
        Gizmos.DrawWireSphere(transform.position, findRange);
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            Gizmos.DrawWireSphere(go.transform.position, 0.5f);
        }
        Gizmos.color = (doApproach) ? Color.cyan : Color.red;
        Gizmos.DrawWireSphere(transform.position + lastEvaluatedVector, 0.6f);
        Gizmos.DrawWireSphere(xWall, 1f);
        Gizmos.DrawWireSphere(yWall, 1f);
        if (targetEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetEnemy.transform.position, 1f);
        }
        Gizmos.color = Color.green;
        for (int i = 0; i < 360; i++)
        {
            if (!blockedAngles[i])
            {
                Vector3 pos = transform.position + GetAngledVector(i, range_collision);
                Gizmos.DrawWireSphere(pos, 0.05f);
            }
        }

    }
    // Update is called once per frame


    void FindNearByObjects()
    {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(
            movement.networkPos, findRange, LayerMask.GetMask(TAG_PLAYER, TAG_PROJECTILE, TAG_BUFF_OBJECT));

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
                    if (CheckIfProjetilcIsDangerous(tid, c.gameObject))
                    {
                        foundObjects.Add(tid, c.gameObject);
                    }
                    break;
                case TAG_BUFF_OBJECT:
                    foundObjects.Add(tid, c.gameObject);
                    break;
                    /*                case TAG_WALL:
                                        foundObjects.Add(tid, c.gameObject);
                                        break;*/
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
                if (dist > (findRange + escapePadding))
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
            if (p.buffManager.GetTrigger(BuffType.InvincibleFromBullets)) continue;
            if (p.buffManager.GetTrigger(BuffType.MirrorDamage)) continue;
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
    }
    private void Update()
    {
        lastEvaluatedVector = EvaluateMoves();
    }

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
        collisionList = new List<GameObject>();
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            int tid = go.GetInstanceID();
            Vector3 directionToTarget = go.transform.position - movement.networkPos;
            directionToTarget.Normalize();
            float distance = Vector2.Distance(go.transform.position, movement.networkPos) - GetRadius(go.transform.localScale);
            //   if (distance > range) continue;
            float multiplier = 0f;
            switch (go.tag)
            {
                case TAG_PLAYER:
                    move += EvaluatePlayer(go, tid, directionToTarget, distance);
                    break;
                case TAG_PROJECTILE:
                    if (distance < range_knn)
                    {
                        collisionList.Add(go);
                    }
                    move += EvaluateProjectile(tid, go, distance, directionToTarget);

                    break;
                case TAG_BUFF_OBJECT:
                    multiplier = GetMultiplier(distance);
                    BoxCollider2D boxColl = CacheComponent<BoxCollider2D>(tid, go);
                    if (boxColl.enabled)
                    {
                        move += directionToTarget * multiplier * 2;
                    }
                    break;
                case TAG_BOX_OBSTACLE:
                    multiplier = GetMultiplier(distance);
                    move -= directionToTarget * multiplier;
                    break;
                    /*                case TAG_WALL:
                                        multiplier = GetMultiplier(distance);
                                        move -= directionToTarget * multiplier * 0.5f;
                                        break;*/
            }
            //  Debug.Log("Inter move " + move + " mag " + move.magnitude + " / " + move.sqrMagnitude);
        }

        move += GetAwayFromWalls_2();
        if (collisionList.Count > 0)
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

        move = AbsoluteEvasion(move);

        //  Debug.Log("Final move " + move +" mag "+move.magnitude + " / "+move.sqrMagnitude);
        return move;
    }
    public bool doApproach = false;
    private Vector3 EvaluatePlayer(GameObject go, int tid, Vector3 directionToTarget, float distance)
    {
        float multiplier = GetMultiplier(distance);
        Unit_Player unitPlayer = CacheComponent<Unit_Player>(tid, go);
        bool skillAvailable = skillManager.SkillIsReady();
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
            multiplier += (distance < 4f) ?
                    -GetMultiplier(distance / 2f)
                : GetMultiplier(distance / 2f);
            return directionToTarget * multiplier;
        }

        if (doApproach)
        {
            switch (player.myCharacter)
            {
                case CharacterType.NAGATO:
                case CharacterType.HARUHI:
                case CharacterType.KOIZUMI:
                case CharacterType.KUYOU:
                case CharacterType.SASAKI:
                case CharacterType.KOIHIME:
                    if (isKamikazeSkill)
                    {
                        multiplier = GetMultiplier(distance / 2f);
                    }
                    else
                    {
                        multiplier = GetMultiplier(distance / 1.5f);
                    }
                    break;
            }
        }
        else
        {
            switch (unitPlayer.myCharacter)
            {
                case CharacterType.NAGATO:
                case CharacterType.MIKURU:
                case CharacterType.HARUHI:
                case CharacterType.KOIZUMI:
                case CharacterType.SASAKI:
                case CharacterType.KOIHIME:
                    multiplier = -GetMultiplier(distance / 5f);
                    break;
                case CharacterType.KIMIDORI:
                    multiplier = (distance < 4f) ?
                          GetMultiplier(distance / 5f)
                        : -GetMultiplier(distance / 5f);
                    break;
                default:
                    multiplier *= -1f;
                    break;
            }
        }
        return directionToTarget * multiplier;
    }

    private Vector3 EvaluateProjectile(int tid, GameObject go, float distance, Vector3 directionToTarget)
    {
        if (isKamikazeSkill && skillManager.SkillInUse())
        {
            return Vector3.zero;
        }
        HealthPoint hp = CacheComponent<HealthPoint>(tid, go);
        Projectile_Movement pMove = (Projectile_Movement)hp.movement;
        float multiplier = GetMultiplier(distance);
        // float speedMod = GetSpeedModifier(player.movement.GetMovementSpeed(), pMove.moveSpeed);
        if (pMove.characterUser == CharacterType.NAGATO)
        {
            Vector3 dir = PerpendicularEscape(pMove, directionToTarget) * GetMultiplier(distance / 3f);
            //  Debug.Log("Move to " + dir);
            return dir;
        }

        if (pMove.moveSpeed > player.movement.GetMovementSpeed())
        {
            if (pMove.moveType == MoveType.Straight)
            {
                float speedDiff = pMove.moveSpeed / player.movement.GetMovementSpeed();
                Vector3 dir = PerpendicularEscape(pMove, directionToTarget) * GetMultiplier(distance / speedDiff);
                //   Debug.Log("Move to " + dir);
                return dir;
            }
            else
            {
                // Debug.Log("Back evasion");
                return -directionToTarget * multiplier;
            }


        }
        else
        {
            return -directionToTarget * multiplier;
        }

    }

    Vector3 PerpendicularEscape(Projectile_Movement projMove, Vector3 dirToTarget)
    {
        float guessedAngle = GetAngleBetween(Vector3.zero, dirToTarget);
        float currDist = Vector2.Distance(projMove.transform.position, transform.position);
        //  Debug.Log("Original proj " + projMove.transform.position + " Angle " + projMove.transform.rotation.eulerAngles.z+" length "+ currDist+" dsir to target" +dirToTarget);
        Vector3 translation = GetAngledVector(projMove.transform.rotation.eulerAngles.z, currDist);
        Vector3 projPos = projMove.transform.position + translation;
        //  Debug.Log("Translation " + translation);
        float highDist = 0f;
        float highAngle = -1f;
        for (float angle = -90f; angle <= 90f; angle += 180f)
        {
            float iterAngle = (guessedAngle + angle) % 360;
            Vector3 myPos = transform.position + GetAngledVector(iterAngle, movement.GetMovementSpeed() * Time.fixedDeltaTime);
            float dist = Vector2.Distance(myPos, projPos);
            //  Debug.Log("Original proj " + projMove.transform.position + " my original " + transform.position);
            //   Debug.Log(iterAngle+": projectile at "+projPos+" Me at"+(myPos)+" relativeVector"+ (projPos - myPos)+":"+((angle<0)?"Clock ":"AntiClock") + " = > " + dist);
            if (highAngle == -1f || dist > highDist)
            {
                highDist = dist;
                highAngle = iterAngle % 360f;

            }
        }
        //    Debug.Log("Recommend angle " + highAngle+" dist "+highDist+" vector "+ GetAngledVector(highAngle, dirToTarget.magnitude));
        return GetAngledVector(highAngle, dirToTarget.magnitude);
    }

    Vector3 GetAwayFromWalls_2()
    {
        if (isKamikazeSkill && skillManager.SkillInUse())
        {
            return Vector3.zero;
        }
        int activeMax = gameFields[player.fieldNo].bulletSpawner.activeMax;
        Vector3 move = Vector3.zero;
        float xBound = (movement.networkPos.x < movement.mapSpec.xMid) ? movement.mapSpec.xMin : movement.mapSpec.xMax;
        float yBound = (movement.networkPos.y < movement.mapSpec.yMid) ? movement.mapSpec.yMin : movement.mapSpec.yMax;

        xWall = new Vector3(movement.networkPos.x, yBound);
        float mod = (activeMax == 0) ? 1f : 2f;
        if (Vector2.Distance(xWall, movement.networkPos) <= findRange)
        {
            move += EvaluateToPoint(xWall, false, mod);
        }

        yWall = new Vector3(xBound, movement.networkPos.y);
        if (Vector2.Distance(yWall, movement.networkPos) <= findRange)
        {
            move += EvaluateToPoint(yWall, false, mod);
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

    [SerializeField] BitArray blockedAngles = new BitArray(360, false);
    Vector3 AbsoluteEvasion(Vector3 finalDir)
    {
        int searchRange = 15;
        blockedAngles.SetAll(false);
        // Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, collideRadius, LayerMask.GetMask(TAG_PROJECTILE));
        foreach (GameObject go in foundObjects.Values)
        {
            if (go == null || !go.activeInHierarchy) continue;
            if (go.tag != TAG_PROJECTILE) continue;
            if (Vector2.Distance(go.transform.position, transform.position) > range_collision) continue;
            if (!CheckIfProjetilcIsDangerous(go.GetInstanceID(), go)) continue;
            int angleToObj = (int)GetAngleBetween(movement.networkPos, go.transform.position);
            int startAngle = (angleToObj - searchRange);
            if (startAngle < 0) startAngle += 360;
            /*  int endAngle = angleToObj + searchRange;
              if (endAngle > 360) endAngle %= 360;*/
            for (float i = 0; i < searchRange * 2; i++)
            {
                int angleIndex = (int)((startAngle + i) % 360f);
                if (angleIndex >= 360) angleIndex %= 360;
                blockedAngles[angleIndex] = true;
            }
        }
        int initAngle = (int)(GetAngleBetween(Vector3.zero, finalDir));
        int searchIndex = initAngle - searchRange;
        if (searchIndex < 0) searchIndex += 360;
        int continuousCount = 0;
        int numSearch = 0;
        int endCount = searchRange * 2;
        while (continuousCount < endCount && numSearch < 360)
        {
            if (!blockedAngles[searchIndex])
            {
                continuousCount++;
            }
            else
            {
                continuousCount = 0;
            }
            searchIndex++;
            if (searchIndex >= 360) searchIndex %= 360;
            numSearch++;
        }
        float finalAngle = searchIndex - searchRange;
        if (finalAngle < 0) finalAngle += 360f;
        //   if (finalAngle != initAngle) Debug.LogWarning("Modify " + initAngle + " => " + finalAngle+" search count "+numSearch);
        return GetAngledVector(finalAngle, finalDir.magnitude);
    }
    Vector3 Drive_KNN()
    {
        float angleStep = 10f;
        float moveDist = Mathf.Max(movement.GetMovementSpeed() * Time.deltaTime, 1f);

        float maxDist = 0f;
        float maxAngle = 0f;
        int minDanger = -1;
        for (float currAngle = 0; currAngle < 360f; currAngle += angleStep)
        {
            Vector3 newPos = GetAngledVector(currAngle, moveDist) + transform.position;
            float totalDist = 0f;
            int dangerCount = 0;
            foreach (GameObject go in collisionList)
            {
                Projectile_Movement pMove = (Projectile_Movement)CacheComponent<HealthPoint>(go.GetInstanceID(), go).movement;
                Vector3 expectedTargetPos = pMove.GetNextPosition();
                float currDist = Vector2.Distance(newPos, expectedTargetPos);
                if (currDist < range_knn)
                {
                    totalDist += Mathf.Pow(currDist, 2);
                    dangerCount++;
                }
            }
            /*            float centerDist = Vector2.Distance(new Vector2(movement.mapSpec.xMid, movement.mapSpec.yMid), transform.position);
                        float boundDist = Vector2.Distance(new Vector2(movement.mapSpec.xMin, movement.mapSpec.yMin), new Vector2(movement.mapSpec.xMid, movement.mapSpec.yMid));


                        totalDist += Mathf.Pow((boundDist - centerDist), 2);*/
            float wallDist = Vector2.Distance(newPos, xWall);
            if (wallDist < range_knn * 2)
            {
                totalDist += Mathf.Pow(wallDist, 2);
                dangerCount++;
            }
            wallDist = Vector2.Distance(newPos, yWall);
            if (wallDist < range_knn * 2)
            {
                totalDist += Mathf.Pow(wallDist, 2);
                dangerCount++;
            }
            if (dangerCount < minDanger || minDanger < 0)
            {
                maxDist = totalDist;
                maxAngle = currAngle;
            }
            else if(dangerCount == minDanger){
                if (totalDist > maxDist)
                {
                    maxDist = totalDist;
                    maxAngle = currAngle;
                }
            }
        }

        return GetAngledVector(maxAngle, moveDist);
    }
    bool CheckIfProjetilcIsDangerous(int tid, GameObject go)
    {
        HealthPoint hp = CacheComponent<HealthPoint>(tid, go);
        if (!hp.damageDealer.isMapObject && hp.pv.IsMine) return false;
        if (GameSession.gameModeInfo.isTeamGame && hp.myTeam == player.myTeam) return false;
        return true;
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
public enum APPROACH_STATUS
{
    APPROACHING_GOANTI, APPROACH_GOCLOCK, AWAY, COLLIDING
}