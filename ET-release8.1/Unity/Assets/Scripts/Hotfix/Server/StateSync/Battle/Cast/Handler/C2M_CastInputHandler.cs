namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_CastInputHandler: MessageLocationHandler<Unit, C2M_CastInput, M2C_CastInput>
    {
        protected override async ETTask Run(Unit unit, C2M_CastInput request, M2C_CastInput response)
        {
            if (!CastConfigCategory.Instance.Contain(request.CastConfigId))
            {
                response.Error = ErrorCode.ERR_CastArgsError;
                return;
            }

            if (!unit.IsAlive())
            {
                response.Error = ErrorCode.ERR_CastUnitDead;
                return;
            }

            int breakErr = unit.TryBreakCastingBeforeCast();
            if (breakErr != ErrorCode.ERR_Success)
            {
                response.Error = breakErr;
                return;
            }
            
            unit.Stop(1);
            response.Error=unit.CreateAndCast(request.CastConfigId,request.TargetId,request.InputPos);

            await ETTask.CompletedTask;
        }
    }
}
