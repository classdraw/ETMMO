namespace ET
{
    public static class KnapsackConfigHelper
    {
        /// <summary>ConstantConfig 90001：背包格子数量上限，客户端 UI 与服务端容量共用。</summary>
        public static int GetKnapsackMaxLoad()
        {
            if (!ConstantConfigCategory.Instance.Contain(ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId))
            {
                Log.Warning($"ConstantConfig 缺少 Id={ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId}，背包上限使用回退值 500");
                return 200;
            }

            int v = ConstantConfigCategory.Instance.Get(ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId).IntValue;
            if (v <= 0)
            {
                Log.Warning($"ConstantConfig Id={ConstantConfigKeys.ConstantKnapsackMaxLoadConfigId} IntValue<=0，背包上限使用回退值 500");
                return 200;
            }

            return v;
        }
    }
}
