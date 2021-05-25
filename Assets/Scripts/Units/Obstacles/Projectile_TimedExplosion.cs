using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_TimedExplosion : MonoBehaviour
{
    Projectile_DamageDealer damageDealer;
    HealthPoint health;
    public float timeout;
    private void Awake()
    {
        damageDealer = GetComponent<Projectile_DamageDealer>();
        health = GetComponent<HealthPoint>();
    }
/*    private void FixedUpdate()
    {
        CheckContacts();
    }*/
    private void OnEnable()
    {
        damageDealer.myCollider.enabled = false;
        if (timeoutRoutine != null) {
            StopCoroutine(timeoutRoutine);
        }
        timeoutRoutine = WaitAndExplode();
        StartCoroutine(timeoutRoutine);
    }
    

    IEnumerator timeoutRoutine = null;
    IEnumerator WaitAndExplode() {
        yield return new WaitForSeconds(timeout);
        //damageDealer.myCollider.enabled = true;
        if (health.pv.IsMine) {
            CheckContacts();
        }
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        health.Kill_Immediate();
    }
    private void CheckContacts()
    {
        Collider2D[] collisions = Physics2D.OverlapCircleAll(transform.position, 2f);
        for (int i = 0; i < collisions.Length; i++)
        {
            Collider2D c = collisions[i];
            HealthPoint healthPoint = c.gameObject.GetComponent<HealthPoint>();
            if (healthPoint == null) continue;
            if (health.pv.IsMine) {
                //맵오브젝트가 아닌데 내꺼 = 무시
                if (health.damageDealer != null)
                {  
                    if (!health.damageDealer.isMapObject) continue;
                }
                else {
                    continue;
                }
            }
            switch (c.gameObject.tag)
            {
                case ConstantStrings.TAG_PLAYER:
                    Debug.Log(c.gameObject.name);
                    damageDealer.DoPlayerCollision(c.gameObject);
                    break;
                case ConstantStrings.TAG_PROJECTILE:
                    damageDealer.DoProjectileCollision(c.gameObject);
                    break;
            }

        }
    }
    private void OnDrawGizmos()
    {
        
    }
}
