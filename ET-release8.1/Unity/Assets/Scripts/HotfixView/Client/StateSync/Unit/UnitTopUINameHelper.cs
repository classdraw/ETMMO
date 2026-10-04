namespace ET.Client
{
    public static class UnitTopUINameHelper
    {
        public static string GetDisplayName(Unit unit)
        {
            if (ClientViewDebugSettings.ReplaceUnitNameWithFaction)
            {
                return CampHelper.GetFactionId(unit).ToString();
            }

            if (string.IsNullOrEmpty(unit.Name))
            {
                return unit.Id.ToString();
            }

            return unit.Name;
        }
    }
}
