namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_GMAddBaseExpHandler : MessageLocationHandler<Unit, C2M_GMAddBaseExp, M2C_GMAddBaseExp>
    {
        protected override async ETTask Run(Unit unit, C2M_GMAddBaseExp request, M2C_GMAddBaseExp response)
        {
            if (request.Exp <= 0)
            {
                response.Error = ErrorCode.ERR_GMAddBaseExpError;
                response.Message = "经验须大于 0";
                return;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                response.Error = ErrorCode.ERR_GMAddBaseExpError;
                response.Message = "NumericComponent 为空";
                return;
            }

            int levelsGained = PlayerLevelHelper.AddBaseExp(unit, request.Exp);
            response.LevelsGained = levelsGained;
            response.Message =
                $"获得经验 {request.Exp}，升 {levelsGained} 级；当前 Lv={numeric.GetAsInt(NumericType.Level)} EXP={numeric[NumericType.EXP]}";
            await ETTask.CompletedTask;
        }
    }
}