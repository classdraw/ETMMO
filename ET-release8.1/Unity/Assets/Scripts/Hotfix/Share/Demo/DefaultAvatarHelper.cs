using System.Collections.Generic;

namespace ET
{
    public static class DefaultAvatarHelper
    {
        /// <summary>
        /// 9001~9003、9004~9009 各组共用一次随机下标；前/后眼各只存一份 Id（9010、9011 分别随机）；9012 单独随机。
        /// </summary>
        public static RoleAvatarIds RollRandomDefault()
        {
            const int fallback = 0;
            int[] buf = new int[6];
            RoleAvatarIds ids = default;

            if (TryPickSyncedAvatarIds(new[] { 9001, 9002, 9003 }, buf, fallback))
            {
                ids.ArmorBody = buf[0];
                ids.ArmorLeft = buf[1];
                ids.ArmorRight = buf[2];
            }
            else
            {
                Log.Warning("DefaultAvatar: 9001~9003 同步随机失败");
            }

            if (TryPickSyncedAvatarIds(new[] { 9004, 9005, 9006, 9007, 9008, 9009 }, buf, fallback))
            {
                ids.Body = buf[0];
                ids.BodyArmLeft = buf[1];
                ids.BodyArmRight = buf[2];
                ids.FootLeft = buf[3];
                ids.FootRight = buf[4];
                ids.Head = buf[5];
            }
            else
            {
                Log.Warning("DefaultAvatar: 9004~9009 同步随机失败");
            }

            ids.EyeFront = PickRandomAvatarIdFromConstant(9010, fallback);
            ids.EyeBack = PickRandomAvatarIdFromConstant(9011, fallback);

            ids.Hair = PickRandomAvatarIdFromConstant(9012, fallback);
            return ids;
        }

        public static RoleAvatarIds FromRoleInfoProto(RoleInfoProto proto)
        {
            if (proto == null)
            {
                return default;
            }

            return new RoleAvatarIds
            {
                ArmorBody = proto.ArmorBody,
                ArmorLeft = proto.ArmorLeft,
                ArmorRight = proto.ArmorRight,
                Body = proto.Body,
                BodyArmLeft = proto.BodyArmLeft,
                BodyArmRight = proto.BodyArmRight,
                FootLeft = proto.FootLeft,
                FootRight = proto.FootRight,
                Head = proto.Head,
                EyeFront = proto.EyeFront,
                EyeBack = proto.EyeBack,
                Hair = proto.Hair,
            };
        }
        
        private static bool TryPickSyncedAvatarIds(int[] constantIds, int[] outIds, int fallback)
        {
            int n = constantIds.Length;
            var lists = new List<int>[n];
            for (int i = 0; i < n; i++)
            {
                lists[i] = ParseAvatarIdListFromConstant(constantIds[i]);
                if (lists[i].Count == 0)
                {
                    for (int j = 0; j < n; j++)
                    {
                        outIds[j] = fallback;
                    }

                    return false;
                }
            }

            int minLen = lists[0].Count;
            for (int i = 1; i < n; i++)
            {
                if (lists[i].Count < minLen)
                {
                    minLen = lists[i].Count;
                }
            }

            int idx = RandomGenerator.RandomNumber(0, minLen);
            for (int i = 0; i < n; i++)
            {
                outIds[i] = lists[i][idx];
            }

            return true;
        }

        private static List<int> ParseAvatarIdListFromConstant(int constantId)
        {
            List<int> ids = new List<int>();
            if (!ConstantConfigCategory.Instance.Contain(constantId))
            {
                Log.Warning($"DefaultAvatar: ConstantConfig 不存在 id={constantId}");
                return ids;
            }

            string raw = ConstantConfigCategory.Instance.Get(constantId).StringValue;
            if (string.IsNullOrWhiteSpace(raw))
            {
                Log.Warning($"DefaultAvatar: ConstantConfig id={constantId} StringValue 为空");
                return ids;
            }

            foreach (string part in raw.Split(','))
            {
                if (int.TryParse(part.Trim(), out int id))
                {
                    ids.Add(id);
                }
            }

            if (ids.Count == 0)
            {
                Log.Warning($"DefaultAvatar: ConstantConfig id={constantId} 无有效整数列表 raw={raw}");
            }

            return ids;
        }

        private static int PickRandomAvatarIdFromConstant(int constantId, int fallback)
        {
            List<int> ids = ParseAvatarIdListFromConstant(constantId);
            if (ids.Count == 0)
            {
                return fallback;
            }

            return RandomGenerator.RandomArray(ids);
        }
    }
}
