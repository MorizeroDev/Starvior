using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollController : MonoBehaviour
{
    public Transform ScrollContainer;
    public Animator UpAni, DownAni;
    public float CanvasHeight = 1440f;
    public static bool UIUsing = false;
    private Vector2 scrollPosition = Vector2.zero;
    private float scrollVelocity = 0f;
    private float timeTouchPhaseEnded = 0f;
    private float inertiaDuration = 0.5f;
    private bool mouseWheeling = false;
    private float FY = 0,LY = 0;
    private List<float> targetMouseY = new List<float>();
    private Dictionary<Transform,List<float>> orY = new();
    private bool UpPlayed = false, DownPlayed = false;

    private Vector2 lastDeltaPos;

    public void ResetSavedPosition(Transform transform)
    {
        Debug.Log("Restore the position:" + transform.name);
        if (!orY.ContainsKey(transform))
        {
            Debug.LogWarning("指定要恢复的Transform不存在。");
            return;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 pos = transform.GetChild(i).transform.localPosition;
            pos.y = orY[transform][i];
            transform.GetChild(i).transform.localPosition = pos;
        }
    }
    public void SetOriginalPosition(Transform transform)
    {
        if (!orY.ContainsKey(transform))
        {
            Debug.Log("Positions saved:" + transform.name);
            orY.Add(transform, new List<float>());
            orY[transform].Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                orY[transform].Add(transform.GetChild(i).transform.localPosition.y);
            }
        }
    }
    public void UpdateContainer(bool SetOriginal = true)
    {
        targetMouseY.Clear();
        mouseWheeling = false;
        scrollVelocity = 0.0f;
        FY = ScrollContainer.GetChild(0).localPosition.y;
        LY = -(CanvasHeight / 2) + 20f;
        for (int i = 0; i < ScrollContainer.childCount; i++)
        {
            targetMouseY.Add(0);
        }
        if (SetOriginal) SetOriginalPosition(ScrollContainer);
    }
    public void ScrollToBottom()
    {
        RectTransform rect = ScrollContainer.GetChild(ScrollContainer.childCount - 1).GetComponent<RectTransform>();
        float del = ScrollContainer.GetChild(ScrollContainer.childCount - 1).localPosition.y - LY + 1440;
        for (int i = 0; i < ScrollContainer.childCount; i++)
        {
            Transform t = ScrollContainer.GetChild(i).transform;
            t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y - del, t.localPosition.z);
        }
    }
    private void Start()
    {
        UpdateContainer();
    }
    private void Update()
    {
        if (UIUsing) return;
        float origin = scrollPosition.y;
        if (mouseWheeling)
        {
            for (int i = 0; i < ScrollContainer.childCount; i++)
            {
                Transform t = ScrollContainer.GetChild(i).transform;
                t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y + (targetMouseY[i] - t.localPosition.y) / 5, t.localPosition.z);
            }
            if (Mathf.Abs(ScrollContainer.GetChild(0).transform.localPosition.y - targetMouseY[0]) <= 0.01f) mouseWheeling = false;
        }
        if (Input.touchCount > 0)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                scrollPosition.y -= Input.GetTouch(0).deltaPosition.y;
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
        }else if (scrollVelocity != 0.0f)
        {
            // slow down
            float t = (Time.time - timeTouchPhaseEnded) / inertiaDuration;
            float frameVelocity = Mathf.Lerp(scrollVelocity, 0, t);
            scrollPosition.y -= frameVelocity * Time.deltaTime;

            if (t >= inertiaDuration)
                scrollVelocity = 0;
        }
        if (Input.GetAxis("Mouse ScrollWheel") != 0) scrollPosition.y += Input.GetAxis("Mouse ScrollWheel") * 1000;
        float del = scrollPosition.y - origin;
        // 限定滑动区间
        if (del != 0)
        {
            RectTransform rect = ScrollContainer.GetChild(ScrollContainer.childCount - 1).GetComponent<RectTransform>();
            if (ScrollContainer.GetChild(ScrollContainer.childCount - 1).localPosition.y - del > LY - 1440)
            {
                //Debug.Log("Bottom Resist by " + ScrollContainer.GetChild(ScrollContainer.childCount - 1).name);
                if (!DownPlayed)
                {
                    DownAni.Play("ScrollLight", 0, 0.0f);
                    DownPlayed = true;
                    if (UpPlayed)
                    {
                        UpPlayed = false;
                        UpAni.Play("ScrollUnLight", 0, 0.0f);
                    }
                }
                del = ScrollContainer.GetChild(ScrollContainer.childCount - 1).localPosition.y - LY + 1440;
            }
            if (ScrollContainer.GetChild(0).localPosition.y - del < FY)
            {
                //Debug.Log("Top Resist by " + ScrollContainer.GetChild(0).name);
                if (!UpPlayed)
                {
                    UpAni.Play("ScrollLight", 0, 0.0f);
                    UpPlayed = true;
                    if (DownPlayed)
                    {
                        DownPlayed = false;
                        DownAni.Play("ScrollUnLight", 0, 0.0f);
                    }
                }
                del = ScrollContainer.GetChild(0).localPosition.y - FY;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
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
            else
            {
                for (int i = 0; i < ScrollContainer.childCount; i++)
                {
                    Transform t = ScrollContainer.GetChild(i).transform;
                    t.localPosition = new Vector3(t.localPosition.x, t.localPosition.y - del, t.localPosition.z);
                }
            }
        }
        else
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase != TouchPhase.Ended) return;
            }
            if (UpPlayed)
            {
                UpPlayed = false;
                UpAni.Play("ScrollUnLight", 0, 0.0f);
            }
            if (DownPlayed)
            {
                DownPlayed = false;
                DownAni.Play("ScrollUnLight", 0, 0.0f);
            }
        }

    }
}
