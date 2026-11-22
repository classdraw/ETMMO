namespace ET.Server
{
    /// <summary>BuffConfig 表字段解析。</summary>
    public static class BuffConfigHelper
    {
        /// <summary>
        /// 持续时长（毫秒）：TotalTime[0]=A，TotalTime[1]=B（可省略视为 0）→ A + ConfigLevel × B。
        /// 配置为空表示永久（0）。
        /// </summary>
        public static int CalcTotalTimeMs(BuffConfig config, int configLevel)
        {
            if (config == null)
            {
                return 0;
            }

            int[] param = config.TotalTime;
            if (param == null || param.Length == 0)
            {
                return 0;
            }

            int a = param[0];
            int b = param.Length > 1 ? param[1] : 0;
            if (configLevel < SkillStatusComponentSystem.DefaultSkillLevel)
            {
                configLevel = SkillStatusComponentSystem.DefaultSkillLevel;
            }

            long duration = (long)a + (long)configLevel * b;
            if (duration <= 0)
            {
                return 0;
            }

            if (duration > int.MaxValue)
            {
                return int.MaxValue;
            }

            return (int)duration;
        }
    }
}