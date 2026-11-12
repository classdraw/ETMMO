namespace ET
{
    [EntitySystemOf(typeof(MonsterPatrolComponent))]
    [FriendOf(typeof(MonsterPatrolComponent))]
    public static partial class MonsterPatrolComponentSystem
    {
        [EntitySystem]
        private static void Awake(this MonsterPatrolComponent self)
        {
        }
    }
}
