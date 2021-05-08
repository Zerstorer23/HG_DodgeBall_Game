using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ConstantStrings;
using static GameFieldManager;

public class Unit_AutoDrive : MonoBehaviour
{
    [SerializeField] List<GameObject> foundObjects;
    [SerializeField] List<GameObject> boxObjects;
    CircleCollider2D finder;
    SkillManager skillManager;
    Unit_Movement movement;
    internal GameObject directionIndicator;
    public float range = 5f;
    float escapePadding = 1f;

    public void SetInfo(Unit_Player p) {
        movement = p.movement;
        skillManager = p.skillManager;
        directionIndicator = p.driverIndicator;
    
    }
    private void Awake()
    {
        finder = GetComponent<CircleCollider2D>();
        SetRange(9f);
    }
    public void SetRange(float a) {
        range = a;
        finder.radius = a;
    }
    private void OnEnable()
    {
        foundObjects = new List<GameObject>();
        boxObjects = new List<GameObject>();
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
        boxObjects.Add(arg0.goData);
    }

    private void OnBoxEnabled(EventObject arg0)
    {
        for (int i = 0; i < boxObjects.Count; i++) {
            if (boxObjects[i].transform.position == arg0.goData.transform.position) {
                boxObjects.RemoveAt(i);
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        foreach (GameObject go in foundObjects)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            Gizmos.DrawWireSphere(go.transform.position, 0.5f);
        }

        foreach (GameObject go in boxObjects)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            float distance = Vector3.Distance(go.transform.position, transform.position);
            if (distance <= range)
            {
                Gizmos.DrawWireSphere(go.transform.position, 1f);
            }
        }
    }
    // Update is called once per frame


    private void OnTriggerEnter2D(Collider2D collision)
    {
      //  Debug.Log("Found trigger " + collision.gameObject.name);
        foundObjects.Add(collision.gameObject);
    }

    Vector3 lastVector = Vector3.zero;
    private void FixedUpdate()
    {
        if (movement == null || !movement.gameObject.activeInHierarchy) {

            gameObject.SetActive(false);
            return;
        }
        Vector3 dir = EvaluateMoves();
        lastVector = dir;
        float aimAngle = GameSession.GetAngle(Vector3.zero, dir); //벡터 곱 비교
        
       // Debug.Log("Eval angle " + aimAngle);
        float rad = aimAngle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * 1.4f;
        float dY = Mathf.Sin(rad) * 1.4f;
        directionIndicator.transform.localPosition = new Vector3(dX, dY);
        directionIndicator.transform.localRotation = Quaternion.Euler(0, 0, aimAngle);

        RemoveObjects();
    }
    public int size;
    void RemoveObjects() {
        int i = 0;
        while (i < foundObjects.Count) {
            GameObject go = foundObjects[i];
            if (go == null || !go.activeInHierarchy)
            {
                Debug.Log("Remove for inactiveobj" );
                foundObjects.RemoveAt(i);
                continue;
            }
            else {
                float dist = Vector2.Distance(transform.position, go.transform.position);
                if (dist > range+ escapePadding) {
                    Debug.Log("Remove for out of dist " + dist + " / " + (range + escapePadding));
                    foundObjects.RemoveAt(i);
                    continue;
                }
            }
            i++;
        }
        size = foundObjects.Count;
    }
   public Vector3 EvaluateMoves() {
        Vector3 move = Vector3.zero;
        foreach (GameObject go in foundObjects) {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            Vector3 dir = go.transform.position - movement.networkPos;
            dir.Normalize();
            float distance = Vector3.Distance(go.transform.position, movement.networkPos);
            if (distance > range) continue;
            if (go.tag == TAG_PLAYER)
            {
                if (skillManager.GetRemainingTime() > 0)
                { //스킬사용가능시 접근
                    dir = -dir;
                }
            }
            else if (go.tag == TAG_PROJECTILE)
            {
                HealthPoint hp = go.GetComponent<HealthPoint>();
                if (!hp.damageDealer.isMapObject && !hp.pv.IsMine)
                {
                    continue;
                }
                dir = -dir;
            }
            else if (go.tag == TAG_BUFF_OBJECT)
            {
                if (!go.GetComponent<BoxCollider2D>().enabled)
                    continue;
             }
            else {
                continue;
            }
            move += dir * GetMultiplier(distance);
        }
        foreach (GameObject go in boxObjects)
        {
            if (go == null || !go.activeInHierarchy)
            {
                continue;
            }
            Vector3 dir = go.transform.position - movement.networkPos;
            float distance = Vector3.Distance(go.transform.position, movement.networkPos);
            if (distance > range) continue;
            move -= dir* GetMultiplier(distance);
        }



        Vector3 random = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        //  Debug.Log("random move " + random);
        //   move += random;
        move += GetAwayFromWalls();
        if (move == Vector3.zero)
        {
            move = lastVector;
        }

        return move.normalized;
    }

    Vector3 GetAwayFromWalls() {
        Vector3 center = new Vector3(xMid, yMid);
        float dist = Vector2.Distance(center, movement.networkPos);
        float notDistance = xMid - xMin;
        Vector3 dir = center - movement.networkPos;
        dir.Normalize();
        dir = dir * GetMultiplier(notDistance - dist);
        

        return dir;
    }

    float GetMultiplier(float x) {
        float y = (1 / Mathf.Pow(x+ 2,2)) * 48 - 0.4f;
     //   Debug.Log(x + "-> " + y);
        return y;
    }
}
