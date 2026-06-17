namespace ET.Server
{
    [EntitySystemOf(typeof(Buff))]
    [FriendOf(typeof(Buff))]
    public static partial class BuffSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.Buff self, int configId)
        {
            self.ConfigId = configId;
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.Buff self)
        {
            self.ConfigId = 0;
            self.Owner = null;
            self.AddUnitId = 0;
            self.AddSkillId = 0;
            self.CreateTime = 0;
            self.TickTime = 0;
            self.TickBeginTime = 0;
            self.ExpireTime = 0;
            self.TickTimer = 0;
            self.WaitTickTimer = 0;
            self.ExpireTimer = 0;
        }

        public static void InitTime(this Buff self, int durationMs)
        {
            long now = TimeInfo.Instance.ServerFrameTime();
            self.CreateTime = now;
            self.TickBeginTime = now;
            self.ExpireTime = durationMs > 0 ? now + durationMs : 0;
        }

        /// <summary>
        /// 时长叠加：在剩余时间基础上继续增加，可越叠越长。
        /// </summary>
        public static void AddDuration(this Buff self, int durationMs)
        {
            if (durationMs <= 0)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            long baseTime = self.ExpireTime > now ? self.ExpireTime : now;
            self.ExpireTime = baseTime + durationMs;
        }

        /// <summary>
        /// 时长重置：从当前时刻重新计算配置时长，不叠加。
        /// </summary>
        public static void ResetDuration(this Buff self, int durationMs)
        {
            long now = TimeInfo.Instance.ServerFrameTime();
            self.TickBeginTime = now;
            self.ExpireTime = durationMs > 0 ? now + durationMs : 0;
        }
    }
}
