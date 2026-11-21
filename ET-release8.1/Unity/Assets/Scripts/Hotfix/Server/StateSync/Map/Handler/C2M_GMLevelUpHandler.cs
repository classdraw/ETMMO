namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class C2M_GMLevelUpHandler : MessageLocationHandler<Unit, C2M_GMLevelUp, M2C_GMLevelUp>
    {
        protected override async ETTask Run(Unit unit, C2M_GMLevelUp request, M2C_GMLevelUp response)
        {
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                response.Error = ErrorCode.ERR_GMLevelUpError;
                response.Message = "NumericComponent 为空";
                return;
            }

            int levelBefore = numeric.GetAsInt(NumericType.Level);
            int gained = PlayerLevelHelper.GmLevelUpOnce(unit);
            if (gained <= 0)
            {
                response.Error = ErrorCode.ERR_GMLevelUpError;
                response.Message = levelBefore >= RoClassicLevelHelper.MaxBaseLevel ? "已达等级上限" : "升级失败";
                return;
            }

            int newLevel = numeric.GetAsInt(NumericType.Level);
            response.NewLevel = newLevel;
            response.Message =
                $"升至 {newLevel} 级；可用属性点={numeric.GetAsInt(NumericType.RemainingAttributePoints)}，可用技能点={numeric.GetAsInt(NumericType.RemainingSkillPoints)}";
            await ETTask.CompletedTask;
        }
    }
}