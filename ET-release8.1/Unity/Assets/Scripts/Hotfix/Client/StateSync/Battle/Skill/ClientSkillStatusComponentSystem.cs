namespace ET.Client
{
    [EntitySystemOf(typeof(ClientSkillStatusComponent))]
    [FriendOf(typeof(ClientSkillStatusComponent))]
    public static partial class ClientSkillStatusComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSkillStatusComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ClientSkillStatusComponent self)
        {
            self.CoolDownEndTimes.Clear();
            self.CoolDownStartTimes.Clear();
        }

        public static void ApplyCoolDownChange(this ClientSkillStatusComponent self, M2C_CoolDownChange message)
        {
            if (self == null || self.IsDisposed || message == null)
            {
                return;
            }

            int count = message.CastConfigIds.Count;
            for (int i = 0; i < count; i++)
            {
                int castConfigId = message.CastConfigIds[i];
                long coolDownEndTime = message.CoolDownTimes[i];
                long coolDownStartTime = message.CoolDownStartTimes[i];
                self.CoolDownEndTimes[castConfigId] = coolDownEndTime;
                self.CoolDownStartTimes[castConfigId] = coolDownStartTime;
            }
        }

        public static bool IsCoolDown(this ClientSkillStatusComponent self, int castConfigId)
        {
            if (self == null || self.IsDisposed)
            {
                return false;
            }

            if (!self.CoolDownEndTimes.TryGetValue(castConfigId, out long coolDownEndTime))
            {
                return false;
            }

            return TimeInfo.Instance.ServerFrameTime() <= coolDownEndTime;
        }
    }
}
