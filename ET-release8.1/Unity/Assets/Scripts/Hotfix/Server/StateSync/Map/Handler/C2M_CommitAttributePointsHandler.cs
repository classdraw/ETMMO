namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_CommitAttributePointsHandler : MessageLocationHandler<Unit, C2M_CommitAttributePoints, M2C_CommitAttributePoints>
    {
        protected override async ETTask Run(Unit unit, C2M_CommitAttributePoints request, M2C_CommitAttributePoints response)
        {
            if (!PlayerAttributeHelper.TryCommitAttributeDeltas(
                    unit,
                    request.DeltaStr,
                    request.DeltaAgi,
                    request.DeltaVit,
                    request.DeltaInt,
                    request.DeltaDex,
                    request.DeltaLuk,
                    out string message))
            {
                response.Error = ErrorCode.ERR_CommitAttributePointsError;
                response.Message = message;
                return;
            }

            response.Message = message;
            await ETTask.CompletedTask;
        }
    }
}