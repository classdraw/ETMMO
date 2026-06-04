namespace ET.Server
{
    [FriendOf(typeof(Cast))]
    public static class CastHelper
    {
        public static Cast Create(this Unit caster,int configId)
        {
            var castComponent = caster.GetComponent<CastComponent>();
            if (castComponent==null)
            {
                return null;
            }

            Cast cast = castComponent.Create(configId);
            cast.Caster = caster;


            return cast;
        }

        public static int CreateAndCast(this Unit caster,int configId)
        {
            Cast cast = caster.Create(configId);
            if (cast==null)
            {
                return ErrorCode.ERR_CastSkillError;//
            }

            return cast.Cast();
        }
    }
}

