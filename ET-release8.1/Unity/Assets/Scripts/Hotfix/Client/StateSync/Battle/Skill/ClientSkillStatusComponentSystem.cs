namespace ET.Client
{
    [EntitySystemOf(typeof(ClientSkillStatusComponent))]
    [FriendOf(typeof(ClientSkillStatusComponent))]
    [FriendOf(typeof(ClientCastComponent))]
    [FriendOf(typeof(ClientCast))]
    public static partial class ClientSkillStatusComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ClientSkillStatusComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ClientSkillStatusComponent self)
        {
            self.CurrentSkillCastInstanceId = default;
            self.CoolDownEndTimes.Clear();
            self.CoolDownStartTimes.Clear();
        }

        /// <summary>
        /// 施法期间是否拦截服务端寻路：无当前行为技能不拦；当前行为技能 Type==2（位移）不拦；Type==1 拦。
        /// </summary>
        public static bool BlocksMoveWhileCasting(this Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return false;
            }

            ClientCastComponent castComponent = unit.GetComponent<ClientCastComponent>();
            if (castComponent == null || castComponent.IsDisposed || !castComponent.IsCasting())
            {
                return false;
            }

            ClientSkillStatusComponent skillStatus = unit.GetComponent<ClientSkillStatusComponent>();
            if (skillStatus == null || skillStatus.IsDisposed || skillStatus.CurrentSkillCastInstanceId == 0)
            {
                return false;
            }

            ClientCast currentCast = castComponent.Get(skillStatus.CurrentSkillCastInstanceId);
            if (currentCast == null || currentCast.IsDisposed)
            {
                return false;
            }

            return currentCast.Config.Type != (int)CastType.Displacement;
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
