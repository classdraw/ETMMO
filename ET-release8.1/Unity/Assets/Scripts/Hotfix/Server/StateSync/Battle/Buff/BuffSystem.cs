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

        public static BuffProto ToMessage(this Buff self)
        {
            BuffProto buffProto = BuffProto.Create(true);
            buffProto.Id = self.Id;
            buffProto.ConfigId = self.ConfigId;
            buffProto.ExpireTime = self.ExpireTime;
            buffProto.CreateTime = self.CreateTime;
            buffProto.ExtraData = self.ToExtraDataBytes();
            return buffProto;
        }

        public static void FromMessage(this Buff self, BuffProto buffProto)
        {
            if (buffProto == null)
            {
                return;
            }

            self.ConfigId = buffProto.ConfigId;
            self.CreateTime = buffProto.CreateTime;
            self.ExpireTime = buffProto.ExpireTime;
            self.FromExtraDataBytes(buffProto.ExtraData);
        }

        private static byte[] ToExtraDataBytes(this Buff self)
        {
            BuffExtraData extraData = new BuffExtraData
            {
                AddUnitId = self.AddUnitId,
                AddSkillId = self.AddSkillId,
                TickTime = self.TickTime,
                TickBeginTime = self.TickBeginTime,
            };
            return MongoHelper.Serialize(extraData);
        }

        private static void FromExtraDataBytes(this Buff self, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                self.AddUnitId = 0;
                self.AddSkillId = 0;
                self.TickTime = 0;
                self.TickBeginTime = 0;
                return;
            }

            BuffExtraData extraData = MongoHelper.Deserialize<BuffExtraData>(bytes);
            self.AddUnitId = extraData.AddUnitId;
            self.AddSkillId = extraData.AddSkillId;
            self.TickTime = extraData.TickTime;
            self.TickBeginTime = extraData.TickBeginTime;
        }
    }
}
