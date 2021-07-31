using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ConstantStrings;

public class Teleporter : MonoBehaviourPun
{
    [SerializeField] Teleporter otherSide;
    [SerializeField] Text coolText;
    [SerializeField] Image directionIndicator;
    [SerializeField] SpriteRenderer hosSprite;
    double teleportDelay = 2.5d;
    public double nextTeleportTime = 0d;
    ICachedComponent cachedComponent = new ICachedComponent();
    float angleToOtherside;
    Dictionary<int,double> teleported = new Dictionary<int, double>();
    private void OnEnable()
    {
        teleported.Clear();
        UpdatePortalDirection();
        if (photonView.InstantiationData != null) {
            int fieldID = (int)photonView.InstantiationData[0];
            transform.localScale = new Vector3(3, 3, 1);
            transform.SetParent(GameFieldManager.gameFields[fieldID].gameObject.transform, true);
        }
    }

/*    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (otherSide == null || PhotonNetwork.Time < (nextTeleportTime)) return;
        string tag = collision.gameObject.tag;
        int tid = collision.gameObject.GetInstanceID();
        // Debug.Log(gameObject.name + "Trigger with " + collision.gameObject.name+" / tag "+tag);
        switch (tag)
        {
            case TAG_PLAYER:
                Unit_Movement unit_Movement = cachedComponent.Get<Unit_Movement>(tid, collision.gameObject);
                DoTeleport(unit_Movement);
                break;
            case TAG_PROJECTILE:
                break;
        }


    }*/
/*    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }*/
    private void FixedUpdate()
    {
        UpdateCoolTime();
        if (otherSide == null || PhotonNetwork.Time < (nextTeleportTime)) return;
        Collider2D[] collisions = Physics2D.OverlapCircleAll(
        transform.position, 1f, LayerMask.GetMask(TAG_PLAYER, TAG_PROJECTILE));
        
        foreach (var c in collisions)
        {
            int tid = c.gameObject.GetInstanceID();
            bool cool =  CheckTeleportRecord(tid);
            if (!cool) continue;
            if (c.gameObject.CompareTag(TAG_PLAYER))
            {
                Unit_Movement unit_Movement = cachedComponent.Get<Unit_Movement>(tid, c.gameObject);
                DoTeleport(unit:unit_Movement);
                return;
            } else if (c.gameObject.CompareTag(TAG_PROJECTILE)) {

                HealthPoint health = cachedComponent.Get<HealthPoint>(tid, c.gameObject);
              //  if (health.IsMapProjectile()) return;
                Projectile_Movement pMove = (Projectile_Movement)health.movement;
                if (pMove.moveType == MoveType.Curves || pMove.moveType == MoveType.Straight) {
                    DoTeleport(pMove:pMove);
                    return;
                }
            }
        }
    }
    IEnumerator NumerateCooltime() {
        while (PhotonNetwork.Time < nextTeleportTime)
        {
            double remain = (nextTeleportTime - PhotonNetwork.Time);
            coolText.text = remain.ToString("0.0");
            yield return new WaitForFixedUpdate();
        }
   
        coolText.text = ""; 
        hosSprite.color = Color.cyan;
        directionIndicator.enabled = true;
        
    }
    [PunRPC]
    public void LinkPortal(int viewID) {
        PhotonView other = PhotonNetwork.GetPhotonView(viewID);
        if (other != null) {
            otherSide = other.gameObject.GetComponent<Teleporter>();
            UpdatePortalDirection();
        }
    
    }
    public void UpdatePortalDirection() {
        if (otherSide != null)
        {
            angleToOtherside = GetAngleBetween(transform.position, otherSide.transform.position);
            directionIndicator.transform.rotation = Quaternion.Euler(0, 0, angleToOtherside);
        }

    }
    private void UpdateCoolTime()
    {
        double remain = (nextTeleportTime - PhotonNetwork.Time);
        if (remain > 0)
        {
            coolText.text = remain.ToString("0.0");
            hosSprite.color = Color.cyan;
        }
        else {
            coolText.text = "";
            directionIndicator.enabled = true;
            hosSprite.color = GetColorByHex("#23FF00");
        }

    }

    void DoTeleport(Unit_Movement unit = null, Projectile_Movement pMove = null)
    {
        GameObject go = null;
        if (unit != null)
        {
            go = unit.gameObject;
            SetUsed(PhotonNetwork.Time + teleportDelay);
            otherSide.SetUsed(nextTeleportTime);
            unit.TeleportPosition(otherSide.transform.position);
        }
        else if (pMove != null)
        {
            go = pMove.gameObject;
            pMove.TeleportPosition(otherSide.transform.position);
        }
        if (go != null) {

            AddTeleportRecord(go);
            otherSide.AddTeleportRecord(go);
        }
    }
    public void SetUsed(double nextTime ) {
        nextTeleportTime = nextTime;
        hosSprite.color = Color.white; 
        directionIndicator.enabled = false;
        StartCoroutine(NumerateCooltime());
    }

    public void AddTeleportRecord(GameObject go) {
        int tid = go.GetInstanceID();
        double time = PhotonNetwork.Time + teleportDelay;
        if (teleported.ContainsKey(tid))
        {
            teleported[tid] = time;
        }
        else {
            teleported.Add(tid,time );
        }
    }
    public bool CheckTeleportRecord(int id) {
        if (teleported.ContainsKey(id))
        {
            return (PhotonNetwork.Time > teleported[id]);
        }
        else {
            return true;
        }
    }
    private void OnDisable()
    {
        cachedComponent.Clear();
    }
}
