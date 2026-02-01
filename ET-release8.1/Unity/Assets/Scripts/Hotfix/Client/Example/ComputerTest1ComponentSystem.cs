namespace ET.Client
{
    [EntitySystemOf(typeof(ComputerTest1Component))]
    [FriendOf(typeof(ComputerTest1Component))]
    public static partial class ComputerTest1ComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ComputerTest1Component self)
        {
            Log.Console("hello world!!!");
        }
    }
}