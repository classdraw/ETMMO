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

        public static bool IsExternalDisplayValid(string externalDisplay)
        {
            if (ExternalDisplayHelper.IsBlankExternalDisplay(externalDisplay))
            {
                return true;
            }

            if (!ExternalDisplayHelper.TryParseExternalDisplayString(externalDisplay, out ExternalDisplayAppearance appearance))
            {
                return false;
            }

            if (!IsAppearanceSlotValid(appearance.BodyDisplayId, (int)FrameRolePartType.Body, required: true))
            {
                return false;
            }

            return IsAppearanceSlotValid(appearance.HeadDisplayId, (int)FrameRolePartType.Head, required: false)
                   && IsAppearanceSlotValid(appearance.TailDisplayId, (int)FrameRolePartType.Tail, required: false)
                   && IsAppearanceSlotValid(appearance.ShirtDisplayId, (int)FrameRolePartType.Shirt, required: false)
                   && IsAppearanceSlotValid(appearance.PantsDisplayId, (int)FrameRolePartType.Pants, required: false);
        }

        public static void ResolveRoleProfile(string baseExternalDisplay, out int race, out int gender, out int configId)
        {
            race = ExternalDisplayHelper.DefaultRace;
            gender = ExternalDisplayHelper.DefaultGender;
            configId = ExternalDisplayHelper.DefaultUnitConfigId;

            if (!ExternalDisplayHelper.TryParseExternalDisplayString(baseExternalDisplay, out ExternalDisplayAppearance appearance))
            {
                return;
            }

            if (appearance.BodyDisplayId > 0
                && FrameRoleTextureId.TryDecode(
                    appearance.BodyDisplayId,
                    out int partKey,
                    out int bodyRace,
                    out int bodyGender,
                    out _)
                && partKey == (int)FrameRolePartType.Body)
            {
                race = bodyRace;
                gender = bodyGender;
                return;
            }

            if (TryGetFirstNonZeroDisplayId(baseExternalDisplay, out int id)
                && TryGetConfig(id, out ExternalDisplayConfig config)
                && config.PartSlotKey == (int)UnitPartSlot.BodyBody)
            {
                configId = id;
            }

            if (configId <= 0)
            {
                configId = ExternalDisplayHelper.DefaultUnitConfigId;
            }
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
