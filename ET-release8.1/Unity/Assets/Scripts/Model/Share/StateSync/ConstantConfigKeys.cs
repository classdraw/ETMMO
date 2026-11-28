namespace ET
{
    //ConstantConfig表所有通用key
    public static class ConstantConfigKeys
    {
        /// <summary>ConstantConfig 表：背包格子数量上限（读 <see cref="ConstantConfig.IntValue"/>）。</summary>
        public const int ConstantKnapsackMaxLoadConfigId = 90001;
        /// <summary>
        /// 默认地图，开始的地图
        /// </summary>
        public const int ConstantDefaultMap = 90002;

        /// <summary>玩家基础等级上限（读 <see cref="ConstantConfig.IntValue"/>）。</summary>
        public const int PlayerMaxBaseLevelConfigId = 90003;

        /// <summary>单项主属性（STR/AGI/VIT/INT/DEX/LUK）加点上限（读 <see cref="ConstantConfig.IntValue"/>）。</summary>
        public const int PlayerMaxPrimaryStatConfigId = 90004;
    }
}

