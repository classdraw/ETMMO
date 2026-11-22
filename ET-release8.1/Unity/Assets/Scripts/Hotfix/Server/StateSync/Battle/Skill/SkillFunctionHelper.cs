namespace ET.Server
{
    /// <summary>
    /// 配置表 Function 调度。
    /// Check/Consume 走 ErrorCode，仅 0 为成功；<see cref="RunFunc(Unit,int,int[],int)"/> 直接返回 Function 的 int。
    /// </summary>
    public static class SkillFunctionHelper
    {
        /// <summary>
        /// 配置表是否填写了 Function：null/空数组或首项≤0 视为未配置（无 CheckFunc、无 ConsumeFunc 的技能）。
        /// </summary>
        public static bool IsFuncConfigured(float[] funcArgs)
        {
            return funcArgs != null && funcArgs.Length > 0 && (int)funcArgs[0] > 0;
        }

        /// <summary>执行 Function 并返回原始 int。keyIndex 为函数编号在 args 中的下标，不做 ErrorCode 判断。</summary>
        /// <param name="castConfigLevel">Buff 等上下文已快照的施法等级；&lt;1 时由 Function 内从 SkillStatus 读取。</param>
        public static int RunFunc(Unit unit, int castId, int[] funcArgs, int keyIndex, int castConfigLevel = 0)
        {
            return Invoke(unit, castId, funcArgs, keyIndex, 0, castConfigLevel);
        }

        private static int RunFunc(Unit unit, int castId, float[] funcArgs, string logTag)
        {
            if (!IsFuncConfigured(funcArgs))
            {
                return ErrorCode.ERR_Success;
            }

            if (unit == null || unit.IsDisposed)
            {
                return ErrorCode.ERR_CastPreUnitIsNull;
            }

            int functionKey = (int)funcArgs[0];
            int err = Invoke(unit, castId, funcArgs, 0, ErrorCode.ERR_CastSkillError);
            if (err != ErrorCode.ERR_Success)
            {
                Log.Console($"[Skill{logTag}] 失败 unitId={unit.Id} castId={castId} func={functionKey} err={err}");
            }

            return err;
        }

        private static int Invoke(Unit unit, int castId, float[] funcArgs, int keyIndex, int fallback)
        {
            if (funcArgs == null || keyIndex < 0 || funcArgs.Length <= keyIndex)
            {
                return fallback;
            }

            if (unit == null || unit.IsDisposed)
            {
                return fallback;
            }

            int functionKey = (int)funcArgs[keyIndex];
            return Evaluate(functionKey, unit, castId, funcArgs, fallback);
        }

        private static int Invoke(Unit unit, int castId, int[] funcArgs, int keyIndex, int fallback, int castConfigLevel = 0)
        {
            if (funcArgs == null || keyIndex < 0 || funcArgs.Length <= keyIndex)
            {
                return fallback;
            }

            if (unit == null || unit.IsDisposed)
            {
                return fallback;
            }

            return Evaluate(funcArgs[keyIndex], unit, castId, funcArgs, fallback, castConfigLevel);
        }

        /// <summary><see cref="SkillConfig.CheckFunc"/>，仅校验不消耗；未配置则直接成功。</summary>
        public static int ValidateCheckFunc(Unit unit, int castId, float[] checkFunc)
        {
            return RunFunc(unit, castId, checkFunc, "CheckFunc");
        }

        /// <summary>仅校验 <see cref="SkillConfig.ConsumeFunc"/> 是否足够，不扣资源。</summary>
        public static int ValidateConsumeFunc(Unit unit, int castId, float[] consumeFunc)
        {
            return InvokeConsumeFunc(unit, castId, consumeFunc, applyConsume: false);
        }

        /// <summary>执行 <see cref="SkillConfig.ConsumeFunc"/>，成功则真实扣资源；未配置则直接成功。</summary>
        public static int ApplyConsumeFunc(Unit unit, int castId, float[] consumeFunc)
        {
            return InvokeConsumeFunc(unit, castId, consumeFunc, applyConsume: true);
        }

        private static int InvokeConsumeFunc(Unit unit, int castId, float[] consumeFunc, bool applyConsume)
        {
            if (!IsFuncConfigured(consumeFunc))
            {
                return ErrorCode.ERR_Success;
            }

            if (unit == null || unit.IsDisposed)
            {
                return ErrorCode.ERR_CastPreUnitIsNull;
            }

            int key = (int)consumeFunc[0];
            int err = EvaluateConsume(key, unit, castId, consumeFunc, applyConsume);
            if (err != ErrorCode.ERR_Success)
            {
                Log.Console($"[SkillConsume] 失败 unitId={unit.Id} castId={castId} func={key} apply={applyConsume} err={err}");
            }

            return err;
        }

        private static int EvaluateConsume(int key, Unit unit, int castId, float[] args, bool applyConsume)
        {
            switch (key)
            {
                case 100:
                    return SkillFunctionCode.Function100(unit, castId, args, applyConsume);
                case 101:
                    return SkillFunctionCode.Function101(unit, castId, args, applyConsume);
                default:
                    return RunFunc(unit, castId, args, "ConsumeFunc");
            }
        }

        /// <summary>castId 为 Cast 配置 Id；返回值即 ErrorCode。</summary>
        public static int Evaluate(int key, Unit unit, int castId, float[] args, int defaultError = ErrorCode.ERR_CastSkillError)
        {
            if (unit == null || unit.IsDisposed)
            {
                Log.Warning($"[SkillFunction] Evaluate Unit 无效: key={key}");
                return defaultError;
            }

            args ??= System.Array.Empty<float>();

            switch (key)
            {
                case 100:
                    return SkillFunctionCode.Function100(unit, castId, args);
                case 101:
                    return SkillFunctionCode.Function101(unit, castId, args);
                case 300:
                    return SkillFunctionCode.Function300(unit, castId, args);
                default:
                    Log.Error($"[SkillFunction] Evaluate 未实现: key={key}");
                    return defaultError;
            }
        }

        /// <summary>直接返回 Function 的 int，args 使用调用方原数组。</summary>
        public static int Evaluate(int key, Unit unit, int castId, int[] args, int defaultError = 0, int castConfigLevel = 0)
        {
            if (unit == null || unit.IsDisposed)
            {
                Log.Warning($"[SkillFunction] Evaluate Unit 无效: key={key}");
                return defaultError;
            }

            args ??= System.Array.Empty<int>();

            switch (key)
            {
                case 500:
                    return SkillFunctionCode.Function500(unit, castId, args, castConfigLevel);
                default:
                    Log.Error($"[SkillFunction] Evaluate 未实现: key={key}");
                    return defaultError;
            }
        }
    }
}