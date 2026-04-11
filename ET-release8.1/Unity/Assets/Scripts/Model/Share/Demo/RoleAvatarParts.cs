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

        /// <summary>从协议/缓存 Parts 转为登录界面用的紧凑结构。</summary>
        public static RoleAvatarIds ToRoleAvatarIds(Dictionary<int, int> parts)
        {
            if (parts == null || parts.Count == 0)
            {
                return default;
            }

            return new RoleAvatarIds
            {
                ArmorLeft = Get(parts, AvatarPartType.Armor_Left),
                ArmorRight = Get(parts, AvatarPartType.Armor_Right),
                ArmorBody = Get(parts, AvatarPartType.Armor_Body),
                Body = Get(parts, AvatarPartType.Body),
                BodyArmLeft = Get(parts, AvatarPartType.Body_Arm_Left),
                BodyArmRight = Get(parts, AvatarPartType.Body_Arm_Right),
                FootLeft = Get(parts, AvatarPartType.Foot_Left),
                FootRight = Get(parts, AvatarPartType.Foot_Right),
                Head = Get(parts, AvatarPartType.Head),
                Hair = Get(parts, AvatarPartType.Hair),
                EyeFront = GetFrontEyeId(parts),
                EyeBack = GetBackEyeId(parts),
            };
        }

        /// <summary>从 <see cref="RoleInfo"/> 写入协议 Parts（与 <see cref="MergeRoleAvatarIdsIntoParts"/> 规则一致）。</summary>
        public static void MergePartsFromRoleInfo(RoleInfo roleInfo, Dictionary<int, int> parts)
        {
            if (roleInfo == null || parts == null)
            {
                return;
            }

            parts.Clear();
            var ids = new RoleAvatarIds
            {
                ArmorLeft = roleInfo.ArmorLeft,
                ArmorRight = roleInfo.ArmorRight,
                ArmorBody = roleInfo.ArmorBody,
                Body = roleInfo.Body,
                BodyArmLeft = roleInfo.BodyArmLeft,
                BodyArmRight = roleInfo.BodyArmRight,
                FootLeft = roleInfo.FootLeft,
                FootRight = roleInfo.FootRight,
                Head = roleInfo.Head,
                EyeFront = roleInfo.EyeFront,
                EyeBack = roleInfo.EyeBack,
                Hair = roleInfo.Hair,
            };
            MergeRoleAvatarIdsIntoParts(ids, parts);
        }

        public static void ApplyPartsToRoleInfo(Dictionary<int, int> parts, RoleInfo roleInfo)
        {
            if (roleInfo == null)
            {
                return;
            }

            if (parts == null || parts.Count == 0)
            {
                roleInfo.ArmorLeft = 0;
                roleInfo.ArmorRight = 0;
                roleInfo.ArmorBody = 0;
                roleInfo.Body = 0;
                roleInfo.BodyArmLeft = 0;
                roleInfo.BodyArmRight = 0;
                roleInfo.FootLeft = 0;
                roleInfo.FootRight = 0;
                roleInfo.Head = 0;
                roleInfo.Hair = 0;
                roleInfo.EyeFront = 0;
                roleInfo.EyeBack = 0;
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

        private static int GetFrontEyeId(Dictionary<int, int> parts)
        {
            int a = Get(parts, AvatarPartType.Eye_Front_Left);
            return a != 0 ? a : Get(parts, AvatarPartType.Eye_Front_Right);
        }

        private static int GetBackEyeId(Dictionary<int, int> parts)
        {
            int a = Get(parts, AvatarPartType.Eye_Back_Left);
            return a != 0 ? a : Get(parts, AvatarPartType.Eye_Back_Right);
        }
    }
}
