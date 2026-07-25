using System.Collections.Generic;
using Unity.Mathematics;
using System;

namespace ET.Server
{
    [FriendOf(typeof(NumericComponent))]
    public static partial class NumericHelper
    {
        /// <summary>计算从 1 累加到 <paramref name="n"/> 的和，即 1 + 2 + … + <paramref name="n"/>。</summary>
        /// <param name="n">正整数。</param>
        /// <returns>三角形数 n(n+1)/2。</returns>
        public static int SumFromOneToN(int n)
        {
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), n, "n 必须为正整数");
            }

            return n * (n + 1) / 2;
        }
        //根据属性计算出血量
        public static int CalcHpResult(int level, int vit, double jobHp)
        {
            int levelAdd = SumFromOneToN(level);
            double baseHp = 35d + level * 5d + levelAdd * jobHp;
            double maxHp = baseHp * (1d + vit / 100d);
            return (int)maxHp;
        }
        
        //根据属性计算出血量
        public static int CalcSpResult(int level, int intell, double jobSp)
        {
            double baseSp = 10d + level * jobSp;
            double maxSp = baseSp * (1d + intell / 100d);
            return (int)maxSp;
        }

        public static int CalcPlayerMeleeAtk(int str, int dex, int luk)
        {
            return str + (str / 10) * (str / 10) + dex / 5 + luk / 5;
        }

        public static int CalcPlayerRangedAtk(int dex, int str, int luk)
        {
            return dex + (dex / 10) * (dex / 10) + str / 5 + luk / 5;
        }

        public static int CalcPlayerDef(int vit)
        {
            return (int)(vit * 0.8d);
        }

        public static int CalcPlayerMAtkMin(int intell)
        {
            return intell + (intell / 7) * (intell / 7);
        }

        public static int CalcPlayerMAtkMax(int intell)
        {
            return intell + (intell / 5) * (intell / 5);
        }

        public static int CalcPlayerMDef(int intell)
        {
            return intell / 2;
        }

        public static int CalcPlayerHit(int level, int dex)
        {
            return level + dex;
        }

        public static int CalcPlayerFlee(int level, int agi)
        {
            return level + agi;
        }

        public static float CalcPlayerAtkSpeed(int agi, int dex)
        {
            float denominator = 200f - (agi + dex / 4f);
            if (denominator <= 0f)
            {
                return 0f;
            }

            return 50f / denominator;
        }
    }
}

