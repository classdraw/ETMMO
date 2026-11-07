namespace ET.Client
{
    public static class UnitHelper
    {

        public static Unit GetMyUnitFromCurrentScene(Scene currentScene)
        {
            PlayerComponent playerComponent = currentScene.Root().GetComponent<PlayerComponent>();
            return currentScene.GetComponent<UnitComponent>().Get(playerComponent.MyId);
        }

        public static bool IsMyUnit(this Unit unit, Scene currentScene)
        {
            if (unit == null || unit.IsDisposed || currentScene == null)
            {
                return false;
            }

            PlayerComponent playerComponent = currentScene.Root().GetComponent<PlayerComponent>();
            if (playerComponent == null)
            {
                return false;
            }

            return unit.Id == playerComponent.MyId;
        }

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

        public static float GetSpLv(this Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return 0f;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return 0f;
            }

            long curSp = numericComponent[NumericType.Sp];
            long maxSp = numericComponent[NumericType.MaxSp];
            if (maxSp == 0)
            {
                return 0f;
            }

            return curSp / (float)maxSp;
        }
        
        
    }
}

