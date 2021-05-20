using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit_SharedMovement : MonoBehaviourPun
{
    public float moveSpeed = 10f;
    PhotonView pv;
    int fieldNo = -1;
    MapSpec mapSpec;
    float fireSpeed = 0.75f;
    [SerializeField]  Transform gunPosition;
    [SerializeField] Image dirFill;
    [SerializeField] Text dirText;
    Dictionary<string, int> controllers;
    TransformSynchronisation transSync;
    float yOffset = -1.5f;
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        transSync = GetComponent<TransformSynchronisation>();
    }
    private void OnEnable()
    {
        fieldNo = (int)pv.InstantiationData[0];
        GameField myField = GameFieldManager.gameFields[fieldNo];
        myField.desolator = this;
        mapSpec = myField.mapSpec;
        controllers = myField.desolator_controllers;
        transform.position = new Vector3(0, transform.position.y);
        EventManager.StartListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);

        Vector3 newPosition = new Vector3(0, mapSpec.yMin + yOffset, 1);
        transform.position = newPosition;
        StartCoroutine(WaitAndFire());
    }
    private void OnDisable()
    {

        EventManager.StopListening(MyEvents.EVENT_FIELD_FINISHED, OnFieldFinish);
    }
    private void OnFieldFinish(EventObject arg0)
    {
        if (fieldNo != arg0.intObj) return;
        if (pv.IsMine) {
            PhotonNetwork.Destroy(pv);
        }
    }


    int prevDir = 0;
    // Update is called once per frame
    private void Update()
    {
        if (controllers.ContainsKey(PhotonNetwork.LocalPlayer.UserId)) {
        var deltaX = Input.GetAxis("Horizontal");
        int dir = deltaX == 0f ? 0 : (deltaX < 0f)? -1 : 1;
            if (prevDir != dir) {
                prevDir = dir;
                pv.RPC("GiveDirection", RpcTarget.All, PhotonNetwork.LocalPlayer.UserId, dir);
            }  
        }

        Move(Time.deltaTime);
        
    }

    IEnumerator WaitAndFire() {
        while (gameObject.activeInHierarchy)
        {
            if (pv.IsMine)
            {
                if (controllers.Count > 0)
                {
                    Fire();
                }
            }
            yield return new WaitForSeconds(fireSpeed);
        }
    }
    void Fire() {
        GameObject obj = PhotonNetwork.InstantiateRoomObject(
            ConstantStrings.PREFAB_BULLET_DESOLATION, gunPosition.position,
            Quaternion.Euler(0,0,90)
            , 0, 
            new object[] { fieldNo, "-1", false }
            );
        PhotonView pv = obj.GetComponent<PhotonView>();
        pv.RPC("SetBehaviour", RpcTarget.AllBuffered, (int)MoveType.Straight, (int)ReactionType.None, 90f);
    }

    [PunRPC]
    public void GiveDirection(string id, int dir) {
        controllers[id] = dir;
    }


    private void Move(float delta)
    {
        float sum = 0f;
        foreach (var entry in controllers.Values) {
            sum += ((float)entry )/ controllers.Count;
        }
        dirFill.fillAmount = (sum + 1f) *0.5f;
        dirText.text = sum.ToString("0.0");

        if (pv.IsMine)
        {
            float deltaX = sum * moveSpeed * delta;
            float newX = Mathf.Clamp(transSync.networkPos.x + deltaX, mapSpec.xMin, mapSpec.xMax);
            Vector3 newPosition = new Vector3(newX, mapSpec.yMin + yOffset,1);
            transSync.EnqueueLocalPosition(newPosition, Quaternion.identity);
        }
    }
}

