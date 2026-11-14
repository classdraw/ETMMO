namespace ET.Server
{
    /// <summary>
    /// 配置表 Function 调度。
    /// Check/Consume 走 ErrorCode，仅 0 为成功；<see cref="RunFunc(Unit,int,int[],int)"/> 直接返回 Function 的 int。
    /// </summary>
    public static class SkillFunctionHelper
    {
        /// <summary>执行 Function 并返回原始 int。keyIndex 为函数编号在 args 中的下标，不做 ErrorCode 判断。</summary>
        public static int RunFunc(Unit unit, int castId, int[] funcArgs, int keyIndex)
        {
            return Invoke(unit, castId, funcArgs, keyIndex, 0);
        }

        private static int RunFunc(Unit unit, int castId, float[] funcArgs, string logTag)
        {
            if (funcArgs == null || funcArgs.Length == 0)
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

        private static int Invoke(Unit unit, int castId, int[] funcArgs, int keyIndex, int fallback)
        {
            if (funcArgs == null || keyIndex < 0 || funcArgs.Length <= keyIndex)
            {
                return fallback;
            }

            if (unit == null || unit.IsDisposed)
            {
                return fallback;
            }

            return Evaluate(funcArgs[keyIndex], unit, castId, funcArgs, fallback);
        }

        /// <summary><see cref="SkillConfig.CheckFunc"/>，仅校验不消耗。</summary>
        public static int ValidateCheckFunc(Unit unit, int castId, float[] checkFunc)
        {
            return RunFunc(unit, castId, checkFunc, "CheckFunc");
        }

        /// <summary>执行 <see cref="SkillConfig.ConsumeFunc"/>，成功则真实扣资源。</summary>
        public static int ApplyConsumeFunc(Unit unit, int castId, float[] consumeFunc)
        {
            return RunFunc(unit, castId, consumeFunc, "ConsumeFunc");
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
                case 300:
                    return SkillFunctionCode.Function300(unit, castId, args);
                default:
                    Log.Error($"[SkillFunction] Evaluate 未实现: key={key}");
                    return defaultError;
            }
        }

        /// <summary>直接返回 Function 的 int，args 使用调用方原数组。</summary>
        public static int Evaluate(int key, Unit unit, int castId, int[] args, int defaultError = 0)
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
                    return SkillFunctionCode.Function500(unit, castId, args);
                default:
                    Log.Error($"[SkillFunction] Evaluate 未实现: key={key}");
                    return defaultError;
            }
        }
    }
}