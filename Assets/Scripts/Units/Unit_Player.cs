using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit_Player : MonoBehaviourPun
{
    Animator animator;
    internal PhotonView pv;
    public CharacterType myCharacter;
    [SerializeField] SpriteRenderer myPortrait;
    [SerializeField] Sprite[] portraits;
    HealthPoint health;

    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
        pv = GetComponent<PhotonView>();
        health = GetComponent<HealthPoint>();
    }
    void Start()
    {
        if (pv.IsMine) { 
            MainCamera.SetFollow(gameObject.transform);
            MainCamera.SetAnimTarget(animator);
        }
        EventManager.TriggerEvent(MyEvents.EVENT_PLAYER_SPAWNED, new EventObject() { stringObj = pv.Owner.UserId , gameObject = gameObject }) ; 
    }  // Update is called once per frame

    [PunRPC]
    public void SetInformation(int charID, int lives) {
        myCharacter = (CharacterType)charID;
        GetComponent<SkillManager>().SetSkill(myCharacter);
        myPortrait.sprite = portraits[charID];
        health.SetMaxLife(lives);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {


    }


}
