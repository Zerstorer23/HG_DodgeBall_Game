using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;

public class Unit_Movement : MonoBehaviourPun
{
    public const float initialSpeed = 10f;
    public float moveSpeed = initialSpeed;
    [SerializeField] EdgeCollider2D[] pathColliders;
    public bool[] isTouching = new bool[4];
    PhotonView pv;
    internal Vector3 lastVector = Vector3.up;


    private void Awake()
    {
        pv = GetComponent<PhotonView>();
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
        for (int i = 0; i < pathColliders.Length; i++) {
            bool isTouch =   Physics2D.IsTouchingLayers(pathColliders[i], LayerMask.GetMask("Boundary"));
            isTouching[i] = isTouch;
        }
    }

    private void Move()
    {
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;



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
        lastVector = new Vector3(deltaX, deltaY);


        //  newXpos = Mathf.Clamp(newXpos, xMin, xMax);
        //    newYpos = Mathf.Clamp(newYpos, yMin, yMax);
        transform.Translate(lastVector);

    }

}

