using System.Collections.Generic;

namespace ET.Client
{
    [FriendOf(typeof(CastComponent))]
    [FriendOf(typeof(Cast))]
    public static class CastFactory
    {
        public static Cast CreateAndAddCast(this Unit caster, long castId, int configId, long casterId, List<long> targetsId)
        {
            CastComponent castComponent = caster.GetComponent<CastComponent>();
            if (castComponent == null || castComponent.IsDisposed)
            {
                return null;
            }

            Cast cast = castComponent.Get(castId);
            if (cast != null && !cast.IsDisposed)
            {
                cast.ConfigId = configId;
                cast.CasterId = casterId;
                cast.TargetsId.Clear();
                if (targetsId != null)
                {
                    cast.TargetsId.AddRange(targetsId);
                }

                return cast;
            }

            cast = castComponent.AddChildWithId<Cast, int>(castId, configId);
            cast.CasterId = casterId;
            cast.TargetsId.Clear();
            if (targetsId != null)
            {
                cast.TargetsId.AddRange(targetsId);
            }

            castComponent.Casts[castId] = cast;
            return cast;
        }
    }
}
