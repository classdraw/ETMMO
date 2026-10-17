namespace ET
{
    /// <summary>
    /// ExternalDisplayConfig（Id / PartSlotKey / SpriteName / desc）查询与校验。
    /// </summary>
    public static class ExternalDisplayConfigHelper
    {
        public static bool TryGetConfig(int id, out ExternalDisplayConfig config)
        {
            config = null;
            if (id <= 0 || !ExternalDisplayConfigCategory.Instance.Contain(id))
            {
                return false;
            }

            config = ExternalDisplayConfigCategory.Instance.Get(id);
            return true;
        }

        /// <summary>创角档案 Race,Gender,Job 字符串校验。</summary>
        public static bool IsExternalDisplayValid(string externalDisplay)
        {
            return RoleCreateProfileHelper.IsProfileValid(externalDisplay);
        }

        public static void ResolveRoleProfile(string baseExternalDisplay, out int race, out int gender, out int configId)
        {
            RoleCreateProfileHelper.ResolveRoleProfile(baseExternalDisplay, out race, out gender, out configId);
        }

        public static bool TryGetFirstNonZeroDisplayId(string externalDisplay, out int id)
        {
            id = 0;
            if (!ExternalDisplayHelper.TryParseExternalDisplayString(externalDisplay, out ExternalDisplayAppearance appearance))
            {
                return false;
            }

            int[] ids =
            {
                appearance.BodyDisplayId,
                appearance.HeadDisplayId,
                appearance.TailDisplayId,
                appearance.ShirtDisplayId,
                appearance.PantsDisplayId,
            };

            for (int i = 0; i < ids.Length; i++)
            {
                int candidate = ids[i];
                if (candidate > 0 && TryGetConfig(candidate, out _))
                {
                    id = candidate;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Spine 表 Id，或 FrameRoleTextureId 编码（创角 UI 仍走 RoleTexture）。
        /// </summary>
        private static bool IsAppearanceSlotValid(int id, int expectedPartKey, bool required)
        {
            if (id <= 0)
            {
                return !required;
            }

            if (TryGetConfig(id, out _))
            {
                return true;
            }

            return FrameRoleTextureId.TryDecode(id, out int partKey, out _, out _, out _)
                   && partKey == expectedPartKey;
        }
    }
}
