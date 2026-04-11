using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 创角协议 Parts：key 为 <see cref="AvatarPartType"/> 整型，value 为 AvatarConfig Id；
    /// 与 <see cref="RoleInfo"/> / <see cref="RoleAvatarIds"/> 互转。
    /// </summary>
    public static class RoleAvatarParts
    {
        public static void MergeRoleAvatarIdsIntoParts(RoleAvatarIds ids, Dictionary<int, int> parts)
        {
            if (parts == null)
            {
                return;
            }

            Put(parts, AvatarPartType.Armor_Left, ids.ArmorLeft);
            Put(parts, AvatarPartType.Armor_Right, ids.ArmorRight);
            Put(parts, AvatarPartType.Armor_Body, ids.ArmorBody);
            Put(parts, AvatarPartType.Body, ids.Body);
            Put(parts, AvatarPartType.Body_Arm_Left, ids.BodyArmLeft);
            Put(parts, AvatarPartType.Body_Arm_Right, ids.BodyArmRight);
            Put(parts, AvatarPartType.Foot_Left, ids.FootLeft);
            Put(parts, AvatarPartType.Foot_Right, ids.FootRight);
            Put(parts, AvatarPartType.Head, ids.Head);
            Put(parts, AvatarPartType.Hair, ids.Hair);

            if (ids.EyeFront != 0)
            {
                int v = ids.EyeFront;
                parts[(int)AvatarPartType.Eye_Front_Left] = v;
                parts[(int)AvatarPartType.Eye_Front_Right] = v;
            }

            if (ids.EyeBack != 0)
            {
                int v = ids.EyeBack;
                parts[(int)AvatarPartType.Eye_Back_Left] = v;
                parts[(int)AvatarPartType.Eye_Back_Right] = v;
            }
        }

        public static void ApplyPartsToRoleInfo(Dictionary<int, int> parts, RoleInfo roleInfo)
        {
            if (parts == null || roleInfo == null)
            {
                return;
            }

            roleInfo.ArmorLeft = Get(parts, AvatarPartType.Armor_Left);
            roleInfo.ArmorRight = Get(parts, AvatarPartType.Armor_Right);
            roleInfo.ArmorBody = Get(parts, AvatarPartType.Armor_Body);
            roleInfo.Body = Get(parts, AvatarPartType.Body);
            roleInfo.BodyArmLeft = Get(parts, AvatarPartType.Body_Arm_Left);
            roleInfo.BodyArmRight = Get(parts, AvatarPartType.Body_Arm_Right);
            roleInfo.FootLeft = Get(parts, AvatarPartType.Foot_Left);
            roleInfo.FootRight = Get(parts, AvatarPartType.Foot_Right);
            roleInfo.Head = Get(parts, AvatarPartType.Head);
            roleInfo.Hair = Get(parts, AvatarPartType.Hair);

            int ef = Get(parts, AvatarPartType.Eye_Front_Left);
            if (ef == 0)
            {
                ef = Get(parts, AvatarPartType.Eye_Front_Right);
            }

            roleInfo.EyeFront = ef;

            int eb = Get(parts, AvatarPartType.Eye_Back_Left);
            if (eb == 0)
            {
                eb = Get(parts, AvatarPartType.Eye_Back_Right);
            }

            roleInfo.EyeBack = eb;
        }

        private static void Put(Dictionary<int, int> parts, AvatarPartType partType, int configId)
        {
            if (configId != 0)
            {
                parts[(int)partType] = configId;
            }
        }

        private static int Get(Dictionary<int, int> parts, AvatarPartType partType)
        {
            return parts.TryGetValue((int)partType, out int v) ? v : 0;
        }
    }
}
