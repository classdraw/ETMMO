namespace ET
{
    [EntitySystemOf(typeof(FactionComponent))]
    [FriendOf(typeof(FactionComponent))]
    public static partial class FactionComponentSystem
    {
        [EntitySystem]
        private static void Awake(this FactionComponent self)
        {
            self.FactionId = (int)CampType.CampA;
        }

        [EntitySystem]
        private static void Awake(this FactionComponent self, int factionId)
        {
            self.FactionId = factionId;
        }
    }
}
