namespace ET.Client
{
    [FriendOf(typeof(ClientCastComponent))]
    [FriendOf(typeof(ClientCast))]
    public static class ClientCastFactory
    {
        public static ClientCast CreateAndAddCast(this Unit caster, M2C_CastStart message)
        {
            ClientCastComponent clientCastComponent = caster.GetComponent<ClientCastComponent>();
            if (clientCastComponent == null || clientCastComponent.IsDisposed)
            {
                return null;
            }
            
            ClientCast clientCast = clientCastComponent.AddChildWithId<ClientCast, int>(message.CastId, message.CastConfigId);
            clientCast.CasterId = message.CasterId;
            clientCast.TargetsId.Clear();
            if (message.TargetsId != null)
            {
                clientCast.TargetsId.AddRange(message.TargetsId);
            }

            clientCastComponent.Add(clientCast);
            return clientCast;
        }
    }
}
