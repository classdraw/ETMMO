namespace ET.Server
{
    [EntitySystemOf(typeof(RobotCase))]
    [FriendOf(typeof(RobotCase))]
    [FriendOf(typeof(RobotCaseComponent))]
    public static partial class RobotCaseSystem
    {
        [EntitySystem]
        private static void Awake(this RobotCase self)
        {
        }

        [EntitySystem]
        private static void Destroy(this RobotCase self)
        {
            foreach (long fiberId in self.Scenes)
            {
                FiberManager.Instance.Remove((int)fiberId).Coroutine();
            }

            self.Scenes.Clear();
        }
    }
}
