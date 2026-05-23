namespace ET.Server
{
    [EntitySystemOf(typeof(MailUnitsComponent))]
    public static partial class MailUnitsComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.MailUnitsComponent self)
        {

        }
        [EntitySystem]
        private static void Destroy(this ET.Server.MailUnitsComponent self)
        {

        }
    }
}