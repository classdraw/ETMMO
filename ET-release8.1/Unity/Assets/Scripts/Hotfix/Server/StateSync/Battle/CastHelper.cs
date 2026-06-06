using System.Numerics;

namespace ET.Server
{
    [FriendOf(typeof(Cast))]
    public static class CastHelper
    {
        public static Cast Create(this Unit caster,int castConfigId,long inputUnitId,Vector3 inputPos)
        {
            var castComponent = caster.GetComponent<CastComponent>();
            if (castComponent==null)
            {
                return null;
            }
            
            Cast cast = castComponent.Create(castConfigId);
            cast.Caster = caster;
            cast.inputUnitId = inputUnitId;
            cast.inputPos = inputPos;
            return cast;
        }

        public static int CreateAndCast(this Unit caster,int castConfigId,long inputUnitId,Vector3 inputPos)//这里可能传入前端选择的目标或者坐标
        {
            Cast cast = caster.Create(castConfigId,inputUnitId, inputPos);
            if (cast==null)
            {
                return ErrorCode.ERR_CastSkillError;//
            }

            return cast.Cast();
        }
    }
}

