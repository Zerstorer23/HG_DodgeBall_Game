using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BulletManager;

public class Action_InstantiateBullet : SkillAction
{
    
    public override float Activate() {

        int fieldNumber = parent.castingPlayer.fieldNo;
        GameObject obj = PhotonNetwork.Instantiate(
            (string)GetParam(SkillParams.PrefabName),
            parent.caster.transform.position,
            parent.caster.transform.rotation,0
            , new object[] {fieldNumber, parent.casterPV.Owner.UserId, false}
            );
        parent.spawnedObject = obj;
        // Debug.Log("instantiate " + obj);
        parent.pv = parent.spawnedObject.GetComponent<PhotonView>();

     //   parent.pv.RPC("SetParentTransform", RpcTarget.AllBuffered);
       // parent.pv.RPC("SetOwnerPlayer", RpcTarget.AllBuffered, fieldNumber, parent.casterPV.Owner.UserId);
        

        return 0f;
    }

}
public class Action_InstantiateBullet_FollowPlayer : SkillAction
{

    public override float Activate()
    {
        int fieldNumber = parent.castingPlayer.fieldNo;
        GameObject obj = PhotonNetwork.Instantiate(
            (string)GetParam(SkillParams.PrefabName),
            parent.caster.transform.position,
            parent.caster.transform.rotation, 0
            , new object[] { fieldNumber, parent.casterPV.Owner.UserId, true
            }
            );
        parent.spawnedObject = obj;
        parent.pv = parent.spawnedObject.GetComponent<PhotonView>();


        return 0f;
    }
}
public class Action_InstantiateBulletAt : SkillAction
{

    public override float Activate()
    {

        int fieldNumber = parent.castingPlayer.fieldNo;
        Vector3 pos = (Vector3)GetParam(SkillParams.Vector3);
        Quaternion angle = (Quaternion)GetParam(SkillParams.Quarternion);

        GameObject obj = PhotonNetwork.Instantiate(
          (string)GetParam(SkillParams.PrefabName), pos, angle, 0
            , new object[] { fieldNumber, parent.casterPV.Owner.UserId, false });
        parent.spawnedObject = obj;
        parent.pv = parent.spawnedObject.GetComponent<PhotonView>();
        return 0;
    }
}

public class Action_Projectile_ResetAngle : SkillAction
{

    public override float Activate()
    {
        parent.pv.RPC("ResetRotation", RpcTarget.AllBuffered);
        return 0;
    }

}
public class Action_GetCurrentPlayerPosition : SkillAction
{

    public override float Activate()
    {
        float angle = parent.caster.GetComponent<Unit_Movement>().GetAim();
        //    Debug.Log("calc angle " + angle + " from " +dir);
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, angle));
        parent.SetParam(SkillParams.EulerAngle, angle);
        parent.SetParam(SkillParams.Vector3, parent.caster.transform.position);
        return 0;
    }
}
public class Action_GetCurrentPlayerPosition_AngledOffset : SkillAction
{

    public override float Activate()
    {
        float angle = parent.caster.GetComponent<Unit_Movement>().GetAim();
        float distance = (float)parent.GetParam(SkillParams.Distance, 1.4f);
        float rad = angle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * distance;
        float dY = Mathf.Sin(rad) * distance;
        parent.SetParam(SkillParams.Quarternion, Quaternion.Euler(0, 0, angle));
        parent.SetParam(SkillParams.EulerAngle, angle);
        parent.SetParam(SkillParams.Vector3, parent.caster.transform.position + new Vector3(dX, dY));
        return 0;
    }

}
public class Action_GetCurrentPlayerVector3_AngledOffset : SkillAction
{

    public override float Activate()
    {
        float angle =(float) parent.GetParam(SkillParams.EulerAngle);
        float distance = (float)parent.GetParam(SkillParams.Distance, 1.4f);
        float rad = angle / 180 * Mathf.PI;
        float dX = Mathf.Cos(rad) * distance;
        float dY = Mathf.Sin(rad) * distance;
        parent.SetParam(SkillParams.Vector3, parent.caster.transform.position + new Vector3(dX,dY));
        return 0;
    }
}
