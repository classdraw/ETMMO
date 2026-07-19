using Unity.Mathematics;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(FollowComponent))]
    [FriendOf(typeof(FollowComponent))]
    [FriendOf(typeof(GameObjectComponent))]
    public static partial class FollowComponentSystem
    {
        private const float ArriveDistance = 0.05f;

        [EntitySystem]
        private static void Awake(this FollowComponent self)
        {
        }

        [EntitySystem]
        private static void Update(this FollowComponent self)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            long now = TimeInfo.Instance.ClientFrameTime();
            if (now >= self.EndTime)
            {
                unit.Dispose();
                return;
            }

            Unit target = self.Target;
            if (target == null || target.IsDisposed)
            {
                unit.Dispose();
                return;
            }

            float3 currentPos = unit.Position;
            float3 targetPos = target.Position;
            float3 offset = targetPos - currentPos;
            offset.y = 0;
            float distance = math.length(offset);
            if (distance <= ArriveDistance)
            {
                unit.Position = targetPos;
                unit.Dispose();
                return;
            }

            float moveDistance = self.Speed * Time.deltaTime;
            if (moveDistance >= distance)
            {
                unit.Position = targetPos;
                unit.Dispose();
                return;
            }

            float3 direction = offset / distance;
            unit.Position = currentPos + direction * moveDistance;
            unit.Forward = direction;
        }

        [EntitySystem]
        private static void Destroy(this FollowComponent self)
        {
            self.Target = default;
            self.Speed = 0;
            self.EndTime = 0;
        }
    }
}
