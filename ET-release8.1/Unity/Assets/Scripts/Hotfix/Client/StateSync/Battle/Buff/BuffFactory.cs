namespace ET.Client
{
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    public static class BuffFactory
    {
        public static Buff Create(this BuffComponent buffComponent, BuffProto buffProto, Unit owner)
        {
            if (buffComponent == null || buffComponent.IsDisposed || buffProto == null)
            {
                return null;
            }

            Buff buff = buffComponent.Get(buffProto.Id);
            if (buff != null && !buff.IsDisposed)
            {
                buff.ConfigId = buffProto.ConfigId;
                buff.Owner = owner;
                buff.CreateTime = buffProto.CreateTime;
                buff.ExpireTime = buffProto.ExpireTime;
                return buff;
            }

            buff = buffComponent.AddChildWithId<Buff, int>(buffProto.Id, buffProto.ConfigId);
            buff.Owner = owner;
            buff.CreateTime = buffProto.CreateTime;
            buff.ExpireTime = buffProto.ExpireTime;
            buffComponent.Buffs[buffProto.Id] = buff;
            return buff;
        }
    }
}
