using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSeaController : MonoBehaviour
{
    public float Speed = 0.1f;
    private float lastTime;
    private SpriteRenderer spriteRenderer;
    public float sx, sy, ex, ey;
    public Transform sD, eD;
    public bool isController = false;
    public float MaxScale = 0.03f;
    public float degree;
    public float MoveSpeed = 14;
    public int Mount;
    private bool seted = false;

    float Cubic(float t,float a,float b,float c,float d)
    {
        return a * Mathf.Pow(1 - t, 3) + 3 * b * t * Mathf.Pow(1 - t, 2) + 3 * c * Mathf.Pow(t, 2) * (1 - t) + d * Mathf.Pow(t, 3);
    }

    private void Start()
    {
        if (isController)
        {
            sx = sD.position.x; sy = sD.position.y;
            ex = eD.position.x; ey = eD.position.y;
        }
        else
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
    void Update()
    {
        if (isController)
        {
            if(Time.time - lastTime > Speed)
            {
                lastTime = Time.time;
                for(int i = 0; i < Mount; i++)
                {
                    StarSeaController star = Instantiate(this.gameObject, transform.parent).GetComponent<StarSeaController>();
                    star.sx = this.sx; star.sy = this.sy; star.ex = this.ex; star.ey = this.ey;
                    star.degree = Random.Range(0.0f, Mathf.PI * 2);
                    star.isController = false;
                    star.lastTime = Time.time;
                    star.MoveSpeed = this.MoveSpeed;
                    star.MaxScale = this.MaxScale;
                    star.transform.eulerAngles = new Vector3(0, 0, star.degree / 3.14f * 180f);
                    star.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            float delta = Time.time - lastTime;
            float a = 0;
            this.transform.position = new Vector3(Mathf.Cos(degree) * delta * MoveSpeed, Mathf.Sin(degree) * delta * MoveSpeed, 0);
            if (this.transform.position.x < sx || this.transform.position.y > sy || this.transform.position.x > ex || this.transform.position.y < ey)
            {
                //Debug.Log(transform.position.x + "," + transform.position.y + "|" + sx + "," + sy + "|" + ex + "|" + ey);
                GameObject.Destroy(this.gameObject);
            }
            if (seted) return;
            if (delta > 0.5f)
            {
                a = 1;
                seted = true;
            }
            else
            {
                a = Cubic(delta / 0.5f,0,1,1,1);
            }
            spriteRenderer.color = new Color(1, 1, 1, a);
            this.transform.localScale = new Vector3(a * MaxScale, a * MaxScale, 1);
        }
    }
}
