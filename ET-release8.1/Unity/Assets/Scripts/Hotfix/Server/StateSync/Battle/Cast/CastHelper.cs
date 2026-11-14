using System.Collections.Generic;
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

                if (CastSkillKind.IsAttachedSkill(casting))
                {
                    continue;
                }

                int unBreakTime = casting.Config.UnBreakTime;

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
        /// <summary>
        /// 常规输入创建技能
        /// </summary>
        /// <param name="caster"></param>
        /// <param name="castConfigId"></param>
        /// <param name="inputUnitId"></param>
        /// <param name="inputPos"></param>
        /// <param name="needStop"></param>
        /// <param name="checkLearned">玩家 C2M/GM 为 true；子弹、Action、AI 为 false。</param>
        public static int CreateAndCast(this Unit caster,int castConfigId,long inputUnitId,float3 inputPos,bool needStop,List<long> targets = null,bool checkLearned = false)
        {
            SkillStatusComponent skillStatus = caster.GetComponent<SkillStatusComponent>();
            if (skillStatus == null || skillStatus.IsDisposed)
            {
                return ErrorCode.ERR_CastSkillError;
            }

            int err = skillStatus.CanCastSkill(castConfigId, checkLearned);
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }

            if (!CastConfigCategory.Instance.Contain(castConfigId))
            {
                return ErrorCode.ERR_CastArgsError;
            }

            int coolDownMs = CastConfigCategory.Instance.Get(castConfigId).CoolDown;

            Cast cast = caster.Create(castConfigId,inputUnitId, inputPos);
            if (cast==null)
            {
                return ErrorCode.ERR_CastSkillError;
            }

            if (targets != null && targets.Count > 0)
            {
                cast.Targets.AddRange(targets);
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

            // 瞬发技能可能在 CastBeginAsync 内已 CastFinish/Dispose，不可再读 cast.Config
            skillStatus.SetCoolDown(castConfigId, coolDownMs);
            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 玩家/GM 外围施法：入参为 <see cref="SkillConfig"/> Id；内部解析 <see cref="SkillConfig.CastId"/> 后走原有 Cast 流程。
        /// </summary>
        public static int CreateAndCastBySkill(this Unit caster, int skillConfigId, long inputUnitId, float3 inputPos, bool needStop,
            List<long> targets = null, bool checkLearned = false)
        {
            if (!SkillConfigCategory.Instance.Contain(skillConfigId))
            {
                return ErrorCode.ERR_CastArgsError;
            }

            if (!SkillConfigHelper.TryGetCastConfigId(skillConfigId, out int castConfigId))
            {
                return ErrorCode.ERR_CastArgsError;
            }

            SkillStatusComponent skillStatus = caster.GetComponent<SkillStatusComponent>();
            if (skillStatus == null || skillStatus.IsDisposed)
            {
                return ErrorCode.ERR_CastSkillError;
            }

            int err = skillStatus.CanCastSkill(castConfigId, checkLearned);
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }

            SkillConfig skillConfig = SkillConfigCategory.Instance.Get(skillConfigId);
            err = SkillFunctionHelper.ValidateCheckFunc(caster, castConfigId, skillConfig.CheckFunc);
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }

            err = SkillFunctionHelper.ApplyConsumeFunc(caster, castConfigId, skillConfig.ConsumeFunc);
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }

            int coolDownMs = CastConfigCategory.Instance.Get(castConfigId).CoolDown;

            Cast cast = caster.Create(castConfigId, inputUnitId, inputPos);
            if (cast == null)
            {
                return ErrorCode.ERR_CastSkillError;
            }

            if (targets != null && targets.Count > 0)
            {
                cast.Targets.AddRange(targets);
            }

            if (needStop)
            {
                caster.Stop(1);
            }

            err = cast.Cast();
            if (err != ErrorCode.ERR_Success)
            {
                return err;
            }

            skillStatus.SetCoolDown(castConfigId, coolDownMs);
            return ErrorCode.ERR_Success;
        }
    }
}

