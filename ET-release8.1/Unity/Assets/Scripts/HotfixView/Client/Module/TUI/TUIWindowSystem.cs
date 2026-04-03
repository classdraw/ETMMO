namespace ET.Client
{
    [EntitySystemOf(typeof(TUIWindow))]
    [FriendOf(typeof(TUIWindow))]
    public static partial class TUIWindowSystem
    {
        [EntitySystem]
        private static void Awake(this TUIWindow self)
        {
        }
    }
}