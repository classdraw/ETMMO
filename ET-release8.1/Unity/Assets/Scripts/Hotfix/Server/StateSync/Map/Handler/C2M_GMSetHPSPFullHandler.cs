namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_GMSetHPSPFullHandler : MessageLocationHandler<Unit, C2M_GMSetHPSPFull, M2C_GMSetHPSPFull>
    {
        protected override async ETTask Run(Unit unit, C2M_GMSetHPSPFull request, M2C_GMSetHPSPFull response)
        {
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                response.Error = ErrorCode.ERR_CastPreNumericIsNull;
                return;
            }

            long maxHp = numeric[NumericType.MaxHp];
            if (maxHp > 0)
            {
                numeric[NumericType.HpBase] = maxHp;
            }

            long maxSp = numeric[NumericType.MaxSp];
            if (maxSp > 0)
            {
                numeric[NumericType.SpBase] = maxSp;
            }

            response.Message = "HP/SP 已满";
            await ETTask.CompletedTask;
        }
    }
}