namespace ET.Server
{
    [EntitySystemOf(typeof(MonsterCreateInfo))]
    [FriendOf(typeof(MonsterCreateInfo))]
    public static partial class MonsterCreateInfoSystem
    {
        [EntitySystem]
        private static void Awake(this MonsterCreateInfo self, int monsterConfigId)
        {
            self.MonsterConfigId = monsterConfigId;
        }

        [EntitySystem]
        private static void Destroy(this MonsterCreateInfo self)
        {
            self.MonsterConfigId = 0;
        }
    }
}
