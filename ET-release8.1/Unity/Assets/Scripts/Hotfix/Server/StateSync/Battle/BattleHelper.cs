using System;

namespace ET.Server
{
    [FriendOfAttribute(typeof(ET.Server.ReliveComponent))]
    public static class BattleHelper
    {
        /// <summary>
        /// 结算战斗
        ///  必须同步，不可以改成异步，否则复杂度高还有问题
        /// 可以用携程，但是不可把整个战斗计算改成异步
        /// </summary>
        public static void CalcAttack(Unit attacker, Unit target, Actions actions)
        {
            int[] param = actions.Config.ActionsParam;
            if (param == null || param.Length < 4)
            {
                Log.Error($"CalcAttack ActionsParam invalid: configId={actions.Config.Id}");
                return;
            }

            int damage = CalcDamage(attacker, param);
            if (damage == 0)
            {
                return;
            }

            NumericComponent numericComponent = target.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            long oldHp = numericComponent[NumericType.Hp];
            // damage < 0 扣血，damage > 0 加血
            long targetHp = numericComponent[NumericType.Hp] + damage;
            numericComponent[NumericType.HpBase] = Math.Clamp(targetHp, 0, numericComponent[NumericType.MaxHp]);

            long newHp = numericComponent[NumericType.Hp];
            long realDamage = newHp - oldHp;

            if (realDamage != 0)
            {
                //广播
                M2C_BattleResult m2CBattleResult = M2C_BattleResult.Create();
                m2CBattleResult.AttackerId = attacker.Id;
                m2CBattleResult.TargetId = target.Id;
                m2CBattleResult.Damage = realDamage;
                m2CBattleResult.IsCrit = false;
                MapMessageHelper.SendClient(target, m2CBattleResult, NoticeClientType.Broadcast);//客户端飘字
            }

            //死亡逻辑
            if (oldHp > 0 && newHp == 0)
            {
                Kill(attacker, target);
            }
        }

        /// <summary>
        /// ActionsParam: [0属性, 1物理/魔法, 2是否固定值(1固定), 3系数或固定伤害]
        /// </summary>
        private static int CalcDamage(Unit attacker, int[] param)
        {
            int isFixed = param[2];
            int coefficient = param[3];

            if (isFixed == 1)
            {
                return coefficient;
            }

            NumericComponent numericComponent = attacker.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return 0;
            }

            int atk = numericComponent.GetAsInt(NumericType.Atk);
            return (int)(atk * (coefficient / 100d));
        }

        /// <summary>
        /// 击杀
        /// </summary>
        /// <param name="killer">击杀者</param>
        /// <param name="killed">死亡者</param>
        public static void Kill(Unit killer, Unit killed)
        {
            //红名，pk值，记录被杀仇恨列表，击杀排行榜
            OnDead(killed);
        }

        public static void OnDead(Unit killed)
        {
            if (killed == null || killed.IsDisposed)
            {
                return;
            }

            ReliveComponent reliveComponent = killed.GetComponent<ReliveComponent>();
            if (reliveComponent == null)
            {
                return;
            }
            //已经死亡
            if (!killed.IsAlive())
            {
                return;
            }

            killed.Stop(0);//停止移动
            reliveComponent.Alive = false;
            switch (killed.Type())
            {
                case UnitType.Player:
                {
                    //被动死亡buff等
                    break;
                }
                case UnitType.Monster:
                {
                    TimerComponent timerComponent = killed.Root().GetComponent<TimerComponent>();
                    long now = TimeInfo.Instance.ServerFrameTime();
                    timerComponent.NewOnceTimer(now + 1000, TimerInvokeType.DeadMonsterTimer, killed);
                    break;
                }
            }
        }
    }
}

