namespace ET
{
    /// <summary>
    /// RO 经典 Base 等级：经验条 <see cref="NumericType.EXP"/>、升级属性点、每级技能点（无 Job 等级/Job 经验）。
    /// </summary>
    public static class RoClassicLevelHelper
    {
        /// <summary>未配表时玩家基础等级上限回退值。</summary>
        public const int DefaultMaxPlayerBaseLevel = 99;

        /// <summary>未配表时单项主属性上限回退值。</summary>
        public const int DefaultMaxPrimaryStatValue = 99;

        /// <summary>相对端游 Base 经验的难度倍率。</summary>
        public const int ExpDifficultyMultiplier = 3;

        /// <summary>1 级角色初始未消耗技能点。</summary>
        public const int InitialSkillPoints = 1;

        /// <summary>玩家基础等级上限，来自 <see cref="ConstantConfigKeys.PlayerMaxBaseLevelConfigId"/>。</summary>
        public static int GetMaxPlayerBaseLevel()
        {
            return GetConstantIntValue(ConstantConfigKeys.PlayerMaxBaseLevelConfigId, DefaultMaxPlayerBaseLevel);
        }

        /// <summary>单项主属性加点上限，来自 <see cref="ConstantConfigKeys.PlayerMaxPrimaryStatConfigId"/>。</summary>
        public static int GetMaxPrimaryStatValue()
        {
            return GetConstantIntValue(ConstantConfigKeys.PlayerMaxPrimaryStatConfigId, DefaultMaxPrimaryStatValue);
        }

        static int GetConstantIntValue(int configId, int fallback)
        {
            ConstantConfigCategory category = ConstantConfigCategory.Instance;
            if (category == null || !category.Contain(configId))
            {
                return fallback;
            }

            int value = category.Get(configId).IntValue;
            return value > 0 ? value : fallback;
        }

        /// <summary>配置表 <see cref="ExpConfig"/> 的 Id 与当前等级一致，表示该级升到下一级所需端游 Base EXP。</summary>
        public static bool CanLevelUp(int currentLevel)
        {
            int maxLevel = GetMaxPlayerBaseLevel();
            if (currentLevel < 1 || currentLevel >= maxLevel)
            {
                return false;
            }

            ExpConfigCategory category = ExpConfigCategory.Instance;
            return category != null && category.Contain(currentLevel);
        }

        public static int GetClassicBaseExpToNext(int currentLevel)
        {
            ExpConfigCategory category = ExpConfigCategory.Instance;
            if (category == null || !category.Contain(currentLevel))
            {
                return int.MaxValue;
            }

            return category.Get(currentLevel).NeedExp;
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

        /// <summary>RO 经典：属性当前值为 <paramref name="currentStatValue"/> 时再 +1 所需属性点（⌊(V-1)/10⌋+2）。</summary>
        public static int GetAttributePointCostToIncrease(int currentStatValue)
        {
            if (currentStatValue < 1)
            {
                currentStatValue = 1;
            }

            return (currentStatValue - 1) / 10 + 2;
        }

        /// <summary>从 <paramref name="statAtStart"/> 起连续加 <paramref name="increaseCount"/> 点属性所消耗的总属性点。</summary>
        public static int ComputeAttributePointCostForIncreaseRange(int statAtStart, int increaseCount)
        {
            if (increaseCount <= 0)
            {
                return 0;
            }

            if (statAtStart < 1)
            {
                statAtStart = 1;
            }

            int total = 0;
            int value = statAtStart;
            for (int i = 0; i < increaseCount; i++)
            {
                total += GetAttributePointCostToIncrease(value);
                value++;
            }

            return total;
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