using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;

public class Unit_Movement : MonoBehaviourPun
{
    public const float initialSpeed = 8f;
    public float moveSpeed;
    public bool[] isTouching = new bool[4];
    PhotonView pv;
    public Vector3 lastVector = Vector3.up;
    public static float xMin, xMax, yMin, yMax;

    BuffManager buffManager;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        buffManager = GetComponent<BuffManager>();
    }
    private void Start()
    {
        xMin = GameSession.xMin;
        xMax = GameSession.xMax;
        yMin = GameSession.yMin;
        yMax = GameSession.yMax;
    }
    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine) return;
        Move();
        CheckCollisions();
    }
    private void OnEnable()
    {
        moveSpeed = initialSpeed;
    }
    private void CheckCollisions()
    {
        isTouching[(int)Directions.W] = (transform.position.x <= xMin);
        isTouching[(int)Directions.E] = (transform.position.x >= xMax);
        isTouching[(int)Directions.N] = (transform.position.y >= yMax);
        isTouching[(int)Directions.S] = (transform.position.y <= yMin);
    }

    private void Move()
    {
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed * buffManager.GetSpeedModifier();
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed * buffManager.GetSpeedModifier();


        if (isTouching[(int)Directions.W]) {
            deltaX = Mathf.Max(deltaX, 0f);
        }
        if (isTouching[(int)Directions.E])
        {
            deltaX = Mathf.Min(deltaX, 0f);
        }
        if (isTouching[(int)Directions.N])
        {
            deltaY = Mathf.Min(deltaY, 0f);
        }
        if (isTouching[(int)Directions.S])
        {
            deltaY = Mathf.Max(deltaY, 0f);
        }
        Vector3 moveDir = new Vector3(deltaX, deltaY, 0f);

        if (moveDir != Vector3.zero) {
            lastVector = moveDir;
        }
        transform.position+= moveDir;

    }

}

