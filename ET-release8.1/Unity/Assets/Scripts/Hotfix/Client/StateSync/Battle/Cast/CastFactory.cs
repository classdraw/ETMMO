namespace ET.Client
{
    [FriendOf(typeof(CastComponent))]
    [FriendOf(typeof(Cast))]
    public static class CastFactory
    {
        public static Cast CreateAndAddCast(this Unit caster, M2C_CastStart message)
        {
            CastComponent castComponent = caster.GetComponent<CastComponent>();
            if (castComponent == null || castComponent.IsDisposed)
            {
                return null;
            }
            
            Cast cast = castComponent.AddChildWithId<Cast, int>(message.CastId, message.CastConfigId);
            cast.CasterId = message.CasterId;
            cast.TargetsId.Clear();
            if (message.TargetsId != null)
            {
                cast.TargetsId.AddRange(message.TargetsId);
            }

            castComponent.Add(cast);
            return cast;
        }
    }
}
