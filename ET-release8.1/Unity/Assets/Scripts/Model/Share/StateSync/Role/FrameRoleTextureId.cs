using System;

namespace ET
{
    /// <summary>
    /// display = partKey * 10000000 + raceKey * 100000 + genderKey * 1000 + index。
    /// Excel 只配这个数字；枚举 overload 便于逻辑层使用，ScriptableObject 侧填 int 即可。
    /// </summary>
    public static class FrameRoleTextureId
    {
        public const int PartMul = 10000000;
        public const int RaceMul = 100000;
        public const int GenderMul = 1000;

        public const int MaxPartKey = 214;
        public const int MaxRaceKey = 99;
        public const int MaxGenderKey = 99;
        public const int MaxIndex = 999;

        public static int Encode(int partKey, int raceKey, int genderKey, int index)
        {
            Validate(partKey, raceKey, genderKey, index);
            return partKey * PartMul + raceKey * RaceMul + genderKey * GenderMul + index;
        }

        public static int Encode(FrameRolePartType part, FrameRoleRaceType race, FrameRoleGenderType gender, int index)
        {
            return Encode((int)part, (int)race, (int)gender, index);
        }

        public static void Decode(int displayId, out int partKey, out int raceKey, out int genderKey, out int index)
        {
            if (displayId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(displayId), displayId, "display 不能为负数");
            }

            partKey = displayId / PartMul;
            int rem = displayId - partKey * PartMul;
            raceKey = rem / RaceMul;
            rem -= raceKey * RaceMul;
            genderKey = rem / GenderMul;
            index = rem - genderKey * GenderMul;
        }

        public static void Decode(int displayId, out FrameRolePartType part, out FrameRoleRaceType race, out FrameRoleGenderType gender, out int index)
        {
            Decode(displayId, out int partKey, out int raceKey, out int genderKey, out index);
            part = (FrameRolePartType)partKey;
            race = (FrameRoleRaceType)raceKey;
            gender = (FrameRoleGenderType)genderKey;
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

            Decode(displayId, out partKey, out raceKey, out genderKey, out index);
            return partKey <= MaxPartKey && raceKey <= MaxRaceKey && genderKey <= MaxGenderKey && index <= MaxIndex;
        }

        public static void Validate(int partKey, int raceKey, int genderKey, int index)
        {
            if (partKey < 0 || partKey > MaxPartKey)
            {
                throw new ArgumentOutOfRangeException(nameof(partKey), partKey, $"部位 key 须在 0~{MaxPartKey}");
            }

            if (raceKey < 0 || raceKey > MaxRaceKey)
            {
                throw new ArgumentOutOfRangeException(nameof(raceKey), raceKey, $"种族 key 须在 0~{MaxRaceKey}");
            }

            if (genderKey < 0 || genderKey > MaxGenderKey)
            {
                throw new ArgumentOutOfRangeException(nameof(genderKey), genderKey, $"性别 key 须在 0~{MaxGenderKey}");
            }

            if (index < 0 || index > MaxIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, $"序号须在 0~{MaxIndex}");
            }
        }
    }
}
