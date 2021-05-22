using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace VitorBattleServer
{
    public class GameLog
    {
        public static void Log(string content,ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
    /**
     * 技能处理函数
     * 参数：释放玩家，所在的战场，连击序号
     */
    public delegate void MagicHandler(Player p, BattleField battleField, int index);
    /**
     * Buff处理函数
     * 参数：处理对象，所在的战场
     */
    public delegate float BuffHandler(object target, BattleField battleField,int layer);
    /**
     * 玩家基本数据
     * 储存攻击力、防御力等
     */
    public struct PlayerBase
    {
        // 血量，攻击，防御，速度，闪避，破甲，抗暴，暴击率
        public float hp, atk, def, spd, mis, brk, acri, cri;
        // 暴击加成
        public float ncri;
        // 构造函数
        public PlayerBase(float hp,float atk,float def,float spd, float brk, float cri, float mis,float acri)
        {
            this.hp = hp; this.atk = atk; this.def = def; this.spd = spd;
            this.mis = mis; this.brk = brk; this.acri = acri; this.cri = cri;
            ncri = 2.0f;
        }
        public override string ToString()
        {
            return $"HP：{hp}，ATK：{atk}，DEF：{def}，SPD：{spd}\nMIS：{mis}，BRK：{brk}，ACRI：{acri}，CRI：{cri}，NCRI：{ncri}";
        }
    }
    /**
     * 玩家
     * 控制玩家的数据等
     */
    public class Player
    {
        public PlayerBase origin;       // 原始数据
        public PlayerBase active;       // 战斗时的数据
        public PlayerBase extra;        // 战斗时附加的数据，每回合结束清楚一次
        public Magic baseMagic;         // 普攻
        public Magic exMagic;           // 大招
        public Equipment equipment;     // 装备
        public int x, y, side;          // 在队伍的位置
        public string name;             // 名字
        public float exBar;             // 能量条
        // 取得战斗时显示的数值
        public float MaxHP { get { return origin.hp + equipment.buff.hp ; } }
        public float HP { 
            get 
            { 
                return active.hp + equipment.buff.hp + extra.hp; 
            }
            set
            {
                GameLog.Log($"{name}.{side}({x},{y})：HP {HP}->{value - equipment.buff.hp - extra.hp}",ConsoleColor.White);
                active.hp = value - equipment.buff.hp - extra.hp;
                if (active.hp <= 0) GameLog.Log($"{name}.{side}({x},{y})被击败了！",ConsoleColor.Red);
            }
        }
        public float ATK { get { return active.atk + equipment.buff.atk + extra.atk; } }
        public float DEF { get { return active.def + equipment.buff.def + extra.def; } }
        public float SPD { get { return active.spd + equipment.buff.spd + extra.spd; } }
        public float MIS { get { return active.mis + equipment.buff.mis + extra.mis; } }
        public float BRK { get { return active.brk + equipment.buff.brk + extra.brk; } }
        public float ACRI { get { return active.acri + equipment.buff.acri + extra.acri; } }
        public float CRI { get { return active.cri + equipment.buff.cri + extra.cri; } }
        public float NCRI { get { return active.ncri + equipment.buff.ncri + extra.ncri; } }
        public void Reset()
        {
            active = origin;
        }
        public override string ToString()
        {
            return $"{name}.{side}({x},{y})（角色）";
        }
        public Player()
        {
            equipment = new Equipment();
        }
    }
    // 技能结构体
    public class Magic
    {
        public MagicHandler handler;    // 处理函数
        public string Description;      // 技能描述
        public float exBar;             // 能量条增幅
        public int hitCount;            // 连击次数
        public string name;             // 技能名称
        public override string ToString()
        {
            return $"{name}（技能）";
        }
    }
    // Buff结构体
    public class Buff
    {
        public enum Action              // 触发时机枚举
        {
            None, OnRoundEnd, OnExtraReleased
        }
        public Action decrease;         // 衰落时机
        public Action action;           // 触发时机
        public BuffHandler handler;     // 处理函数
        public int SeasonTick;          // Buff有效周期计数
        public int BaseSeasonTick;      // Buff基本周期计数
        public object Target;           // 操作对象
        public string name;             // Buff名称（ID）
        public int Layer                // 层数
        {
            get { return (int)Math.Floor(SeasonTick / BaseSeasonTick * 1f); }
        }
        public override string ToString()
        {
            return $"{name}（Buff）";
        }
    }
    // 装备结构体
    public class Equipment
    {
        public PlayerBase buff;         // 加成
        public string name;             // 装备名称
        public override string ToString()
        {
            return $"{name}（装备）";
        }
    }
    // 战场
    public class BattleField
    {
        // 全局buff表
        public List<Buff> buff = new List<Buff>();
        // 过程可视化指令
        public string Visual = "";
        /**
         * 队伍
         * 注意：在显示时，0显示在中间的两侧，然后依次向两边排列
         */
        public Player[,] Camp1 = new Player[2, 1];  // 左边一方
        public Player[,] Camp2 = new Player[2, 1];  // 右边一方
        // 构造函数
        public BattleField(Player[,] camp1, Player[,] camp2)
        {
            GameLog.Log($"[成功] 创建新的战场（{this.GetHashCode()}）",ConsoleColor.Green);
            Camp1 = camp1; Camp2 = camp2;
            for(int x = 0;x <= 2; x++)
            {
                for(int y = 0;y <= 1; y++)
                {
                    // 格式化数据
                    Camp1[x, y].x = x; Camp1[x, y].y = y; Camp1[x, y].side = 0;
                    Camp2[x, y].x = x; Camp2[x, y].y = y; Camp2[x, y].side = 1;
                    Camp1[x, y].Reset(); Camp2[x, y].Reset();
                }
            }
        }
        // 消除Buff
        public void RemoveBuff(Buff remove)
        {
            GameLog.Log($"{remove.Target.ToString()}的<{remove.name}>Buff消失了！",ConsoleColor.Red);
            buff.Remove(remove);
        }
        // 触发Buff
        public void CarryBuff(Buff.Action action)
        {
            List<Buff> remove = new List<Buff>();
            foreach (Buff buff in buff.FindAll(m => m.action == action))
            {
                buff.handler(buff.Target, this, buff.Layer);
            }
            foreach (Buff buff in buff.FindAll(m => m.decrease == action))
            {
                buff.SeasonTick-=buff.Layer;
                if (buff.SeasonTick <= 0) remove.Add(buff);
            }
            for (int i = 0; i < remove.Count; i++) RemoveBuff(buff[i]);
        }
        // 战斗⚔
        public void Battle()
        {
            List<Player> camp1 = new List<Player>(), camp2 = new List<Player>(), all = new List<Player>();
            foreach (Player p in Camp1) { camp1.Add(p); all.Add(p); }
            foreach (Player p in Camp2) { camp2.Add(p); all.Add(p); }
            // 根据速度排序
            all.Sort((n, m) => m.SPD.CompareTo(n.SPD));
            Console.Write("攻击顺序：");
            for (int i = 0; i < all.Count; i++)
                Console.Write($"{all[i].name}.{all[i].side}({all[i].x},{all[i].y})、");
            Console.Write("\n");
            int round = 1;
            // 任何一方没有全灭的话
            while (camp1.FindAll(m => m.HP > 0).Count > 0 && camp2.FindAll(m => m.HP > 0).Count > 0)
            {
                CarryBuff(Buff.Action.OnRoundEnd);
                for (int i = 0; i < all.Count; i++)
                {
                    if(all[i].HP > 0)
                    {
                        GameLog.Log($"~回合{round}~{all[i].name}.{all[i].side}({all[i].x},{all[i].y})：",ConsoleColor.Green);
                        if (all[i].exBar >= 100)
                        {
                            CarryBuff(Buff.Action.OnExtraReleased);
                            GameLog.Log($"释放了大招<{all[i].exMagic.name}>！");
                            for (int hit = 1; hit <= all[i].exMagic.hitCount; hit++)
                            {
                                GameLog.Log($"连击{hit}！");
                                all[i].exMagic.handler(all[i], this, hit);
                            }
                            all[i].exBar = 0;
                        }
                        else
                        {
                            all[i].exBar += all[i].baseMagic.exBar;
                            GameLog.Log($"释放了普技<{all[i].baseMagic.name}>，能量+{all[i].baseMagic.exBar}（{all[i].exBar}/100）。");
                            for (int hit = 1;hit <= all[i].baseMagic.hitCount;hit++)
                            {
                                GameLog.Log($"连击{hit}！");
                                all[i].baseMagic.handler(all[i], this, hit);
                            }
                        }
                    }
                }
                round++;
            }
            if (camp1.FindAll(m => m.HP > 0).Count > 0) GameLog.Log("队伍1获胜！！！",ConsoleColor.Green);
            if (camp2.FindAll(m => m.HP > 0).Count > 0) GameLog.Log("队伍2获胜！！！", ConsoleColor.Green);
        }
    }
}
