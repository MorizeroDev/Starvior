using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollController : MonoBehaviour
{
    public Transform ScrollContainer;
    public Animator UpAni, DownAni;
    private Vector2 scrollPosition = Vector2.zero;
    private float scrollVelocity = 0f;
    private float timeTouchPhaseEnded = 0f;
    private float inertiaDuration = 0.5f;
    private bool mouseWheeling = false;
    private List<float> targetMouseY = new List<float>();

    private Vector2 lastDeltaPos;

    public void UpdateContainer()
    {
        targetMouseY.Clear();
        for (int i = 0; i < ScrollContainer.childCount; i++)
        {
            targetMouseY.Add(0);
        }
    }
    private void Start()
    {
        UpdateContainer();
    }
    void Update()
    {
        float origin = scrollPosition.y;
        if (mouseWheeling)
        {
            for (int i = 0; i < ScrollContainer.childCount; i++)
            {
                Transform t = ScrollContainer.GetChild(i).transform;
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y + (targetMouseY[i] - t.localPosition.y) / 5, t.localPosition.z);
            }
            if (Mathf.Abs(ScrollContainer.GetChild(0).transform.localPosition.y - targetMouseY[0]) <= 0.1f) mouseWheeling = false;
        }
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                scrollPosition.y += Input.GetTouch(0).deltaPosition.y;
                lastDeltaPos = Input.GetTouch(0).deltaPosition;
            }
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                //print("End:" + lastDeltaPos.y + "|" + Input.GetTouch(0).deltaTime);
                if (Mathf.Abs(lastDeltaPos.y) > 20.0f)
                {
                    scrollVelocity = (int)(lastDeltaPos.y * 0.5 / Input.GetTouch(0).deltaTime);
                    //print(scrollVelocity);
                }
                timeTouchPhaseEnded = Time.time;
            }
        }
        else
        {
            if (scrollVelocity != 0.0f)
            {
                // slow down
                float t = (Time.time - timeTouchPhaseEnded) / inertiaDuration;
                float frameVelocity = Mathf.Lerp(scrollVelocity, 0, t);
                scrollPosition.y += frameVelocity * Time.deltaTime;

                if (t >= inertiaDuration)
                    scrollVelocity = 0;
            }
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            scrollPosition.y += Input.GetAxis("Mouse ScrollWheel") * 1000;
            float del = scrollPosition.y - origin;
            if (del != 0)
            {
                for (int i = 0; i < ScrollContainer.childCount; i++)
                {
                    Transform t = ScrollContainer.GetChild(i).transform;
                    if (mouseWheeling)
                    {
                        targetMouseY[i] -= del;
                    }
                    else
                    {
                        targetMouseY[i] = t.localPosition.y - del;
                    }
                    //t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y - del, t.localPosition.z);
                }
                mouseWheeling = true;
            }
        }
        else
        {
            float del = scrollPosition.y - origin;
            if (del != 0)
            {
                for (int i = 0; i < ScrollContainer.childCount; i++)
                {
                    Transform t = ScrollContainer.GetChild(i).transform;
                    t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y - del, t.localPosition.z);
                }
            }
        }



    }
}
