namespace ET.Client
{
    //特效管理
    public static class EffectHelper
    {
        public static async ETTask<Unit> CreateEffect(Unit target,int configId)
        {
            EffectConfig effectConfig = EffectConfigCategory.Instance.Get(configId);
            //加载特效出来

            await ETTask.CompletedTask;
            return null;
        }
    }
}

