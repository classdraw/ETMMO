namespace ET.Client
{
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    public static class BuffFactory
    {
        public static Buff CreateAndAddBuff(this Unit unit, BuffProto buffProto)
        {
            BuffComponent buffComponent = unit.GetComponent<BuffComponent>();
            if (buffComponent == null || buffComponent.IsDisposed || buffProto == null)
            {
                return null;
            }
            
            //这里必须是新增
            Buff buff = buffComponent.AddChildWithId<Buff, int>(buffProto.Id, buffProto.ConfigId);
            buff.Owner = unit;
            buff.CreateTime = buffProto.CreateTime;
            buff.ExpireTime = buffProto.ExpireTime;
            buffComponent.Add(buff);
            return buff;
        }
    }
}
