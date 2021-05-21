using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VitorBattleServer
{
    class VitorGame
    {
        public class VitorBattle
        {
            public enum TargetCamp  // 技能目标所在队伍
            {
                // 我方，敌方
                This, There
            }
            public enum TargetTag   // 技能常规目标枚举
            {
                // 单体，所在列，所在行
                Person, CurrentRow, CurrentLine,
                // 随机，全体
                Random, All,
                // 特定列，特定行
                SpecificRow, SpecificLine,
                // 十字周围，周围
                CrossSurroud, Surround
            }
            // 随机器
            public static Random random = new Random(Guid.NewGuid().GetHashCode());
            // 技能库
            public static List<Magic> MagicLibrary = new List<Magic>();
            // 角色库
            public static List<Player> PlayerLibrary = new List<Player>();
            /**
             * 创建技能
             * 参数：名称，处理函数，描述，能量条加成，连击次数
             */
            public static void CreateMagic(string name, MagicHandler handler, string description, float exbar,int hitcount)
            {
                MagicLibrary.Add(new Magic { handler = handler, Description = description, exBar = exbar, hitCount = hitcount, name = name });
                GameLog.Log($"[成功] 创建技能：{name}（{description}）",ConsoleColor.Green);
            }
            /**
             * 创建角色
             * 参数：名称，能力，普技名，大招名
             */
            public static void CreatePlayer(string name, PlayerBase playerbase, string basemagic,string exmagic)
            {
                Magic baseMagic = MagicLibrary.Find(m => m.name == basemagic);
                Magic exMagic = MagicLibrary.Find(m => m.name == exmagic);
                PlayerLibrary.Add(new Player { name = name, origin = playerbase, baseMagic = baseMagic, exMagic = exMagic });
                GameLog.Log($"[成功] 创建角色：{name}\n{PlayerLibrary[^1].origin.ToString()}\n" + 
                                  $"普技：（{baseMagic.name}）{baseMagic.Description}\n大招：（{exMagic.name}）{exMagic.Description}"
                                  ,ConsoleColor.Green);
            }
            /**
             * 添加Buff
             * 参数：战场，触发时机，衰退时机，作用对象，名称（ID），处理函数，生命周期
             */
            public static void CreateBuff(BattleField bf, Buff.Action action, Buff.Action decrease, object target, string name, BuffHandler handler, int SeasonTick)
            {
                Buff b = new Buff();
                b.action = action;
                b.decrease = decrease;
                b.name = name;
                b.Target = target;
                b.handler = handler;
                b.BaseSeasonTick = SeasonTick;
                b.SeasonTick = SeasonTick;
                int i = bf.buff.FindIndex(m => m.name == name);
                if (i != -1)
                {
                    bf.buff[i].SeasonTick += SeasonTick;
                    GameLog.Log($"{target.ToString()}的<{name}>Buff的层数增加到了：{bf.buff[i].Layer}", ConsoleColor.Yellow);
                }
                else
                {
                    GameLog.Log($"{target.ToString()}得到了<{name}>Buff",ConsoleColor.Cyan);
                    bf.buff.Add(b);
                }
            }
            /**
             * 抽取角色
             * 参数：角色名
             */
            public static Player GetPlayer(string name)
            {
                Player p = PlayerLibrary.Find(m => m.name == name);
                return new Player { name = name, origin = p.origin, baseMagic = p.baseMagic, exMagic = p.exMagic };
            }
            /**
             * 匹配对手
             * 参数：战场，发起战斗的玩家，匹配方式，目标阵营，[附加参数]
             */
            public static List<Player> MatchPlayer(BattleField bf, Player p, TargetTag target, TargetCamp camp, int addition = 0)
            {
                Player[,] Camp;
                List<Player> Target = new List<Player>();
                // 获取目标阵营
                if (p.side == 0)
                {
                    Camp = (camp == TargetCamp.This) ? bf.Camp1 : bf.Camp2;
                }
                else{
                    Camp = (camp == TargetCamp.This) ? bf.Camp2 : bf.Camp1;
                }
                int c = 0,avaliable = -1;
                // 环绕x，y坐标（十字环绕为前4个项，环绕为全部）
                int[] sx = new int[] { p.x,p.x,p.x - 1,p.x + 1,p.x - 1,p.x + 1,p.x - 1,p.x + 1 };
                int[] sy = new int[] { p.y - 1,p.y + 1,p.y,p.y,p.y - 1,p.y - 1,p.y + 1,p.y + 1 };
                // 根据目标策略挑选对手
                switch (target)
                {
                    // 单体
                    case (TargetTag.Person):
                        if(camp == TargetCamp.This)
                        {
                            // 若是我方则一定是我自己
                            Target.Add(p);
                        }
                        else
                        {
                            // 若是敌方则从最外层向里层遍历
                            for (int x = 0; x <= 2; x++)
                            {
                                if (Camp[x, p.y].HP > 0)
                                {
                                    Target.Add(Camp[x, p.y]);
                                    break;
                                }
                            }
                        }
                        break;
                    // 所在列
                    case (TargetTag.CurrentRow):
                        if(camp == TargetCamp.This)
                        {
                            // 如果是我方则所在行必定有玩家存在
                            for (int y = 0; y <= 1; y++)
                            {
                                if (Camp[p.x, y].HP > 0)
                                    Target.Add(Camp[p.x, y]);
                            }
                        }
                        else
                        {
                            // 如果是敌方则对过去的列是不定的
                            for (int x = 0; x <= 2; x++)
                            {
                                if (Camp[x, 0].HP > 0 || Camp[x, 1].HP > 0)
                                {
                                    for (int y = 0; y <= 1; y++)
                                        if (Camp[x, y].HP > 0)
                                            Target.Add(Camp[x, y]);
                                    break;
                                }
                            }
                        }
                        break;
                    // 所在行
                    case (TargetTag.CurrentLine):
                        // 如果是我方则所在列必定有玩家存在，敌方则不一定存在，那么不存在时将得到空目标组
                        for (int x = 0; x <= 2; x++)
                        {
                            if (Camp[x, p.y].HP > 0)
                                Target.Add(Camp[x, p.y]);
                        }
                        // 得到空目标组时只有可能是同一行上的敌方不存在
                        if (Target.Count == 0)
                        {
                            for (int x = 0; x <= 2; x++)
                            {
                                if (Camp[x, (p.y == 0) ? 1 : 0].HP > 0)
                                    Target.Add(Camp[x, (p.y == 0) ? 1 : 0]);
                            }
                        }
                        break;
                    // 指定的行
                    case (TargetTag.SpecificLine):
                        // 从上向下遍历
                        for (int y = 0; y <= 1; y++)
                        {
                            // 如果这一行上有玩家存在
                            if (Camp[0, y].HP > 0 || Camp[1, y].HP > 0 || Camp[2, y].HP > 0)
                            {
                                c++; 
                                avaliable = y;
                            }
                            // 达到要求的行
                            if (c == addition)
                            {
                                for (int x = 0; x <= 2; x++)
                                {
                                    if (Camp[x, y].HP > 0)
                                        Target.Add(Camp[x, y]);
                                }
                            }
                        }
                        // 若匹配失败，则填入玩家存在的最后一行
                        if(avaliable != -1 && Target.Count == 0)
                        {
                            for (int x = 0; x <= 2; x++)
                            {
                                if (Camp[x, avaliable].HP > 0)
                                    Target.Add(Camp[x, avaliable]);
                            }
                        }
                        break;
                    // 指定的列
                    case (TargetTag.SpecificRow):
                        // 从里向外遍历
                        for (int x = 0; x <= 2; x++)
                        {
                            // 如果这一列上有玩家存在
                            if (Camp[x, 0].HP > 0 || Camp[x, 1].HP > 0)
                            {
                                c++;
                                avaliable = x;
                            }
                            // 达到要求的列
                            if (c == addition)
                            {
                                for (int y = 0; y <= 1; y++)
                                {
                                    if (Camp[x, y].HP > 0)
                                        Target.Add(Camp[x, y]);
                                }
                                break;
                            }
                        }
                        // 若匹配失败，则填入玩家存在的最后一列
                        if (avaliable != -1 && Target.Count == 0)
                        {
                            for (int y = 0; y <= 1; y++)
                            {
                                if (Camp[avaliable, y].HP > 0)
                                    Target.Add(Camp[avaliable, y]);
                            }
                        }
                        break;
                    // 全体
                    case (TargetTag.All):
                        // 一口气把玩家全部塞进去
                        foreach (Player player in Camp) Target.Add(player);
                        break;
                    // 随机
                    case (TargetTag.Random):
                        // 将所有玩家塞入抽奖箱中
                        List<Player> box = new List<Player>();
                        foreach (Player player in Camp) box.Add(player);
                        while(c < addition)
                        {
                            // 抽取玩家并不放回
                            int ran_index = random.Next(0, box.Count);
                            Target.Add(box[ran_index]);
                            box.RemoveAt(ran_index);
                            c++;
                            // 如果不够抽取则退出
                            if (box.Count == 0) break;
                        }
                        break;
                    // 十字环绕
                    case (TargetTag.CrossSurroud):
                        // 根据已经算好的坐标判断是否越界，十字对应的是前4项
                        for(int i = 0;i < 4; i++)
                        {
                            if (sx[i] >= 0 && sx[i] <= 2 && sy[i] >= 0 && sy[i] <= 1 && Camp[sx[i], sy[i]].HP > 0)
                                Target.Add(Camp[sx[i], sy[i]]);
                        }
                        break;
                    // 环绕
                    case (TargetTag.Surround):
                        // 根据已经算好的坐标判断是否越界
                        for (int i = 0; i < 8; i++)
                        {
                            if (sx[i] >= 0 && sx[i] <= 2 && sy[i] >= 0 && sy[i] <= 1 && Camp[sx[i], sy[i]].HP > 0)
                                Target.Add(Camp[sx[i], sy[i]]);
                        }
                        break;
                }
                // 传回目标
                return Target;
            }
            /**
             * 服务端启动函数
             * 完成技能的初始化、通信建立等
             */
            static void Main(string[] args)
            {
                // 初始化所有角色和技能
                #region 慎的设定
                CreateMagic("盾击",
                (p, bf, hit) => {
                    List<Player> match = MatchPlayer(bf, p, TargetTag.Person, TargetCamp.There);
                    if (match.Count == 0) return;
                    Player e = match[0];
                    // 造成伤害
                    e.HP -= p.ATK * 0.7f;
                    CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.None, p, "钢之印记",
                        (t, bf, l) =>
                        {
                            return 0;
                        }
                    , 1);
                    CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.None, p, "抗暴增加20%",
                        (t, bf, l) =>
                        {
                            Player p = (Player)t;
                            p.extra.acri += 0.2f;
                            return 0;
                        }
                    , 1);
                    match = MatchPlayer(bf, p, TargetTag.Random, TargetCamp.This, 1);
                    if(match.Count != 0)
                    {
                        CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.OnRoundEnd, match[0], "攻击提升120%",
                            (t, bf, l) =>
                            {
                                Player p = (Player)t;
                                p.extra.atk += p.origin.atk * 0.2f;
                                return 0;
                            }
                        , 1);
                    }
                }, "给予对方70%攻击的伤害，并给自己附加一层钢之印记，增加20%抗暴，并随机为一位队友提升20%攻击。"
                ,10,1);
                CreateMagic("铁壁",
                (p, bf, hit) => {
                    List<Player> match = MatchPlayer(bf, p, TargetTag.Person, TargetCamp.There);
                    if (match.Count == 0) return;
                    Player e = match[0];
                    // 寻找自身的钢之印记
                    int i = bf.buff.FindIndex(m => m.name == "钢之印记" && m.Target.Equals(p));
                    int c = 1;
                    if (i != -1)
                    {
                        c = bf.buff[i].Layer;
                        // 擦除钢之印记
                        bf.RemoveBuff(bf.buff[i]);
                    }
                    // 造成伤害
                    e.HP -= p.ATK * 0.5f * c;
                    CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.None, p, "根据钢之印记个数提升防御",
                        (t, bf, l) =>
                        {
                            Player p = (Player)t;
                            p.extra.def += p.origin.def * 0.05f * c;
                            return 0;
                        }
                    , 2);
                    // 20%必定闪避
                    if(random.Next(0,100) <= 20)
                    {
                        CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.None, p, "必定闪避",
                            (t, bf, l) =>
                            {
                                Player p = (Player)t;
                                p.extra.mis += 1;
                                return 0;
                            }
                        , 2);
                    }
                }, "消除所有钢之印记，造成自身攻击的50%乘以印记数量的伤害，提升自身防御5%乘以印记数量，20%几率完全闪避一回合。"
                , 0, 1);
                CreatePlayer("慎",
                    new PlayerBase(600f, 50f, 30f, 13f, 0.07f, 0.1f, 0.04f, 0.1f),
                    "盾击", "铁壁"
                );
                #endregion
                #region 虚的设定
                CreateMagic("突刺",
                (p, bf, hit) => {
                    List<Player> match = MatchPlayer(bf, p, TargetTag.CurrentLine, TargetCamp.There);
                    if (match.Count == 0) return;
                    // 造成伤害
                    foreach(Player e in match) e.HP -= p.ATK * 1f;
                    CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.OnExtraReleased, p, "暴击率增加5%",
                        (t, bf, l) =>
                        {
                            Player p = (Player)t;
                            p.extra.cri += 0.05f;
                            return 0;
                        }
                    , 1);
                }, "给对手造成自身攻击的伤害，并增加自身暴击率5%直至大招释放。"
                , 10, 1);
                CreateMagic("空瞬",
                (p, bf, hit) => {
                    // 第一连击
                    if(hit == 1)
                    {
                        List<Player> match = MatchPlayer(bf, p, TargetTag.Person, TargetCamp.There);
                        if (match.Count == 0) return;
                        // 造成伤害
                        match[0].HP -= p.ATK * 1.2f;
                        CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.OnRoundEnd, p, "必定暴击且暴击效果提升25%",
                            (t, bf, l) =>
                            {
                                Player p = (Player)t;
                                p.extra.cri += 1f;
                                p.extra.ncri += 0.25f;
                                return 0;
                            }
                        , 1);
                        CreateBuff(bf, Buff.Action.OnRoundEnd, Buff.Action.OnRoundEnd, match[0], "降低50%的防御",
                            (t, bf, l) =>
                            {
                                Player p = (Player)t;
                                p.extra.def -= p.origin.def * 0.5f;
                                return 0;
                            }
                        , 1);
                    }
                    // 第二连击
                    if (hit == 2)
                    {
                        List<Player> match = MatchPlayer(bf, p, TargetTag.Person, TargetCamp.There);
                        if (match.Count == 0) return;
                        // 造成伤害
                        match[0].HP -= p.ATK * 2f;
                    }
                }, "(1) 给对方造成120%攻击的伤害，并提升自身暴力效果25%，下次必定暴击，减低对方50%的防御。\n（2）给对方造成200%攻击的伤害。"
                , 10, 2);
                CreatePlayer("虚",
                    new PlayerBase(450f, 70f, 20f, 16f, 0.1f, 0.15f, 0.04f, 0.1f),
                    "突刺", "空瞬"
                );
                #endregion
                // 启用socket监听
                new Thread(WebCommunication.Listening).Start();
                Console.ReadLine();
                Player[,] camp1 = new Player[,] { 
                    { GetPlayer("慎"),GetPlayer("虚") }, 
                    { GetPlayer("慎"),GetPlayer("慎") },
                    { GetPlayer("慎"),GetPlayer("虚") }
                };
                Player[,] camp2 = new Player[,] {
                    { GetPlayer("慎"),GetPlayer("虚") },
                    { GetPlayer("虚"),GetPlayer("虚") },
                    { GetPlayer("慎"),GetPlayer("虚") }
                };
                BattleField bf = new BattleField(camp1, camp2);
                Console.ReadLine();
                bf.Battle();
                Console.ReadLine();
            }
        }
    }
}
