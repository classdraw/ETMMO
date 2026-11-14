namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_CastInputHandler: MessageLocationHandler<Unit, C2M_CastInput, M2C_CastInput>
    {
        protected override async ETTask Run(Unit unit, C2M_CastInput request, M2C_CastInput response)
        {
            // 协议字段仍为 CastConfigId，语义为 SkillConfig.Id
            int skillConfigId = request.CastConfigId;
            if (!SkillConfigCategory.Instance.Contain(skillConfigId))
            {
                response.Error = ErrorCode.ERR_CastArgsError;
                return;
            }

            int breakErr = unit.TryBreakCastingBeforeCast();
            if (breakErr != ErrorCode.ERR_Success)
            {
                response.Error = breakErr;
                return;
            }

            response.Error = unit.CreateAndCastBySkill(skillConfigId, request.TargetId, request.InputPos, true, null, true);
            await ETTask.CompletedTask;
        }
    }
}