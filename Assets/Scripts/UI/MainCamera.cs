using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MainCamera : MonoBehaviour
{
   public CinemachineStateDrivenCamera stateCam;

    [SerializeField] Transform fieldTransform;

    private static MainCamera prMainCam;
    // Start is called before the first frame update
    public static MainCamera instance
    {
        get
        {
            if (!prMainCam)
            {
                prMainCam = FindObjectOfType<MainCamera>();
                if (!prMainCam)
                {
                    //  prEvManager = Instantiate(EventManager) as EventManager;
                    Debug.LogWarning("There needs to be one active MainCamera script on a GameObject in your scene.");
                }
            }

            return prMainCam;
        }
    }
    public static void SetFollow(Transform trans) {
        instance.stateCam.Follow = trans;
    }

    public static void FocusOnField(bool enable) {
        if(instance != null)
        instance.GetComponent<Animator>().SetBool("ViewField", enable);
 /*       if (enable) {
            instance.stateCam.Follow = instance.fieldTransform;
        }*/
    }

}
