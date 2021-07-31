using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapObject_Wall : MonoBehaviourPun
{
    // Start is called before the first frame update
    private void OnEnable()
    {
        if (photonView.InstantiationData != null)
        {
            int fieldID = (int)photonView.InstantiationData[0];
            float length = (float)photonView.InstantiationData[1];
            transform.localScale = new Vector3(length, 0.4f, 1);
            transform.SetParent(GameFieldManager.gameFields[fieldID].gameObject.transform, true);
        }
    }
}
