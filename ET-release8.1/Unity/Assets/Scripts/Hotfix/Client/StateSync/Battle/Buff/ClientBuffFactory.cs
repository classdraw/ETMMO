namespace ET.Client
{
    [FriendOf(typeof(ClientBuffComponent))]
    [FriendOf(typeof(ClientBuff))]
    public static class ClientBuffFactory
    {
        public static ClientBuff CreateAndAddBuff(this Unit unit, BuffProto buffProto)
        {
            ClientBuffComponent clientBuffComponent = unit.GetComponent<ClientBuffComponent>();
            if (clientBuffComponent == null || clientBuffComponent.IsDisposed || buffProto == null)
            {
                return null;
            }
            
            //这里必须是新增
            ClientBuff clientBuff = clientBuffComponent.AddChildWithId<ClientBuff, int>(buffProto.Id, buffProto.ConfigId);
            clientBuff.Owner = unit;
            clientBuff.CreateTime = buffProto.CreateTime;
            clientBuff.ExpireTime = buffProto.ExpireTime;
            clientBuffComponent.Add(clientBuff);
            return clientBuff;
        }
    }
}
