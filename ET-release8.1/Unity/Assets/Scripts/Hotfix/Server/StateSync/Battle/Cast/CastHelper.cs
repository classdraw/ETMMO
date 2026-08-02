using Unity.Mathematics;

namespace ET.Server
{
    [FriendOf(typeof(Cast))]
    [FriendOf(typeof(CastComponent))]
    [FriendOf(typeof(SkillStatusComponent))]
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
        /// 新技能释放前处理当前施法：UnBreakTime==-1 的技能不参与打断且不会被打断；
        /// UnBreakTime>=0 的技能需判断不可打断时间，可打断则发送 M2C_CastBreak。
        /// </summary>
        public static int TryBreakCastingBeforeCast(this Unit unit)
        {
            CastComponent castComponent = unit.GetComponent<CastComponent>();
            if (castComponent == null || castComponent.IsDisposed)
            {
                return ErrorCode.ERR_Success;
            }

            long now = TimeInfo.Instance.ServerFrameTime();
            using ListComponent<Cast> breakList = ListComponent<Cast>.Create();

            foreach (Entity entity in castComponent.Children.Values)
            {
                if (entity is not Cast casting || casting.IsDisposed)
                {
                    continue;
                }

                int unBreakTime = casting.Config.UnBreakTime;
                if (unBreakTime == -1)
                {
                    continue;
                }

                if (casting.StartTime <= 0)
                {
                    Log.Console($"[Cast] 玩家 {unit.Id} 技能 {casting.Id}({casting.ConfigId}) 不可打断，技能尚未开始");
                    return ErrorCode.ERR_CastCasting;
                }

                long unBreakEndTime = casting.StartTime + unBreakTime;
                if (now < unBreakEndTime)
                {
                    Log.Console($"[Cast] 玩家 {unit.Id} 技能 {casting.Id}({casting.ConfigId}) 不可打断，剩余 {unBreakEndTime - now}ms");
                    return ErrorCode.ERR_CastCasting;
                }

                breakList.Add(casting);
            }

            foreach (Cast casting in breakList)
            {
                Log.Console($"[Cast] 玩家 {unit.Id} 打断技能 {casting.Id}({casting.ConfigId})，已施法 {now - casting.StartTime}ms，UnBreakTime={casting.Config.UnBreakTime}ms");
                casting.CastBreak();
            }

            return ErrorCode.ERR_Success;
        }

        public static int CreateAndCast(this Unit caster,int castConfigId,long inputUnitId,float3 inputPos,bool needStop)//这里可能传入前端选择的目标或者坐标
        {
            SkillStatusComponent skillStatusComponent = caster.GetComponent<SkillStatusComponent>();
            if (skillStatusComponent == null || skillStatusComponent.IsDisposed)
            {
                return ErrorCode.ERR_CastSkillError;
            }

            int err = skillStatusComponent.CanCastSkill(castConfigId);
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }

            Cast cast = caster.Create(castConfigId,inputUnitId, inputPos);
            if (cast==null)
            {
                return ErrorCode.ERR_CastSkillError;
            }
            //需求就是开始释放前停止移动，不会施法动画位移会受到stop影响！！！AI别乱改
            if (needStop)
            {
                caster.Stop(1);
            }
            err = cast.Cast();
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }



            skillStatusComponent.SetCoolDown(castConfigId, cast.Config.CoolDown);
            return ErrorCode.ERR_Success;
        }
    }
}

