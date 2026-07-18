using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(CastComponent))]
    public static class CastHelper
    {
        public static Cast Create(this Unit caster,int castConfigId,long inputUnitId,float3 inputPos)
        {
            var castComponent = caster.GetComponent<CastComponent>();
            if (castComponent==null)
            {
                return null;
            }
            
            Cast cast = castComponent.Create(castConfigId);
            cast.Caster = caster;
            cast.InputUnitId = inputUnitId;
            cast.InputPos = inputPos;
            return cast;
        }

        /// <summary>
        /// 新技能释放前处理当前施法：不可打断则返回错误，可打断则发送 M2C_CastBreak。
        /// </summary>
        public static int TryBreakCastingBeforeCast(this Unit unit)
        {
            CastComponent castComponent = unit.GetComponent<CastComponent>();
            if (castComponent == null || castComponent.IsDisposed)
            {
                return ErrorCode.ERR_Success;
            }

            Cast casting = castComponent.GetCasting();
            if (casting == null || casting.IsDisposed)
            {
                return ErrorCode.ERR_Success;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            long unBreakEndTime = casting.StartTime + casting.Config.UnBreakTime;
            if (now < unBreakEndTime)
            {
                Log.Console($"[Cast] 玩家 {unit.Id} 技能 {casting.Id}({casting.ConfigId}) 不可打断，剩余 {unBreakEndTime - now}ms");
                return ErrorCode.ERR_CastCasting;
            }

            Log.Console($"[Cast] 玩家 {unit.Id} 打断技能 {casting.Id}({casting.ConfigId})，已施法 {now - casting.StartTime}ms，UnBreakTime={casting.Config.UnBreakTime}ms");
            casting.CastBreak();
            return ErrorCode.ERR_Success;
        }

        public static int CreateAndCast(this Unit caster,int castConfigId,long inputUnitId,float3 inputPos)//这里可能传入前端选择的目标或者坐标
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

