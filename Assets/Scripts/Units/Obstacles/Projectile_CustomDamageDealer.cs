using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_CustomDamageDealer : MonoBehaviour
{

    List<string> playerList;
    //   HashSet<string> foundTargets = new HashSet<string>();
    SortedDictionary<string, Unit_Player> playerDict = new SortedDictionary<string, Unit_Player>();
    Projectile_DamageDealer damageDealer;
    Projectile proj;
    PhotonView pv;
    public float colliderRadius;
    private void Awake()
    {
        damageDealer = GetComponent<Projectile_DamageDealer>();
        proj = GetComponent<Projectile>();
        pv = GetComponent<PhotonView>();
    }
    private void OnEnable()
    {
        int fieldNo = (int)pv.InstantiationData[0];

        //  foundTargets = new HashSet<string>();
        playerDict = GameFieldManager.GetPlayersInArea(fieldNo);
        playerList = new List<string>(playerDict.Keys);
        Debug.Assert(playerDict.Count == playerList.Count, " player dict mismatch");
    }
    private void Update()
    {
        FindNearByPlayers();
    }
    private void FindNearByPlayers()
    {
        //     Debug.Log("Num players : " + unit_Players.Count + " / Captured: " + foundTargets.Count);
        for (int i = 0; i < playerList.Count; i++) {
            string key = playerList[i];
            Unit_Player player = playerDict[key];
            if (key == proj.pv.Owner.UserId || player == null) continue;
            //if (!foundTargets.Contains(key))
          //  {
                float dist = Vector2.Distance(gameObject.transform.position, player.transform.position);
                if (dist <= colliderRadius)
                {
                    var go = player.gameObject;
                  //  foundTargets.Add(key);
                    damageDealer.DoPlayerCollision(go);
                }
           // }
        }
    }

    /*
       private void FindNearByPlayers()
    {
        //     Debug.Log("Num players : " + unit_Players.Count + " / Captured: " + foundTargets.Count);
        for (int i = 0; i < playerList.Count; i++) {
            string key = playerList[i];
            Unit_Player player = playerDict[key];
            if (key == proj.pv.Owner.UserId || player == null) continue;
            float dist = Vector2.Distance(gameObject.transform.position, player.transform.position);
            // Debug.Log(entry.Value.pv.Owner.NickName+": "+ entry.Key+ " dist: " + dist +" vs "+colliderRadius);
            if (foundTargets.ContainsKey(key))
            {
                if (dist > colliderRadius)
                {
                    Debug.Log(key + " leaves region");
                    foundTargets.Remove(key);
                }
            }
            else
            {
                if (dist <= colliderRadius)
                {
                    Debug.Log(key + " enters region");
                    var go = player.gameObject;
                    foundTargets.Add(key, go);
                    damageDealer.DoPlayerCollision(go);
                }
            }

        }
    }
     */
}
