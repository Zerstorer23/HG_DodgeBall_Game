using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_TouchPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool isTouching = false;
    public static Vector2 touchVector;
/*   private void OnMouseDown()
    {
        // var touches = Input.touches;
        Debug.Log("touch count " + Input.touchCount);
        for (int i = 0; i < Input.touchCount; ++i) {
            var touch = Input.GetTouch(i);
            //if (touch.phase != TouchPhase.Began) continue;
            Debug.Log(i+": Mousedown " + touch.position+" touch "+ EventSystem.current.IsPointerOverGameObject(touch.fingerId));
            if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                isTouching = true;
              //  return;
            }
        }
    }*/
    public void HandleTouch() {
        if (Input.touchCount <= 0) return;

        Debug.Log("touch count " + Input.touchCount);
        for (int i = 0; i < Input.touchCount; ++i)
        {
            var touch = Input.GetTouch(i);

            if (touch.phase == TouchPhase.Began)
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    Debug.Log(i + ": Mousedown " + touch.position + " touch " + EventSystem.current.IsPointerOverGameObject(touch.fingerId));
                    isTouching = true;
                    touchVector = touch.position;
                    return;
                }
                
            }
            else if (touch.phase == TouchPhase.Canceled
               || touch.phase == TouchPhase.Ended)
            {
                isTouching = false;
                return;
            }
            else 
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    touchVector = touch.position;
                    return;
                }
            }
        }

    }
    private void Update()
    {
        HandleTouch();
    }

    /*    private void OnMouseUp()
        {
            isTouching = false;
            Debug.Log("Mousedown "+isTouching);

        }*/
/*    private void Update()
    {
        if (Input.touchCount > 0)
        {
            var touches = Input.touches;
            for (int i = 0; i < touches.Length; i++)
            {

                if (!EventSystem.current.IsPointerOverGameObject(i))
                {
                    isTouching = true;
                    EventManager.TriggerEvent(MyEvents.EVENT_SCREEN_TOUCH, new EventObject() { objData = touches[i].position });
                    Debug.Log("Mousedown " + isTouching);
                }
            }

        }
    }*/

    private void OnEnable()
    {
        gameObject.SetActive(Application.platform == RuntimePlatform.Android);
    }
}
