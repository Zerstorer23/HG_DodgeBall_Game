using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformSynchronisation_Timed : MonoBehaviourPunCallbacks
    , IPunObservable
{

    public Vector3 networkPos;
    public Quaternion networkQuaternion;
    public float syncPeriod = 1f;

    bool doSync = false;
    private void Start()
    {
        networkPos = transform.localPosition;
        networkQuaternion = transform.rotation;
        StartCoroutine(SyncPosition());
    }
    IEnumerator SyncPosition() {
        while (gameObject.activeInHierarchy) {
            networkPos = transform.localPosition;
            networkQuaternion = transform.rotation;
            doSync = true;
            yield return new WaitForSeconds(1f);
        }
    }
    public void ForceSync() {
        networkPos = transform.localPosition;
        networkQuaternion = transform.rotation;
        doSync = true;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //통신을 보내는 
        if (stream.IsWriting)
        {
            if (doSync) {
                stream.SendNext(networkPos);
                stream.SendNext(networkQuaternion);
                doSync = false;
            }
        }

        //클론이 통신을 받는 
        else
        {
            transform.localPosition = (Vector3)stream.ReceiveNext();
            transform.rotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
