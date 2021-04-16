using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviourPun
{
    BoxCollider2D myCollider;
    private void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
    }

    [PunRPC]
    public void SetParentPlayer(string owner) {
        Transform parent = GameSession.GetInst().charSpawner.GetTransformOfPlayer(owner);
        if (parent != null) {
            transform.SetParent(parent);        
        }
    }

}
