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
    }
}

