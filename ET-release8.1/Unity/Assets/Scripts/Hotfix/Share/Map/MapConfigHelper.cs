namespace ET
{
    public static class MapConfigHelper
    {
        public static int GetIdByLogicName(string logicName)
        {
            if (string.IsNullOrEmpty(logicName))
            {
                return 0;
            }

            foreach (MapConfig mapConfig in MapConfigCategory.Instance.GetAll().Values)
            {
                if (mapConfig.LogicName == logicName)
                {
                    return mapConfig.Id;
                }
            }

            return 0;
        }
    }
}
