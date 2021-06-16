
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviourPun
{
    PhotonView pv;
    public float delay = 1f;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    private void OnEnable()
    {
        StartCoroutine(WaitAndKill());
    }

    IEnumerator WaitAndKill()
    {
        yield return new WaitForSeconds(delay);
        KillMe();
    }

    public void KillMe()
    {
        if (pv.IsMine)
        {
            PhotonNetwork.Destroy(pv);
        }
    }

}
