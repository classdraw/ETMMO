namespace ET.Server
{
    [EntitySystemOf(typeof(Player))]
    [FriendOf(typeof(Player))]
    public static partial class PlayerSystem
    {
        [EntitySystem]
        private static void Awake(this Player self, string a,int b)
        {
            self.AccountName = a;
            self.BaseAvatar = b;
        }
    }
}