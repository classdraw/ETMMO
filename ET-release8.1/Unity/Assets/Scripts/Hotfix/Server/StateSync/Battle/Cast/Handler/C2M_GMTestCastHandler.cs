namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_GMTestCastHandler: MessageLocationHandler<Unit, C2M_GMTestCast, M2C_GMTestCast>
    {
        protected override async ETTask Run(Unit unit, C2M_GMTestCast request, M2C_GMTestCast response)
        {
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