namespace ET.Client
{
    public static class UnitHelper
    {
        public static float GetHpLv(this Unit unit)
        {
            if (unit==null||unit.IsDisposed)
            {
                return 0f;
            }

            var numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent==null)
            {
                return 0f;
            }

            var curHp = numericComponent[NumericType.Hp];
            var maxHp = numericComponent[NumericType.MaxHp];
            if (maxHp==0)
            {
                return 0f;
            }
            return curHp / (float)maxHp;
        }
    }
}

