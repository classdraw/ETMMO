using Unity.Mathematics;

namespace ET.Client
{
    [EntitySystemOf(typeof(FollowComponent))]
    [FriendOf(typeof(FollowComponent))]
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
            if (!self.IsReady)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            Unit target = self.Target;
            if (target == null || target.IsDisposed)
            {
                return;
            }

            if (self.FlyTimeMs <= 0)
            {
                return;
            }

            if (self.Speed <= 0f)
            {
                TryStartFollow(self, unit, target);
                return;
            }

            long now = TimeInfo.Instance.ClientFrameTime();
            if (now >= self.EndTime)
            {
                unit.Dispose();
                return;
            }

            if (self.LastUpdateTime <= 0)
            {
                self.LastUpdateTime = now;
                return;
            }

            float deltaTime = (now - self.LastUpdateTime) / 1000f;
            self.LastUpdateTime = now;
            if (deltaTime <= 0f)
            {
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

            float moveDistance = self.Speed * deltaTime;
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

        private static void TryStartFollow(FollowComponent self, Unit unit, Unit target)
        {
            float3 offset = target.Position - unit.Position;
            offset.y = 0;
            float distance = math.max(math.length(offset), 0.01f);
            self.Speed = distance / (self.FlyTimeMs / 1000f);
            self.EndTime = TimeInfo.Instance.ClientFrameTime() + self.FlyTimeMs;
            self.LastUpdateTime = 0;
        }

        [EntitySystem]
        private static void Destroy(this FollowComponent self)
        {
            self.Target = default;
            self.FlyTimeMs = 0;
            self.Speed = 0;
            self.EndTime = 0;
            self.LastUpdateTime = 0;
            self.IsReady = false;
        }
    }
}
