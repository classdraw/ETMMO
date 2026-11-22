namespace ET
{
    public static class UnitHelper
    {
        public static bool IsForbidSkill(this Unit unit) => UnitControlHelper.IsForbidSkill(unit);

        public static bool IsForbidMove(this Unit unit) => UnitControlHelper.IsForbidMove(unit);

        public static bool IsForbidRotation(this Unit unit) => UnitControlHelper.IsForbidRotation(unit);
    }
}