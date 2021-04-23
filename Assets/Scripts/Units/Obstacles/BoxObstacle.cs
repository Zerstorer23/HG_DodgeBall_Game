using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoxObstacle : MonoBehaviourPun
{
    float width;
    float height;
    float warnDelay;
    BoxCollider2D myCollider;
    PhotonView pv;
    [SerializeField] SpriteRenderer fillSprite;
    IEnumerator deleteRoutine;

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
        pv = GetComponent<PhotonView>();
    }

    [PunRPC]
    public void SetInformation(float _width, float _delay) {

        this.warnDelay = _delay;
        height = (Random.Range(0f, 1f) < 0.5f) ? _width : _width / 2f;
        width = (Random.Range(0f, 1f) < 0.5f) ? _width : _width / 2f;

        transform.localScale = new Vector3(width, height, 1);
        StartFadeIn();

    }
    private void StartFadeIn()
    {
        Color color = Color.blue;
        color.a = 0;
        fillSprite.color = color;
        fillSprite.DOFade(1f, warnDelay).OnComplete(()=>{
            myCollider.enabled = true;
            fillSprite.color = Color.green;
            if (deleteRoutine != null)
                StopCoroutine(deleteRoutine);
            deleteRoutine = WaitAndDestroy();
            StartCoroutine(deleteRoutine);
            CheckContacts();
        });
    }

    private void CheckContacts()
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;
        Collider2D[] collisions =  Physics2D.OverlapBoxAll(transform.position, transform.localScale,transform.eulerAngles.z);
        for (int i = 0; i < collisions.Length; i++) {
            Collider2D c = collisions[i];
            HealthPoint healthPoint = c.gameObject.GetComponent<HealthPoint>();
            if (healthPoint == null) return;
        //    Debug.Log(i + ". collision " + c.gameObject.tag+" / "+c.gameObject.layer + " / " + c.gameObject.name);
            switch (c.gameObject.tag) {
                case ConstantStrings.TAG_PLAYER:
                    healthPoint.DoDamage(true);
                    break;
                case ConstantStrings.TAG_PROJECTILE:
                    healthPoint.DoDamage(true);
                    break;
            }
        
        }
    }

    IEnumerator WaitAndDestroy()
    {
        yield return new WaitForSeconds(warnDelay * 2f);
        if (PhotonNetwork.IsMasterClient) { 
            EventManager.TriggerEvent(MyEvents.EVENT_SPAWNER_EXPIRE, null);
            PhotonNetwork.Destroy(pv);
        }
    }

}
