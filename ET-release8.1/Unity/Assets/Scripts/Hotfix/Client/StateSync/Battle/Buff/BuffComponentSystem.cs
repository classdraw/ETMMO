namespace ET.Client
{
    [EntitySystemOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    public static partial class BuffComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.BuffComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.BuffComponent self)
        {
            self.Clear();
        }

        public static Buff Add(this BuffComponent self, BuffProto buffProto, Unit owner)
        {
            if (buffProto == null)
            {
                return null;
            }

            Buff buff = self.AddChildWithId<Buff, int>(buffProto.Id, buffProto.ConfigId);
            buff.Owner = owner;
            buff.CreateTime = buffProto.CreateTime;
            buff.ExpireTime = buffProto.ExpireTime;
            self.Buffs[buffProto.Id] = buff;
            return buff;
        }

        public static Buff Get(this BuffComponent self, long buffId)
        {
            self.Buffs.TryGetValue(buffId, out EntityRef<Buff> buffRef);
            return buffRef;
        }

        public static void Remove(this BuffComponent self, long buffId)
        {
            if (!self.Buffs.Remove(buffId, out EntityRef<Buff> buffRef))
            {
                return;
            }

            Buff buff = buffRef;
            buff?.Dispose();
        }

        public static void Clear(this BuffComponent self)
        {
            foreach (Buff buff in self.Buffs.Values)
            {
                buff?.Dispose();
            }

            self.Buffs.Clear();
        }
    }
}
