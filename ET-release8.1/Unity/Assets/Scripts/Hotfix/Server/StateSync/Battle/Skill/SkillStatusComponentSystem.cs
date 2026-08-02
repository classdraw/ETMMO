namespace ET.Server
{
    [EntitySystemOf(typeof(SkillStatusComponent))]
    [FriendOf(typeof(SkillStatusComponent))]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public static partial class SkillStatusComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.SkillStatusComponent self)
        {

        }
        [EntitySystem]
        private static void Destroy(this ET.Server.SkillStatusComponent self)
        {
            self.ResetCurrentSkill();
            self.CoolDowns.Clear();
            self.CoolDownStartTimes.Clear();
        }

        public static int CanCastSkill(this SkillStatusComponent self, int castConfigId)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null)
            {
                return ErrorCode.ERR_CastPreUnitIsNull;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return ErrorCode.ERR_CastPreNumericIsNull;
            }
            //禁止施法状态
            if (numericComponent[NumericType.ForbidSkill] > 0)
            {
                return ErrorCode.ERR_CastPreForbidSkill;
            }
            //判断冷却状态
            if (self.CoolDowns.TryGetValue(castConfigId, out long tarTime))
            {
                long nowTime = TimeInfo.Instance.ServerFrameTime();
                if (nowTime <= tarTime)
                {
                    return ErrorCode.ERR_CastPreCoolDown;
                }
            }

            return ErrorCode.ERR_Success;
        }

        public static void SetCoolDown(this SkillStatusComponent self, int castConfigId, int coolDownMs)
        {
            if (coolDownMs <= 0)
            {
                return;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            self.CoolDowns[castConfigId] = now + coolDownMs;
            self.CoolDownStartTimes[castConfigId] = now;
            CoolDownNoticeHelper.SendCoolDownChange(self.GetParent<Unit>(), castConfigId, self.CoolDowns[castConfigId], now);
        }

        /// <summary>
        /// UnBreakTime>=0 的技能开始释放时，记录当前技能状态。
        /// </summary>
        public static void BeginCurrentSkill(this SkillStatusComponent self, Cast cast)
        {
            if (cast == null || cast.IsDisposed || cast.Config.UnBreakTime < 0)
            {
                return;
            }

            self.CurrentSkillCastInstanceId = cast.Id;
            self.CurrentSkillCastID = cast.ConfigId;
            self.CurrentSkillStartTime = cast.StartTime;
            self.CurrentSkillStatus = SkillStatusType.Running;
        }

        /// <summary>
        /// 技能结束或打断时，清空当前技能状态。
        /// </summary>
        public static void ClearCurrentSkill(this SkillStatusComponent self, Cast cast)
        {
            if (cast == null || cast.IsDisposed || cast.Config.UnBreakTime < 0)
            {
                return;
            }

            if (self.CurrentSkillCastInstanceId != cast.Id)
            {
                return;
            }

            self.ResetCurrentSkill();
        }

        public static void ResetCurrentSkill(this SkillStatusComponent self)
        {
            self.CurrentSkillCastInstanceId = default;
            self.CurrentSkillCastID = default;
            self.CurrentSkillStartTime = default;
            self.CurrentSkillStatus = SkillStatusType.New;
        }
    }
}

