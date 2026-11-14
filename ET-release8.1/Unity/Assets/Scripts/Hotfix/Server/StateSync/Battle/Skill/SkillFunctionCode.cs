using System;

namespace ET.Server
{
    /// <summary>
    /// 各 Function 实现。返回值为 <see cref="ErrorCode"/>，仅 <see cref="ErrorCode.ERR_Success"/> 表示成功。
    /// 签名 <c>int Function{编号}(Unit unit, int castId, float[] args)</c>，castId 由外部传入。
    /// </summary>
    public static class SkillFunctionCode
    {
        /// <summary>
        /// 血誓斩消耗方法
        /// 消耗：args[1]+L 点血；长度≥4 时再扣 args[2]+args[3]*L 蓝。先校验血蓝均够，再一并扣除；任一不够则不扣并返回对应 ErrorCode。
        /// 配置示例：100,2,4,2
        /// </summary>
        public static int Function100(Unit unit, int castId, float[] args)
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

            if (hpCost <= 0 && (!consumeSp || spCost <= 0))
            {
                return ErrorCode.ERR_Success;
            }

            // 再扣除：仅当上述校验全部通过
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
        
        
        /// <summary>血量百分比须严格大于 args[1]（如 20 表示 &gt;20%）。</summary>
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
    }
}