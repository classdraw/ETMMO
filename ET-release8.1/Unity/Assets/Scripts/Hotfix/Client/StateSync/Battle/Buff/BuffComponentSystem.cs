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

        public static void Add(this BuffComponent self,Buff buff)
        {
            self.Buffs.TryAdd(buff.Id,buff);
        }

        public static Buff Get(this BuffComponent self, long buffId)
        {
            if (self.Buffs.TryGetValue(buffId, out EntityRef<Buff> buffRef))
            {
                return buffRef;
            }

            return null;
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

        public static void Update(this BuffComponent self,BuffProto buffProto)
        {
            Buff buff = self.Get(buffProto.Id);
            if (buff==null)
            {
                return;
            }
            buff.CreateTime = buffProto.CreateTime;
            buff.ExpireTime = buffProto.ExpireTime;
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
