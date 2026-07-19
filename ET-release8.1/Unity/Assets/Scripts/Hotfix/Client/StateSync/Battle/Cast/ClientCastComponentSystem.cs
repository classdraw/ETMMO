namespace ET.Client
{
    [EntitySystemOf(typeof(ClientCastComponent))]
    [FriendOf(typeof(ClientCastComponent))]
    [FriendOf(typeof(ClientCast))]
    public static partial class ClientCastComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.ClientCastComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.ClientCastComponent self)
        {
            self.Clear();
        }

        public static void Add(this ClientCastComponent self,ClientCast clientCast)
        {
            self.Casts.TryAdd(clientCast.Id, clientCast);
        }

        public static ClientCast Get(this ClientCastComponent self, long castId)
        {
            if (self.Casts.TryGetValue(castId, out EntityRef<ClientCast> castRef))
            {
                return castRef;
            }
            return null;
        }

        public static void Remove(this ClientCastComponent self, long castId)
        {
            if (!self.Casts.Remove(castId, out EntityRef<ClientCast> castRef))
            {
                return;
            }

            ClientCast clientCast = castRef;
            clientCast?.Dispose();
        }
        
        
        public static void Clear(this ClientCastComponent self)
        {
            foreach (ClientCast cast in self.Casts.Values)
            {
                cast?.Dispose();
            }

            self.Casts.Clear();
        }

        public static bool IsCasting(this ClientCastComponent self)
        {
            if (self == null || self.IsDisposed)
            {
                return false;
            }

            foreach (ClientCast cast in self.Casts.Values)
            {
                if (cast != null && !cast.IsDisposed)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsCasting(this Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return false;
            }

            return unit.GetComponent<ClientCastComponent>()?.IsCasting() ?? false;
        }
    }
}
