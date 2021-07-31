using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerBelt : MonoBehaviourPun
{
    Vector3 direction = Vector3.zero;
     float speed = 20f;
    private void OnEnable()
    {
        direction = ConstantStrings.GetAngledVector(transform.rotation.eulerAngles.z, 1f);
        if (photonView.InstantiationData != null)
        {
            int fieldID = (int)photonView.InstantiationData[0];
            float length = (float)photonView.InstantiationData[1];
            transform.localScale = new Vector3(length, 2.5f, 1);
            transform.SetParent(GameFieldManager.gameFields[fieldID].gameObject.transform, true);
        }
    }
    public Vector3 GetDirection() {
        return direction * speed;
    }
}
