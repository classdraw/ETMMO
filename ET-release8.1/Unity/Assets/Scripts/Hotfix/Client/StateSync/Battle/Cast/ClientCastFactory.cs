namespace ET.Client
{
    [FriendOf(typeof(ClientCastComponent))]
    [FriendOf(typeof(ClientCast))]
    [FriendOf(typeof(ClientSkillStatusComponent))]
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

            if (CastConfigCategory.Instance.Contain(message.CastConfigId))
            {
                CastConfig castConfig = CastConfigCategory.Instance.Get(message.CastConfigId);
                if (castConfig.UnBreakTime >= 0)
                {
                    ClientSkillStatusComponent skillStatus = caster.GetComponent<ClientSkillStatusComponent>();
                    if (skillStatus != null && !skillStatus.IsDisposed)
                    {
                        skillStatus.CurrentSkillCastInstanceId = message.CastId;
                    }
                }
            }

            return clientCast;
        }
    }
}
