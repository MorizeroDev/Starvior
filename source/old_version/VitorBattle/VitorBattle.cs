using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VitorBattleSystem{
public delegate void MagicHandler(Player p);
public delegate void BuffHandler(object target);
public struct PlayerBase{
    public float hp,atk,def,spd,mis,brk,acri,cri;
    public float ncri;
}
public class Player{
    public PlayerBase origin;
    public PlayerBase active;
    public Equipment equipment;
    public float HP{get{return active.hp + equipment.buff.hp;}}
    public float ATK{get{return active.atk + equipment.buff.atk;}}
    public float DEF{get{return active.def + equipment.buff.def;}}
    public float SPD{get{return active.spd + equipment.buff.spd;}}
    public float MIS{get{return active.mis + equipment.buff.mis;}}
    public float BRK{get{return active.brk + equipment.buff.brk;}}
    public float ACRI{get{return active.acri + equipment.buff.acri;}}
    public float CRI{get{return active.cri + equipment.buff.cri;}}
    public float NCRI{get{return active.ncri + equipment.buff.ncri;}}
    public void Reset(){
        active = origin;
    }
}
public class Magic{
    public MagicHandler handler;
    public string Description;
}
public class Buff{
    public enum Action{             // 触发时机枚举
        OnRoundEnd
    }
    public Action action;           // 触发时机
    public BuffHandler handler;     // 处理函数
    public int SeasonTick;          // Buff有效周期计数
    public int BaseSeasonTick;      // Buff基本周期计数
    public object Target;           // 操作对象
    public string Name;             // Buff名称（ID）
}
public class Equipment{
    public PlayerBase buff;
    public string Name;
}
public class BattleField{
    public static List<Buff> buff = new List<Buff>();
    public static Player[,] Camp1 = new Player[2,1];
    public static Player[,] Camp2 = new Player[2,1];
}
public class VitorBattle : MonoBehaviour
{
    public static Dictionary<Magic,string> MagicLibrary;
    public static void CreateMagic(string name,MagicHandler handler,string description){
        MagicLibrary.Add(new Magic{handler = handler,Description = description},name);
    }
    public static void CreateBuff(Buff.Action action,object target, string name,BuffHandler handler,int SeasonTick){
        Buff b = new Buff();
        b.action = action;
        b.Name = name;
        b.Target = target;
        b.handler = handler;
        b.BaseSeasonTick = SeasonTick;
        b.SeasonTick = SeasonTick;
        int i = BattleField.buff.FindIndex(m => m.Name == name);
        if(i != -1){
            BattleField.buff[i].SeasonTick += SeasonTick;
        }else{
            BattleField.buff.Add(b);
        }
    }
    public static Player MatchPlayer(Player p){
        return p;
    }
    static VitorBattle(){
        // 初始化所有技能
        CreateMagic("普攻",
        (i)=>{
            Player e = MatchPlayer(i);
            e.active.hp -= i.ATK;
        },"没有描述，就是很普通的技能。");
        CreateMagic("毒攻",
        (i)=>{
            Player e = MatchPlayer(i);
            e.active.hp -= i.ATK;
            CreateBuff(Buff.Action.OnRoundEnd, e, "中毒",
                (t) => {
                    Player e = (Player)t;
                    e.active.hp -= e.origin.hp * 0.1f;
                },3);
        },"没错，我就是来测试buff的。");
    }
}

}