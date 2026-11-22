namespace ET
{
    /// <summary>StateSync 玩法调试控制台日志分模块开关，默认全部关闭。</summary>
    public static class StateSyncDebugLog
    {
        /// <summary>Buff 增删改与 Buff 数值变化。</summary>
        [StaticField]
        public static bool BuffEnabled=false;


        /// <summary>移动 / 寻路 / Stop 等。</summary>
        [StaticField]
        public static bool MoveEnabled = false;

        public static void LogBuff(string message)
        {
            if (!BuffEnabled)
            {
                return;
            }

            Log.Console(message);
        }

        public static void LogMove(string message)
        {
            if (!MoveEnabled)
            {
                return;
            }

            Log.Console(message);
        }
    }
}