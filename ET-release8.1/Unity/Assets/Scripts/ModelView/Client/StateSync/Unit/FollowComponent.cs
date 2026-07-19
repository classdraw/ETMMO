namespace ET.Client
{
    [ComponentOf(typeof(Unit))]
    public class FollowComponent : Entity, IAwake, IUpdate, IDestroy
    {
        public EntityRef<Unit> Target;
        public float Speed;
        public long EndTime;
    }
}
