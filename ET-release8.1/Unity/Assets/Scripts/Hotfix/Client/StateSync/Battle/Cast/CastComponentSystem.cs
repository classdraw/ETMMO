using System.Collections.Generic;

namespace ET.Client
{
    [EntitySystemOf(typeof(CastComponent))]
    [FriendOf(typeof(CastComponent))]
    [FriendOf(typeof(Cast))]
    public static partial class CastComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Client.CastComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ET.Client.CastComponent self)
        {
            self.Clear();
        }

        public static Cast Create(this CastComponent self, long castId, int configId, long casterId, List<long> targetsId)
        {
            Cast cast = self.AddChildWithId<Cast, int>(castId, configId);
            cast.CasterId = casterId;
            cast.TargetsId.Clear();
            if (targetsId != null)
            {
                cast.TargetsId.AddRange(targetsId);
            }

            self.Casts[castId] = cast;
            return cast;
        }

        public static Cast Get(this CastComponent self, long castId)
        {
            self.Casts.TryGetValue(castId, out EntityRef<Cast> castRef);
            return castRef;
        }

        public static void Remove(this CastComponent self, long castId)
        {
            if (!self.Casts.Remove(castId, out EntityRef<Cast> castRef))
            {
                return;
            }

            Cast cast = castRef;
            cast?.Dispose();
        }

        public static void Clear(this CastComponent self)
        {
            foreach (Cast cast in self.Casts.Values)
            {
                cast?.Dispose();
            }

            self.Casts.Clear();
        }
    }
}
