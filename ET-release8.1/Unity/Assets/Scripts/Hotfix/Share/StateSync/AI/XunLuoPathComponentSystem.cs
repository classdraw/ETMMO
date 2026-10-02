using Unity.Mathematics;

namespace ET
{
    [EntitySystemOf(typeof(XunLuoPathComponent))]
    [FriendOf(typeof(XunLuoPathComponent))]
    public static partial class XunLuoPathComponentSystem
    {
        [EntitySystem]
        private static void Awake(this XunLuoPathComponent self)
        {
        }

        public static float3 GetCurrent(this XunLuoPathComponent self)
        {
            if (self.path == null || self.path.Length == 0)
            {
                return float3.zero;
            }

            if (self.Index < 0 || self.Index >= self.path.Length)
            {
                self.Index = 0;
            }

            return self.path[self.Index];
        }

        public static void MoveNext(this XunLuoPathComponent self)
        {
            self.Index = ++self.Index % self.path.Length;
        }
    }
}
