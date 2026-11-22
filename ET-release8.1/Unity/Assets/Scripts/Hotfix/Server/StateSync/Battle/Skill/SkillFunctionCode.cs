using System;

namespace ET.Server
{
    /// <summary>
    /// 各 Function 实现。castId 为 Cast 配置 Id，L 为该施法的技能等级。
    /// 100、101、300 返回 <see cref="ErrorCode"/>；500 返回伤害数值。
    /// </summary>
    public static class SkillFunctionCode
    {
        #region 施法消耗100开始
        /// <summary>
        /// 消耗：args[1]+L 点血；长度≥4 时再扣 args[2]+args[3]*L 蓝。先校验血蓝均够，再一并扣除；任一不够则不扣并返回对应 ErrorCode。
        /// SkillConfig的1的ConsumeFunc
        /// </summary>
        public static int Function100(Unit unit, int castId, float[] args, bool applyConsume = true)
        {
            if (args == null || args.Length < 2)
            {
                Log.Warning($"[SkillFunction] Function100 参数不足 castId={castId}");
                return ErrorCode.ERR_CastArgsError;
            }

            SkillStatusComponent skillStatus = unit.GetComponent<SkillStatusComponent>();
            if (skillStatus == null || skillStatus.IsDisposed)
            {
                return ErrorCode.ERR_CastSkillError;
            }

            int level = skillStatus.GetSkillLevel(castId);
            int hpCost = (int)(args[1] + level);
            int spCost = 0;
            bool consumeSp = args.Length >= 4;
            if (consumeSp)
            {
                spCost = (int)(args[2] + args[3] * level);
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return ErrorCode.ERR_CastPreNumericIsNull;
            }

            long curHp = numeric[NumericType.Hp];
            long curSp = numeric[NumericType.Sp];
            long maxHp = numeric[NumericType.MaxHp];
            long maxSp = numeric[NumericType.MaxSp];

            // 先判断：血、蓝（若配置）都必须满足，否则不做任何扣除
            if (hpCost > 0 && curHp < hpCost)
            {
                return ErrorCode.ERR_CastPreHpNotEnough;
            }

            if (consumeSp && spCost > 0 && curSp < spCost)
            {
                return ErrorCode.ERR_CastPreSpNotEnough;
            }

            if (!applyConsume || (hpCost <= 0 && (!consumeSp || spCost <= 0)))
            {
                return ErrorCode.ERR_Success;
            }

            if (hpCost > 0)
            {
                numeric[NumericType.HpBase] = Math.Clamp(curHp - hpCost, 0, maxHp);
            }

            if (consumeSp && spCost > 0)
            {
                numeric[NumericType.SpBase] = Math.Clamp(curSp - spCost, 0, maxSp);
            }

            return ErrorCode.ERR_Success;
        }

        /// <summary>
        /// 消耗：args[1]+args[2]×L 点蓝（Sp）。先校验蓝量，不足则返回 <see cref="ErrorCode.ERR_CastPreSpNotEnough"/> 且不扣；足够则扣除。
        /// 配置示例 <c>[101,8,2]</c> 表示消耗 8+2L 蓝量。
        /// 技能未配 ConsumeFunc（空表或首项≤0）时由 <see cref="SkillFunctionHelper"/> 跳过，不会进入本方法。
        /// SkillConfig 的 ConsumeFunc
        /// </summary>
        public static int Function101(Unit unit, int castId, float[] args, bool applyConsume = true)
        {
            if (args == null || args.Length < 3)
            {
                Log.Warning($"[SkillFunction] Function101 参数不足 castId={castId}");
                return ErrorCode.ERR_CastArgsError;
            }

            SkillStatusComponent skillStatus = unit.GetComponent<SkillStatusComponent>();
            if (skillStatus == null || skillStatus.IsDisposed)
            {
                return ErrorCode.ERR_CastSkillError;
            }

            int level = skillStatus.GetSkillLevel(castId);
            int spCost = (int)(args[1] + args[2] * level);

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return ErrorCode.ERR_CastPreNumericIsNull;
            }

            if (spCost <= 0)
            {
                return ErrorCode.ERR_Success;
            }

            long curSp = numeric[NumericType.Sp];
            long maxSp = numeric[NumericType.MaxSp];

            if (curSp < spCost)
            {
                Log.Console(
                    $"[Skill] 蓝量不足 castId={castId} level={level} need={spCost} cur={curSp} (Function101 仅校验，未扣蓝)");
                return ErrorCode.ERR_CastPreSpNotEnough;
            }

            if (!applyConsume)
            {
                return ErrorCode.ERR_Success;
            }

            numeric[NumericType.SpBase] = Math.Clamp(curSp - spCost, 0, maxSp);
            return ErrorCode.ERR_Success;
        }

        #endregion
        

        #region 施法条件300开始
        /// <summary>
        /// 血量百分比须大于等于 args[1]（如 20 表示 ≥20%）。
        /// SkillConfig的1的CheckFunc
        /// </summary>
        public static int Function300(Unit unit, int castId, float[] args)
        {
            if (args == null || args.Length < 2)
            {
                Log.Warning($"[SkillFunction] Function300 参数不足 castId={castId}");
                return ErrorCode.ERR_CastArgsError;
            }

            float minPercent = args[1];
            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return ErrorCode.ERR_CastPreNumericIsNull;
            }

            long hp = numeric[NumericType.Hp];
            long maxHp = numeric[NumericType.MaxHp];
            if (maxHp <= 0)
            {
                return ErrorCode.ERR_CastPreCheckFunc;
            }

            float hpPercent = hp * 100f / maxHp;
            return hpPercent >= minPercent ? ErrorCode.ERR_Success : ErrorCode.ERR_CastPreCheckFunc;
        }
        

        #endregion

        #region Cast里面用到500开始

        /// <summary>
        /// 返回 Atk × (args[3] + args[4] × L)%。
        /// ActionsConfig的77001的ActionsParam
        /// ActionsConfig的77002的ActionsParam
        /// </summary>
        public static int Function500(Unit unit, int castId, int[] args, int castConfigLevel = 0)
        {
            if (args == null || args.Length < 5)
            {
                Log.Warning($"[SkillFunction] Function500 参数不足 castId={castId}");
                return 0;
            }

            NumericComponent numeric = unit.GetComponent<NumericComponent>();
            if (numeric == null)
            {
                return 0;
            }

            int level;
            if (castConfigLevel >= SkillStatusComponentSystem.DefaultSkillLevel)
            {
                level = castConfigLevel;
            }
            else
            {
                SkillStatusComponent skillStatus = unit.GetComponent<SkillStatusComponent>();
                if (skillStatus == null || skillStatus.IsDisposed)
                {
                    return 0;
                }

                level = skillStatus.GetSkillLevel(castId);
            }
            int atk = numeric.GetAsInt(NumericType.Atk);
            int percent = args[3] + args[4] * level;
            return (int)(atk * (percent / 100d)) * -1;
        }


        #endregion
        
        
        
        
    }
}