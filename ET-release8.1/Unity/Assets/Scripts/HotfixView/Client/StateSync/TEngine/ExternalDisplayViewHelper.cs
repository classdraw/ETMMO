namespace ET.Client
{
    public static class ExternalDisplayViewHelper
    {
        public static void ApplyToUnit(Scene scene, Unit unit)
        {
            if (scene == null || unit == null)
            {
                return;
            }

            unit.GetComponent<Avatar2DComponent>()?.Refresh();
        }
    }
}
