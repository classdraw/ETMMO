namespace ET.Client
{
    [EntitySystemOf(typeof(ClientBuffComponent))]
    [FriendOf(typeof(ClientBuffComponent))]
    [FriendOf(typeof(ClientBuff))]
    public static partial class ClientBuffComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.ClientBuffComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.ClientBuffComponent self)
        {
            self.Clear();
        }

        public static void Add(this ClientBuffComponent self,ClientBuff clientBuff)
        {
            self.Buffs.TryAdd(clientBuff.Id,clientBuff);
        }

        public static ClientBuff Get(this ClientBuffComponent self, long buffId)
        {
            if (self.Buffs.TryGetValue(buffId, out EntityRef<ClientBuff> buffRef))
            {
                return buffRef;
            }

            return null;
        }

        public static void Remove(this ClientBuffComponent self, long buffId)
        {
            if (!self.Buffs.Remove(buffId, out EntityRef<ClientBuff> buffRef))
            {
                return;
            }

            ClientBuff clientBuff = buffRef;
            clientBuff?.Dispose();
        }

        public static void Update(this ClientBuffComponent self,BuffProto buffProto)
        {
            ClientBuff clientBuff = self.Get(buffProto.Id);
            if (clientBuff==null)
            {
                return;
            }
            clientBuff.CreateTime = buffProto.CreateTime;
            clientBuff.ExpireTime = buffProto.ExpireTime;
        }

        public static void Clear(this ClientBuffComponent self)
        {
            foreach (ClientBuff buff in self.Buffs.Values)
            {
                buff?.Dispose();
            }

            self.Buffs.Clear();
        }
    }
}
