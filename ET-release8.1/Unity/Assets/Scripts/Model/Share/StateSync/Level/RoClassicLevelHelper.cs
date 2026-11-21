namespace ET
{
    /// <summary>
    /// RO 经典版 Base Level 经验与升级可用点（见策划 <c>RO经典版战斗数值公式.md</c> §10、§11）。
    /// </summary>
    public static class RoClassicLevelHelper
    {
        /// <summary>本项目基础等级上限（一转阶段）。</summary>
        public const int MaxBaseLevel = 30;

        /// <summary>相对端游 Base 经验的难度倍率。</summary>
        public const int ExpDifficultyMultiplier = 3;

        /// <summary>1 级角色初始未消耗技能点。</summary>
        public const int InitialSkillPoints = 1;

        /// <summary>从当前 <paramref name="currentLevel"/> 升到下一级所需端游 Base EXP（rAthena pre-re BaseExp 表）。</summary>
        /// <summary>rAthena pre-re <c>BaseExp</c>：下标 <c>L-1</c> 表示 L 级→L+1 级所需端游经验（当前上限 30 级，用到 L=1..29）。</summary>
        [StaticField]
        private static readonly int[] BaseExpToNextLevel =
        {
            9, 16, 25, 36, 77, 112, 153, 200, 253, 320,
            385, 490, 585, 700, 830, 970, 1120, 1260, 1420, 1620,
            1860, 1990, 2240, 2504, 2950, 3426, 3934, 4474, 6889,
        };

        public static bool CanLevelUp(int currentLevel)
        {
            return currentLevel >= 1 && currentLevel < MaxBaseLevel && currentLevel <= BaseExpToNextLevel.Length;
        }

        public static int GetClassicBaseExpToNext(int currentLevel)
        {
            if (currentLevel < 1 || currentLevel > BaseExpToNextLevel.Length)
            {
                return int.MaxValue;
            }

            return BaseExpToNextLevel[currentLevel - 1];
        }

        /// <summary>本项目：从 <paramref name="currentLevel"/> 升到 <c>currentLevel+1</c> 所需经验。</summary>
        public static int GetNeedExpToNext(int currentLevel)
        {
            long need = (long)GetClassicBaseExpToNext(currentLevel) * ExpDifficultyMultiplier;
            return need > int.MaxValue ? int.MaxValue : (int)need;
        }

        /// <summary>升到等级 <paramref name="newLevel"/> 时发放的可用属性点（<c>newLevel≥2</c> 时有效）。</summary>
        public static int GetStatPointsOnReachLevel(int newLevel)
        {
            if (newLevel < 2)
            {
                return 0;
            }

            return 3 + (newLevel - 1) / 5;
        }

        /// <summary>升到等级 <paramref name="newLevel"/> 时增加的技能点（1 级初始点见 <see cref="InitialSkillPoints"/>）。</summary>
        public static int GetSkillPointsOnReachLevel(int newLevel)
        {
            return newLevel >= 2 ? 1 : 0;
        }

        /// <summary>创角 / 补档：假定从未分配属性点时，<paramref name="level"/> 级应有的剩余属性点。</summary>
        public static int GetTotalRemainingAttributePointsForLevel(int level)
        {
            if (level < 1)
            {
                level = 1;
            }

            int total = 0;
            for (int lv = 2; lv <= level; lv++)
            {
                total += GetStatPointsOnReachLevel(lv);
            }

            return total;
        }

        /// <summary>创角 / 补档：假定从未学技能消耗点时，<paramref name="level"/> 级应有的剩余技能点。</summary>
        public static int GetTotalRemainingSkillPointsForLevel(int level)
        {
            if (level < 1)
            {
                level = 1;
            }

            int total = InitialSkillPoints;
            for (int lv = 2; lv <= level; lv++)
            {
                total += GetSkillPointsOnReachLevel(lv);
            }

            return total;
        }
    }
}