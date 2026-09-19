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
            if (unit == null || unit.IsDisposed)
            {
                return ErrorCode.ERR_CastPreUnitIsNull;
            }

            if (!unit.IsAlive())
            {
                return ErrorCode.ERR_CastUnitDead;
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

        /// <summary>写入技能 CD 与 CD 开始时间，行为技能与附属技能均生效。</summary>
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
        /// 行为技能（UnBreakTime>=0）开始释放时写入 CurrentSkill；附属技能不写入（CD 见 SetCoolDown，施法时间见 Cast.StartTime）。
        /// </summary>
        public static void BeginCurrentSkill(this SkillStatusComponent self, Cast cast)
        {
            if (!CastSkillKind.IsBehaviorSkill(cast))
            {
                return;
            }

            self.CurrentSkillCastInstanceId = cast.Id;
            self.CurrentSkillCastID = cast.ConfigId;
            self.CurrentSkillStartTime = cast.StartTime;
            self.CurrentSkillStatus = SkillStatusType.Running;
        }

        /// <summary>
        /// 行为技能结束或打断时清空 CurrentSkill；附属技能结束不影响 CurrentSkill。
        /// </summary>
        public static void ClearCurrentSkill(this SkillStatusComponent self, Cast cast)
        {
            if (!CastSkillKind.IsBehaviorSkill(cast))
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

