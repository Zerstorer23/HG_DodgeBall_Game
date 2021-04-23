
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviourPun
{
    PhotonView pv;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void OnEnable()
    {
        StartCoroutine(WaitAndKill(1.5f));
    }

    IEnumerator WaitAndKill(float delay)
    {
        yield return new WaitForSeconds(delay);
        KillMe();
    }

    public void KillMe()
    {
        if(pv.IsMine)
        PhotonNetwork.Destroy(gameObject);
    }

}
