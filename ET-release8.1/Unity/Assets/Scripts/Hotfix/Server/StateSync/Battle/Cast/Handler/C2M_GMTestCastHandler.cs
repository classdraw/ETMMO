namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_GMTestCastHandler: MessageLocationHandler<Unit, C2M_GMTestCast, M2C_GMTestCast>
    {
        protected override async ETTask Run(Unit unit, C2M_GMTestCast request, M2C_GMTestCast response)
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

            unit.Stop(1);
            response.Error=unit.CreateAndCast(request.CastConfigId,request.TargetId,request.InputPos);

            await ETTask.CompletedTask;
        }
    }
}