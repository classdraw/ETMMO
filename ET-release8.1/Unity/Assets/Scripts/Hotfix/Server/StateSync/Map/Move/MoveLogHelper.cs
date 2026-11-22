using Unity.Mathematics;

namespace ET.Server
{
    public static class MoveLogHelper
    {
        public static void LogPathRequest(Unit unit, float3 target)
        {
            if (!StateSyncDebugLog.MoveEnabled)
            {
                return;
            }

            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            StateSyncDebugLog.LogMove($"[Move] 单位 {unit.Id} 寻路目标 ({target.x:F2},{target.y:F2},{target.z:F2})");
        }

        public static void LogStop(Unit unit, int error)
        {
            if (!StateSyncDebugLog.MoveEnabled)
            {
                return;
            }

            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            StateSyncDebugLog.LogMove(
                $"[Move] 单位 {unit.Id} Stop error={error} pos=({unit.Position.x:F2},{unit.Position.y:F2},{unit.Position.z:F2})");
        }

        public static void LogForbidMove(Unit unit)
        {
            if (!StateSyncDebugLog.MoveEnabled)
            {
                return;
            }

            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            StateSyncDebugLog.LogMove($"[Move] 单位 {unit.Id} 移动被禁止（ForbidMove）");
        }

        public static void LogMoveRejected(Unit unit, int error)
        {
            if (!StateSyncDebugLog.MoveEnabled)
            {
                return;
            }

            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            StateSyncDebugLog.LogMove($"[Move] 单位 {unit.Id} 移动被拒绝，Error={error}");
        }
    }
}