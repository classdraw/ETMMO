namespace ET.Server
{
    [FriendOf(typeof(Cast))]
    public static class CastHelper
    {
        public static Cast Create(this Unit caster,int castConfigId)
        {
            var castComponent = caster.GetComponent<CastComponent>();
            if (castComponent==null)
            {
                return null;
            }

            Cast cast = castComponent.Create(castConfigId);
            cast.Caster = caster;


            return cast;
        }

        public static int CreateAndCast(this Unit caster,int castConfigId)//这里可能传入前端选择的目标或者坐标
        {
            Cast cast = caster.Create(castConfigId);
            if (cast==null)
            {
                return ErrorCode.ERR_CastSkillError;//
            }

            return cast.Cast();
        }

        public static void SelectTargetsNone(Cast cast)
        {
        }

        public static void SelectTargetsSelf(Cast cast)
        {
        }

        public static void SelectTargetsSingle(Cast cast)
        {
        }

        public static void SelectTargetsSelfFan(Cast cast)
        {
        }

        public static void SelectTargetsSelfRectangle(Cast cast)
        {
        }

        public static void SelectTargetsSelfFanRectangle(Cast cast)
        {
        }

        public static void SelectTargetsDstFan(Cast cast)
        {
        }

        public static void SelectTargetsDstRectangle(Cast cast)
        {
        }

        public static void SelectTargetsDstFanRectangle(Cast cast)
        {
        }

        public static void SelectTargetsPosition(Cast cast)
        {
        }
    }
}

