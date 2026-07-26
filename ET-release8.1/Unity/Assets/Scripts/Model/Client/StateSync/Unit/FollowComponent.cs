namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class FollowComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public EntityRef<Unit> Target;
        public int FlyTimeMs;
        public float Speed;
        public long EndTime;
        public long LastUpdateTime;
        public bool IsReady;
    }
}
