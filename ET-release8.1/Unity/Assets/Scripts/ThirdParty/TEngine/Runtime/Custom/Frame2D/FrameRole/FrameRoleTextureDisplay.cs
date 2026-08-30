using System;

namespace ET
{
    /// <summary>
    /// ScriptableObject 层 display 编解码（纯 int，不依赖 Model 枚举）。
    /// 逻辑层请使用 Model.Share 中的 FrameRoleTextureId。
    /// </summary>
    internal static class FrameRoleTextureDisplay
    {
        public const int PartMul = 10000000;
        public const int RaceMul = 100000;
        public const int GenderMul = 1000;

        public static int Encode(int partKey, int raceKey, int genderKey, int index)
        {
            return partKey * PartMul + raceKey * RaceMul + genderKey * GenderMul + index;
        }

        public static bool TryDecode(int displayId, out int partKey, out int raceKey, out int genderKey, out int index)
        {
            partKey = 0;
            raceKey = 0;
            genderKey = 0;
            index = 0;
            if (displayId < 0)
            {
                return false;
            }

            partKey = displayId / PartMul;
            int rem = displayId - partKey * PartMul;
            raceKey = rem / RaceMul;
            rem -= raceKey * RaceMul;
            genderKey = rem / GenderMul;
            index = rem - genderKey * GenderMul;
            return true;
        }
    }
}
