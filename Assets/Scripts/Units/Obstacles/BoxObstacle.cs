using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public class BoxObstacle : MonoBehaviourPun
{
    float width;
    float height;
    float warnDelay;
    bool isDead = false;
    BoxCollider2D myCollider;
    PhotonView pv;
    [SerializeField] SpriteRenderer fillSprite;
    IEnumerator deleteRoutine;

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
        pv = GetComponent<PhotonView>();
        EventManager.StartListening(MyEvents.EVENT_GAME_FINISHED, OnGameEnd);
    }

    private void OnGameEnd(EventObject arg0)
    {
        DoDeath();
    }
    private void OnEnable()
    {
        isDead = false;
      
    }

/*    private void Update()
     {
          CheckContacts();
        DebugDrawBox(transform.position,transform.localScale,transform.eulerAngles.z,Color.red,1f);
     }*/
    void DebugDrawBox(Vector2 point, Vector2 size, float angle, Color color, float duration)
    {

        var orientation = Quaternion.Euler(0, 0, angle);

        // Basis vectors, half the size in each direction from the center.
        Vector2 right = orientation * Vector2.right * size.x / 2f;
        Vector2 up = orientation * Vector2.up * size.y / 2f;

        // Four box corners.
        var topLeft = point + up - right;
        var topRight = point + up + right;
        var bottomRight = point - up + right;
        var bottomLeft = point - up - right;

        // Now we've reduced the problem to drawing lines.
        Debug.DrawLine(topLeft, topRight, color, duration);
        Debug.DrawLine(topRight, bottomRight, color, duration);
        Debug.DrawLine(bottomRight, bottomLeft, color, duration);
        Debug.DrawLine(bottomLeft, topLeft, color, duration);
    }
    [PunRPC]
    public void SetInformation(float _width, float _delay) {

        this.warnDelay = _delay;
        height = (UnityEngine.Random.Range(0f, 1f) < 0.5f) ? _width : _width / 2f;
        width = (UnityEngine.Random.Range(0f, 1f) < 0.5f) ? _width : _width / 2f;

        transform.localScale = new Vector3(width, height, 1);
        StartFadeIn();

    }
    private void OnDisable()
    {
        fillSprite.DORewind();
    }
    private void StartFadeIn()
    {
        Color color = Color.blue;
        color.a = 0;
        fillSprite.color = color;
        fillSprite.DOFade(1f, warnDelay).OnComplete(()=> {
            //CheckContacts
            fillSprite.color = Color.green;
            if (deleteRoutine != null)
                StopCoroutine(deleteRoutine);
            deleteRoutine = WaitAndDestroy();
            StartCoroutine(deleteRoutine);
        });
        StartCoroutine(WaitAndCollide());
    }

    private void CheckContacts()
    {
        //if (!PhotonNetwork.LocalPlayer.IsMasterClient) return;
          //  Debug.Log("Location " + transform.position + " scale " + transform.localScale + " z " + transform.eulerAngles.z);
        Collider2D[] collisions = Physics2D.OverlapBoxAll(transform.position,  transform.localScale,transform.eulerAngles.z,LayerMask.GetMask("Player","Projectile"),minDepth:-3f,maxDepth:3f);
      //  Collider[] collisions = Physics.OverlapBox(transform.position,  transform.localScale,transform.eulerAngles.z);
        
        for (int i = 0; i < collisions.Length; i++) {
            Collider2D c = collisions[i];
           // Debug.Log(i + ". " + c.gameObject.name + " collision " + c.gameObject.tag + " / " + c.gameObject.layer + " / " + c.gameObject.name);
            HealthPoint healthPoint = c.gameObject.GetComponent<HealthPoint>();
           if (healthPoint == null) return;
       //     Debug.Log(i + ". collision hp " + healthPoint + " vs " + ConstantStrings.TAG_PLAYER);
               switch (c.gameObject.tag) {
                  case ConstantStrings.TAG_PLAYER:
                      healthPoint.DoDamage(null, true);
                      break;
                  case ConstantStrings.TAG_PROJECTILE:
                      healthPoint.DoDamage(null, true);
                      break;
              }

        }
    }
    IEnumerator WaitAndCollide()
    {
        yield return new WaitForSeconds(warnDelay);
        CheckContacts();
        yield return new WaitForFixedUpdate();
        myCollider.enabled = true;
    }

    IEnumerator WaitAndDestroy()
    {
        float randTime = Random.Range(warnDelay, warnDelay * 3f);
        yield return new WaitForSeconds(randTime);
        DoDeath();
    }
    void DoDeath()
    {
        if (isDead) return;
        isDead = true;
        if (pv.IsMine) {
            EventManager.TriggerEvent(MyEvents.EVENT_SPAWNER_EXPIRE, null);
            try
            {
                PhotonNetwork.Destroy(pv);
            }
            catch (Exception e)
            {
                Debug.Log(e.StackTrace);

            }
        }

    }
}
