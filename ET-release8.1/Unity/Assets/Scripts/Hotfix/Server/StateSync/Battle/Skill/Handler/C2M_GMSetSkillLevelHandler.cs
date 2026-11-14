namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_GMSetSkillLevelHandler : MessageLocationHandler<Unit, C2M_GMSetSkillLevel, M2C_GMSetSkillLevel>
    {
        protected override async ETTask Run(Unit unit, C2M_GMSetSkillLevel request, M2C_GMSetSkillLevel response)
        {
            SkillStatusComponent skillStatus = unit.GetComponent<SkillStatusComponent>();
            if (skillStatus == null || skillStatus.IsDisposed)
            {
                response.Error = ErrorCode.ERR_GMSetSkillLevelError;
                return;
            }

            // 协议字段为外围 SkillConfig.Id，入库等级按 CastId
            int skillConfigId = request.CastConfigId;
            if (!SkillConfigCategory.Instance.Contain(skillConfigId))
            {
                response.Error = ErrorCode.ERR_CastArgsError;
                return;
            }

            if (!SkillConfigHelper.TryGetCastConfigId(skillConfigId, out int castConfigId))
            {
                response.Error = ErrorCode.ERR_CastArgsError;
                return;
            }

            if (request.Level < 1)
            {
                response.Error = ErrorCode.ERR_GMSetSkillLevelError;
                return;
            }

            skillStatus.SetSkillLevel(castConfigId, request.Level);
            await ETTask.CompletedTask;
        }
    }
}