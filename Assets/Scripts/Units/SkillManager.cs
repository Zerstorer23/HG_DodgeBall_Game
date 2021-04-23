using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;
using static ConstantStrings;

public class SkillManager : MonoBehaviourPun
{
    PhotonView pv;
    Unit_Movement unitMovement;

    //Data
    CharacterType myCharacter;
    float cooltime;
    float duration;
    float projSpeed;
    string[] prafabs;
    string myPrefab;

    delegate void voidFunc();
    voidFunc ActivateSkill;
    float lastActivatedTime;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        unitMovement = GetComponent<Unit_Movement>();
    }

    public void SetSkill(CharacterType type) {
        myCharacter = type;
        ParseSkill();
        lastActivatedTime = Time.time;
    }
    void ParseSkill() {
        switch (myCharacter)
        {
            case CharacterType.NAGATO:
                myPrefab = PREFAB_BULLET_NAGATO;
                cooltime = 1f;
                duration = 1.5f;
                projSpeed = 4f;
                ActivateSkill = Skill_Nagato;
                break;
            case CharacterType.HARUHI:
                myPrefab = PREFAB_BULLET_HARUHI;
                ActivateSkill = Skill_Haruhi;
                duration = 0.5f;
                projSpeed = 3f;
                cooltime = 1f;
                break;
            case CharacterType.MIKURU:
                myPrefab = PREFAB_BULLET_1;
                cooltime = 1f;
                ActivateSkill = Skill_Mikuru;
                duration = 0.5f;
                break;
        }
    }

    private void Update()
    {
        if (!pv.IsMine) return;
        CheckSKillActivation();
    }

    private void CheckSKillActivation()
    {
        if (Input.GetKeyDown(KeyCode.Space) && ActivateSkill != null)
        {
            if ((Time.time) >= (lastActivatedTime + cooltime))
                ActivateSkill();
        }
    }


    #region skills
    private void Skill_Nagato() {
        Vector3 dir = unitMovement.lastVector;
        float angle = GetAngleBetween(Vector3.zero, dir);
        Debug.Log("Vector " + dir + " angle " + angle);

        GameObject obj = PhotonNetwork.InstantiateRoomObject(myPrefab, transform.position, Quaternion.Euler(0, 0, angle));
        obj.GetComponent<PhotonView>().RPC("SetMoveInformation", RpcTarget.AllBuffered, 1f, projSpeed, 0f, 0f, angle);

        obj.GetComponent<PhotonView>().RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Straight, (int)ReactionType.None);
        obj.GetComponent<PhotonView>().RPC("SetDelay", RpcTarget.AllBuffered, duration);
        obj.GetComponent<PhotonView>().RPC("SetExclusionPlayer", RpcTarget.AllBuffered, pv.Owner.UserId);
        obj.transform.SetParent(BulletManager.GetInstance().Home_Bullets);

    }
    private void Skill_Haruhi()
    {
        GameObject obj = PhotonNetwork.InstantiateRoomObject(myPrefab, transform.position, Quaternion.identity);
        obj.GetComponent<PhotonView>().RPC("SetMoveInformation", RpcTarget.AllBuffered, 1f, 0f, 0f, 0f, 0f);
        obj.GetComponent<PhotonView>().RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Static, (int)ReactionType.None);
        obj.GetComponent<PhotonView>().RPC("SetGradualScale", RpcTarget.AllBuffered, duration, projSpeed);
        obj.GetComponent<PhotonView>().RPC("SetExclusionPlayer", RpcTarget.AllBuffered, pv.Owner.UserId);
        obj.transform.SetParent(BulletManager.GetInstance().Home_Bullets);

    }
    private void Skill_Mikuru()
    {
        unitMovement.moveSpeed *= 1.5f;
        StartCoroutine(WaitAndExecute(duration));
        GameObject obj = PhotonNetwork.InstantiateRoomObject(myPrefab, transform.position, Quaternion.identity);
        obj.GetComponent<PhotonView>().RPC("SetMoveInformation", RpcTarget.AllBuffered, 1f, 0f, 0f, 0f, 0f);
        obj.GetComponent<PhotonView>().RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Static, (int)ReactionType.None);
        obj.GetComponent<PhotonView>().RPC("SetDuration", RpcTarget.AllBuffered, duration);
        obj.GetComponent<PhotonView>().RPC("SetExclusionPlayer", RpcTarget.AllBuffered, pv.Owner.UserId);
        Debug.Log(pv.Owner.UserId);
        obj.GetComponent<PhotonView>().RPC("SetParentPlayer", RpcTarget.AllBuffered, pv.Owner.UserId);
    }

    IEnumerator WaitAndExecute(float delay) {
        yield return new WaitForSeconds(delay);
        unitMovement.moveSpeed /= 1.5f;
    }

    #endregion
}
public enum CharacterType { 
   NONE, NAGATO,HARUHI,MIKURU
}