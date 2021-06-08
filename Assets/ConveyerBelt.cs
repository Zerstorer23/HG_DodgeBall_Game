using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerBelt : MonoBehaviour
{
    public Directions cardinal = Directions.E;
    Vector3 direction = Vector3.zero;
     float speed = 20f;

    private void Awake()
    {
        direction = ConstantStrings.GetAngledVector(transform.rotation.eulerAngles.z, 1f);

        /*switch (cardinal)
        {
            case Directions.W:
                direction = Vector3.left;
                break;
            case Directions.E:
                direction = Vector3.right;
                break;
            case Directions.N:
                direction = Vector3.up;
                break;
            case Directions.S:
                direction = Vector3.down;
                break;
                *//*
            case Directions.NW:
                direction = new Vector3(-0.7f,0.7f,0);
                break;
            case Directions.NE:
                direction = new Vector3(0.7f, 0.7f, 0);
                break;
            case Directions.SW:
                direction = new Vector3(-0.7f, -0.7f, 0);
                break;
            case Directions.SE:
                direction = new Vector3(0.7f, -0.7f, 0);
                break;*//*
        }*/
    }
    public Vector3 GetDirection() {
        return direction * speed;
    }
}
