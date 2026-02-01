namespace ET.Client
{
    [EntitySystemOf(typeof(ComputersComponent))]
    [FriendOf(typeof(ComputersComponent))]
    public static partial class ComputerComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ComputersComponent self)
        {
        }
    }
}