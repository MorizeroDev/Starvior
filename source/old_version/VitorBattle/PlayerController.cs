using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum BattleState{
        Normal,Attack,Miss,Prepare,Attack2,SmileWin,
        Magic,Attack3,BadHurt,ExPrepare,Attack4,
        ExBadHurt,Hurt,Attack5,Sleep,Miss2,Win,Die
    }
    public float fps = 6;               //每秒行走图刷新次数
    private SpriteRenderer s;           //控制对象图片
    private Sprite[] walker;            //行走图图片集
    public string character;            //使用的人物的行走图名称
    public bool IsRightSide = false;
    public bool Spelling = false;
    public BattleState State = BattleState.Normal;
    private BattleState lState;
    private int tick = 0;
    private float lowx,highx;
    private float ttick = 0;
    private void Awake() {
        s = this.gameObject.GetComponent<SpriteRenderer>();
        walker = Resources.LoadAll<Sprite>("Players/" + character);
        Vector3 v = this.gameObject.transform.localPosition;
        highx = v.x;lowx = highx + (IsRightSide ? 1 : -1);
        s.flipX = !IsRightSide;
    }
    public void Up(){
        Awake();
    }
    void Update() {
        if(lState != State){
            tick = 0; lState = State;
        }
        ttick += Time.deltaTime;
        if(ttick > 1 / fps){
        	tick++; ttick = 0;
        }
        if(tick > 2 && (State == BattleState.Normal || State == BattleState.Magic)) 
        	tick = 0;
        if(tick > 2) 
        	tick = 2;

        s.sprite = walker[tick + (int)State * 3];
        float suitx = 
        	(State == BattleState.Miss || State == BattleState.Miss2 || State == BattleState.Hurt)
        	? lowx : highx;
        if(Spelling) 
        	suitx += (IsRightSide ? -2 : 2);

        Transform v = this.gameObject.transform;
        if(v.localPosition.x != suitx){
            v.localPosition = new Vector3(v.localPosition.x + (suitx - v.localPosition.x) / 3,v.localPosition.y,v.localPosition.z);
        }
    }
}
