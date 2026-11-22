namespace ET.Server
{
    [FriendOf(typeof(Buff))]
    public static class BuffLogHelper
    {
        public static void LogAdd(Buff buff, Unit owner)
        {
            if (!StateSyncDebugLog.BuffEnabled)
            {
                return;
            }

            if (buff == null || buff.IsDisposed || owner == null || owner.IsDisposed)
            {
                return;
            }

            long remainMs = GetRemainMs(buff);
            StateSyncDebugLog.LogBuff(
                $"[Buff] 单位 {owner.Id} 新增 buffId={buff.Id} configId={buff.ConfigId} layer={buff.Layer} " +
                $"剩余={remainMs}ms ConfigLevel={buff.ConfigLevel} castConfigId={buff.AddCastConfigId} 来源Unit={buff.AddUnitId}");
        }

        public static void LogUpdate(Buff buff, Unit owner)
        {
            if (!StateSyncDebugLog.BuffEnabled)
            {
                return;
            }

            if (buff == null || buff.IsDisposed || owner == null || owner.IsDisposed)
            {
                return;
            }

            long remainMs = GetRemainMs(buff);
            StateSyncDebugLog.LogBuff(
                $"[Buff] 单位 {owner.Id} 刷新 buffId={buff.Id} configId={buff.ConfigId} layer={buff.Layer} " +
                $"剩余={remainMs}ms ConfigLevel={buff.ConfigLevel}");
        }

        public static void LogRemove(Buff buff, Unit owner)
        {
            if (!StateSyncDebugLog.BuffEnabled)
            {
                return;
            }

            if (buff == null || owner == null || owner.IsDisposed)
            {
                return;
            }

            StateSyncDebugLog.LogBuff(
                $"[Buff] 单位 {owner.Id} 移除 buffId={buff.Id} configId={buff.ConfigId} layer={buff.Layer} " +
                $"ConfigLevel={buff.ConfigLevel} castConfigId={buff.AddCastConfigId}");
        }

        public static void LogNumericChange(Buff buff, Unit target, int actionConfigId, int numericType, long before, long after,
            ActionsRunType actionsRunType)
        {
            if (!StateSyncDebugLog.BuffEnabled)
            {
                return;
            }

            if (buff == null || target == null || target.IsDisposed)
            {
                return;
            }

            string phase = actionsRunType == ActionsRunType.BuffRemove ? "移除还原" : "附加";
            long delta = after - before;
            StateSyncDebugLog.LogBuff(
                $"[Buff] 单位 {target.Id} buffId={buff.Id} configId={buff.ConfigId} Action={actionConfigId} {phase} " +
                $"数值类型={numericType} {before} -> {after} (Δ{delta})");
        }

        private static long GetRemainMs(Buff buff)
        {
            if (buff.ExpireTime <= 0)
            {
                return 0;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            long remain = buff.ExpireTime - now;
            return remain > 0 ? remain : 0;
        }
    }
}