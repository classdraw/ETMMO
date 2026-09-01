namespace ET
{
    /// <summary>
    /// 外显配置表（ExternalDisplayConfig）查询与校验，供服务端及客户端 BodyType / 种族性别读表。
    /// </summary>
    public static class ExternalDisplayConfigHelper
    {
        public static bool TryGetConfig(int displayId, out ExternalDisplayConfig config)
        {
            config = null;
            if (displayId <= 0)
            {
                return false;
            }

            foreach (ExternalDisplayConfig item in ExternalDisplayConfigCategory.Instance.GetAll().Values)
            {
                if (item.DisplayId == displayId)
                {
                    config = item;
                    return true;
                }
            }

            return false;
        }

        public static bool IsExternalDisplayValid(string externalDisplay)
        {
            if (!ExternalDisplayHelper.TryParseExternalDisplayString(externalDisplay, out ExternalDisplayAppearance appearance))
            {
                return false;
            }

            if (appearance.BodyDisplayId <= 0)
            {
                return false;
            }

            if (!TryGetConfig(appearance.BodyDisplayId, out ExternalDisplayConfig bodyConfig))
            {
                return false;
            }

            int race = bodyConfig.Race;
            int gender = bodyConfig.Gender;
            int characterBodyType = bodyConfig.BodyType;

            if (!IsSlotValid(appearance.BodyDisplayId, (int)FrameRolePartType.Body, race, gender, characterBodyType, required: true))
            {
                return false;
            }

            if (!IsSlotValid(appearance.HeadDisplayId, (int)FrameRolePartType.Head, race, gender, characterBodyType, required: false))
            {
                return false;
            }

            if (!IsSlotValid(appearance.TailDisplayId, (int)FrameRolePartType.Tail, race, gender, characterBodyType, required: false))
            {
                return false;
            }

            if (!IsSlotValid(appearance.ShirtDisplayId, (int)FrameRolePartType.Shirt, race, gender, characterBodyType, required: false))
            {
                return false;
            }

            return IsSlotValid(appearance.PantsDisplayId, (int)FrameRolePartType.Pants, race, gender, characterBodyType, required: false);
        }

        public static void ResolveRoleProfile(string baseExternalDisplay, out int race, out int gender, out int configId)
        {
            race = ExternalDisplayHelper.DefaultRace;
            gender = ExternalDisplayHelper.DefaultGender;
            configId = ExternalDisplayHelper.DefaultUnitConfigId;

            if (!TryGetFirstNonZeroDisplayId(baseExternalDisplay, out int displayId))
            {
                return;
            }

            if (!TryGetConfig(displayId, out ExternalDisplayConfig config) || config.Race <= 0 || config.Gender <= 0)
            {
                return;
            }

            race = config.Race;
            gender = config.Gender;
        }

        /// <summary>
        /// 从外显字符串中取 DisplayId：须在配置表中存在，且解码 PartKey 为 Body。
        /// </summary>
        public static bool TryGetFirstNonZeroDisplayId(string externalDisplay, out int displayId)
        {
            displayId = 0;
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
                int id = ids[i];
                if (id <= 0 || !TryGetConfig(id, out _))
                {
                    continue;
                }

                if (!FrameRoleTextureId.TryDecode(id, out int partKey, out _, out _, out _)
                    || partKey != (int)FrameRolePartType.Body)
                {
                    continue;
                }

                displayId = id;
                return true;
            }

            return false;
        }

        private static bool IsSlotValid(int displayId, int partKey, int race, int gender, int characterBodyType, bool required)
        {
            if (displayId <= 0)
            {
                return !required;
            }

            if (!TryGetConfig(displayId, out ExternalDisplayConfig config))
            {
                return false;
            }

            if (!ExternalDisplayHelper.MatchesRace(config.Race, race) || !ExternalDisplayHelper.MatchesGender(config.Gender, gender))
            {
                return false;
            }

            return ExternalDisplayHelper.MatchesNeedBodyType(partKey, config.NeedBodyType, config.BodyType, characterBodyType);
        }
    }
}
