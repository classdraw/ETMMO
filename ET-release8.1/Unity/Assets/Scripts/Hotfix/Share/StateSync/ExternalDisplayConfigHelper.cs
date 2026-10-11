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
            if (!ExternalDisplayHelper.TryParseExternalDisplayString(externalDisplay, out ExternalDisplayAppearance appearance))
            {
                return false;
            }

            if (appearance.BodyDisplayId <= 0 || !TryGetConfig(appearance.BodyDisplayId, out _))
            {
                return false;
            }

            return IsOptionalSlotValid(appearance.HeadDisplayId)
                   && IsOptionalSlotValid(appearance.TailDisplayId)
                   && IsOptionalSlotValid(appearance.ShirtDisplayId)
                   && IsOptionalSlotValid(appearance.PantsDisplayId);
        }

        public static void ResolveRoleProfile(string baseExternalDisplay, out int race, out int gender, out int configId)
        {
            race = ExternalDisplayHelper.DefaultRace;
            gender = ExternalDisplayHelper.DefaultGender;
            configId = ExternalDisplayHelper.DefaultUnitConfigId;

            if (TryGetFirstNonZeroDisplayId(baseExternalDisplay, out int id)
                && TryGetConfig(id, out ExternalDisplayConfig config)
                && config.PartSlotKey == (int)UnitPartSlot.BodyBody)
            {
                configId = id;
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

        private static bool IsOptionalSlotValid(int id)
        {
            return id <= 0 || TryGetConfig(id, out _);
        }
    }
}
