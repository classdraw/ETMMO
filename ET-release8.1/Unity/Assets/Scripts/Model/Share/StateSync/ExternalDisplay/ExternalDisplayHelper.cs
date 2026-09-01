using System;
using System.Collections.Generic;

namespace ET
{
    public delegate List<int> ExternalDisplayPartIdsQuery(int partKey, int race, int gender, int bodyDisplayId);

    public delegate bool ExternalDisplayEntryQuery(int displayId, out ExternalDisplayEntryInfo entryInfo);

    /// <summary>
    /// 外显 / 装备（Shirt、Pants 等）穿戴限制规则。
    /// </summary>
    public static class ExternalDisplayHelper
    {
        public const string DefaultExternalDisplayVal = "1001,1023,1050,1053";
        public const int DefaultRace = (int)FrameRoleRaceType.Human;
        public const int DefaultGender = (int)FrameRoleGenderType.Male;
        public const char ExternalDisplaySeparator = ',';
        public const int ExternalDisplayPartCount = 5;
        public const int DefaultUnitConfigId = 1001;

        public static bool MatchesRace(int configRaceKey, int race)
        {
            return configRaceKey == 0 || configRaceKey == race;
        }

        public static bool MatchesGender(int configGenderKey, int gender)
        {
            return configGenderKey == 0 || configGenderKey == gender;
        }

        public static bool MatchesEncodedRaceGender(int displayId, int race, int gender)
        {
            if (!FrameRoleTextureId.TryDecode(displayId, out _, out int encodedRace, out int encodedGender, out _))
            {
                return false;
            }

            return MatchesRace(encodedRace, race) && MatchesGender(encodedGender, gender);
        }

        /// <summary>
        /// needBodyType &lt;= 0 不限制；否则 entryBodyType 须与角色 Body 的 bodyType 一致。
        /// </summary>
        public static bool MatchesNeedBodyType(int partKey, int needBodyType, int entryBodyType, int characterBodyType)
        {
            if (partKey == (int)FrameRolePartType.Body || needBodyType <= 0)
            {
                return true;
            }

            return entryBodyType == characterBodyType;
        }

        /// <summary>
        /// 单条外显配置是否满足种族 / 性别 / 体型限制。
        /// </summary>
        public static bool CanWearDisplay(
            int partKey,
            int displayId,
            int race,
            int gender,
            int characterBodyType,
            int entryBodyType,
            int entryNeedBodyType)
        {
            if (displayId <= 0)
            {
                return false;
            }

            if (!MatchesEncodedRaceGender(displayId, race, gender))
            {
                return false;
            }

            return MatchesNeedBodyType(partKey, entryNeedBodyType, entryBodyType, characterBodyType);
        }

        /// <summary>装备部位（Shirt / Pants）穿戴限制，规则与外显一致。</summary>
        public static bool CanWearEquipment(
            FrameRolePartType equipPart,
            int displayId,
            int race,
            int gender,
            int characterBodyType,
            int entryBodyType,
            int entryNeedBodyType)
        {
            return CanWearDisplay((int)equipPart, displayId, race, gender, characterBodyType, entryBodyType, entryNeedBodyType);
        }

        public static void InitDefaultAppearance(ref ExternalDisplayAppearance appearance, ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            appearance.Race = DefaultRace;
            appearance.Gender = DefaultGender;
            ValidatePartSelections(ref appearance, getPartDisplayIds);
        }

        public static void ValidatePartSelections(ref ExternalDisplayAppearance appearance, ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            appearance.BodyDisplayId = FixPartDisplayId(
                (int)FrameRolePartType.Body, appearance.Race, appearance.Gender, appearance.BodyDisplayId, appearance.BodyDisplayId, getPartDisplayIds);
            appearance.HeadDisplayId = FixPartDisplayId(
                (int)FrameRolePartType.Head, appearance.Race, appearance.Gender, appearance.HeadDisplayId, appearance.BodyDisplayId, getPartDisplayIds);
            appearance.TailDisplayId = FixPartDisplayId(
                (int)FrameRolePartType.Tail, appearance.Race, appearance.Gender, appearance.TailDisplayId, appearance.BodyDisplayId, getPartDisplayIds);
            appearance.ShirtDisplayId = FixPartDisplayId(
                (int)FrameRolePartType.Shirt, appearance.Race, appearance.Gender, appearance.ShirtDisplayId, appearance.BodyDisplayId, getPartDisplayIds);
            appearance.PantsDisplayId = FixPartDisplayId(
                (int)FrameRolePartType.Pants, appearance.Race, appearance.Gender, appearance.PantsDisplayId, appearance.BodyDisplayId, getPartDisplayIds);
        }

        public static void CycleRace(
            ref ExternalDisplayAppearance appearance,
            int delta,
            Func<List<int>> getAvailableRaces,
            Func<int, List<int>> getAvailableGenders,
            ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            List<int> races = getAvailableRaces();
            if (races.Count == 0)
            {
                return;
            }

            appearance.Race = CycleValue(races, appearance.Race, delta);
            List<int> genders = getAvailableGenders(appearance.Race);
            if (genders.Count > 0 && !genders.Contains(appearance.Gender))
            {
                appearance.Gender = genders[0];
            }

            ValidatePartSelections(ref appearance, getPartDisplayIds);
        }

        public static void CycleGender(
            ref ExternalDisplayAppearance appearance,
            int delta,
            Func<int, List<int>> getAvailableGenders,
            ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            List<int> genders = getAvailableGenders(appearance.Race);
            if (genders.Count == 0)
            {
                return;
            }

            appearance.Gender = CycleValue(genders, appearance.Gender, delta);
            ValidatePartSelections(ref appearance, getPartDisplayIds);
        }

        public static void CyclePart(
            ref ExternalDisplayAppearance appearance,
            FrameRolePartType part,
            int delta,
            ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            int current = GetPartDisplayId(ref appearance, part);
            List<int> ids = getPartDisplayIds((int)part, appearance.Race, appearance.Gender, appearance.BodyDisplayId);
            if (ids.Count == 0)
            {
                SetPartDisplayId(ref appearance, part, 0);
                if (part == FrameRolePartType.Body)
                {
                    ValidatePartSelections(ref appearance, getPartDisplayIds);
                }

                return;
            }

            SetPartDisplayId(ref appearance, part, CycleValue(ids, current, delta));
            if (part == FrameRolePartType.Body)
            {
                ValidatePartSelections(ref appearance, getPartDisplayIds);
            }
        }

        public static int FixPartDisplayId(
            int partKey,
            int race,
            int gender,
            int currentDisplayId,
            int bodyDisplayId,
            ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            List<int> ids = getPartDisplayIds(partKey, race, gender, bodyDisplayId);
            if (ids.Count == 0)
            {
                return 0;
            }

            if (currentDisplayId > 0 && ids.Contains(currentDisplayId))
            {
                return currentDisplayId;
            }

            if (currentDisplayId > 0 && FrameRoleTextureId.TryDecode(currentDisplayId, out _, out _, out _, out int index))
            {
                for (int i = 0; i < ids.Count; i++)
                {
                    if (FrameRoleTextureId.TryDecode(ids[i], out _, out _, out _, out int candidateIndex) && candidateIndex == index)
                    {
                        return ids[i];
                    }
                }
            }

            return ids[0];
        }

        public static int CycleValue(List<int> values, int current, int delta)
        {
            if (values == null || values.Count == 0)
            {
                return current;
            }

            int index = values.IndexOf(current);
            if (index < 0)
            {
                index = 0;
            }
            else
            {
                index = (index + delta) % values.Count;
                if (index < 0)
                {
                    index += values.Count;
                }
            }

            return values[index];
        }

        /// <summary>
        /// 外显字符串格式：Body,Head,Tail,Shirt,Pants（DisplayId，可选部位可为 0）。
        /// </summary>
        public static string ToExternalDisplayString(ExternalDisplayAppearance appearance)
        {
            return string.Join(
                ExternalDisplaySeparator.ToString(),
                appearance.BodyDisplayId,
                appearance.HeadDisplayId,
                appearance.TailDisplayId,
                appearance.ShirtDisplayId,
                appearance.PantsDisplayId);
        }

        public static bool TryParseExternalDisplayString(string externalDisplay, out ExternalDisplayAppearance appearance)
        {
            appearance = default;
            if (string.IsNullOrWhiteSpace(externalDisplay))
            {
                return false;
            }

            string[] parts = externalDisplay.Split(ExternalDisplaySeparator);
            if (parts.Length != ExternalDisplayPartCount)
            {
                return false;
            }

            if (!TryParseDisplayId(parts[0], out int bodyDisplayId)
                || !TryParseDisplayId(parts[1], out int headDisplayId)
                || !TryParseDisplayId(parts[2], out int tailDisplayId)
                || !TryParseDisplayId(parts[3], out int shirtDisplayId)
                || !TryParseDisplayId(parts[4], out int pantsDisplayId))
            {
                return false;
            }

            appearance.BodyDisplayId = bodyDisplayId;
            appearance.HeadDisplayId = headDisplayId;
            appearance.TailDisplayId = tailDisplayId;
            appearance.ShirtDisplayId = shirtDisplayId;
            appearance.PantsDisplayId = pantsDisplayId;

            if (appearance.BodyDisplayId > 0
                && FrameRoleTextureId.TryDecode(appearance.BodyDisplayId, out _, out int race, out int gender, out _))
            {
                appearance.Race = race;
                appearance.Gender = gender;
            }

            return true;
        }

        /// <summary>
        /// 校验外显字符串：各 DisplayId 存在、部位匹配，且满足种族 / 性别 / 体型穿戴规则。
        /// </summary>
        public static bool IsExternalDisplayValid(string externalDisplay, ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            if (!TryParseExternalDisplayString(externalDisplay, out ExternalDisplayAppearance appearance))
            {
                return false;
            }

            return IsAppearanceValid(appearance, getPartDisplayIds);
        }

        public static bool IsAppearanceValid(ExternalDisplayAppearance appearance, ExternalDisplayPartIdsQuery getPartDisplayIds)
        {
            if (getPartDisplayIds == null || appearance.BodyDisplayId <= 0)
            {
                return false;
            }

            if (!FrameRoleTextureId.TryDecode(appearance.BodyDisplayId, out _, out int bodyRace, out int bodyGender, out _))
            {
                return false;
            }

            if (appearance.Race <= 0 || appearance.Gender <= 0)
            {
                appearance.Race = bodyRace;
                appearance.Gender = bodyGender;
            }
            else if (appearance.Race != bodyRace || appearance.Gender != bodyGender)
            {
                return false;
            }

            int bodyDisplayId = appearance.BodyDisplayId;
            if (!IsSlotDisplayValid(FrameRolePartType.Body, appearance.BodyDisplayId, appearance.Race, appearance.Gender, bodyDisplayId, getPartDisplayIds, required: true))
            {
                return false;
            }

            if (!IsSlotDisplayValid(FrameRolePartType.Head, appearance.HeadDisplayId, appearance.Race, appearance.Gender, bodyDisplayId, getPartDisplayIds, required: false))
            {
                return false;
            }

            if (!IsSlotDisplayValid(FrameRolePartType.Tail, appearance.TailDisplayId, appearance.Race, appearance.Gender, bodyDisplayId, getPartDisplayIds, required: false))
            {
                return false;
            }

            if (!IsSlotDisplayValid(FrameRolePartType.Shirt, appearance.ShirtDisplayId, appearance.Race, appearance.Gender, bodyDisplayId, getPartDisplayIds, required: false))
            {
                return false;
            }

            return IsSlotDisplayValid(FrameRolePartType.Pants, appearance.PantsDisplayId, appearance.Race, appearance.Gender, bodyDisplayId, getPartDisplayIds, required: false);
        }

        /// <summary>
        /// 按种族 / 性别 / 身材补全外显：Body 匹配 bodyType，其余部位取合法默认值。
        /// </summary>
        public static ExternalDisplayAppearance CreateDefaultAppearance(
            int race,
            int gender,
            int bodyType,
            ExternalDisplayPartIdsQuery getPartDisplayIds,
            ExternalDisplayEntryQuery tryGetEntry)
        {
            ExternalDisplayAppearance appearance = new ExternalDisplayAppearance
            {
                Race = race,
                Gender = gender,
                BodyDisplayId = PickBodyDisplayId(race, gender, bodyType, getPartDisplayIds, tryGetEntry),
            };

            ValidatePartSelections(ref appearance, getPartDisplayIds);
            return appearance;
        }

        private static bool TryParseDisplayId(string text, out int displayId)
        {
            displayId = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (!int.TryParse(text.Trim(), out displayId))
            {
                return false;
            }

            return displayId >= 0;
        }

        private static bool IsSlotDisplayValid(
            FrameRolePartType part,
            int displayId,
            int race,
            int gender,
            int bodyDisplayId,
            ExternalDisplayPartIdsQuery getPartDisplayIds,
            bool required)
        {
            if (displayId <= 0)
            {
                return !required;
            }

            if (!FrameRoleTextureId.TryDecode(displayId, out int partKey, out _, out _, out _))
            {
                return false;
            }

            if (partKey != (int)part)
            {
                return false;
            }

            List<int> ids = getPartDisplayIds((int)part, race, gender, bodyDisplayId);
            return ids.Contains(displayId);
        }

        private static int PickBodyDisplayId(
            int race,
            int gender,
            int bodyType,
            ExternalDisplayPartIdsQuery getPartDisplayIds,
            ExternalDisplayEntryQuery tryGetEntry)
        {
            List<int> bodyIds = getPartDisplayIds((int)FrameRolePartType.Body, race, gender, 0);
            if (bodyIds.Count == 0)
            {
                return 0;
            }

            if (tryGetEntry != null)
            {
                for (int i = 0; i < bodyIds.Count; i++)
                {
                    if (tryGetEntry(bodyIds[i], out ExternalDisplayEntryInfo info) && info.BodyType == bodyType)
                    {
                        return bodyIds[i];
                    }
                }
            }

            return bodyIds[0];
        }

        private static int GetPartDisplayId(ref ExternalDisplayAppearance appearance, FrameRolePartType part)
        {
            return part switch
            {
                FrameRolePartType.Body => appearance.BodyDisplayId,
                FrameRolePartType.Head => appearance.HeadDisplayId,
                FrameRolePartType.Tail => appearance.TailDisplayId,
                FrameRolePartType.Shirt => appearance.ShirtDisplayId,
                FrameRolePartType.Pants => appearance.PantsDisplayId,
                _ => 0,
            };
        }

        private static void SetPartDisplayId(ref ExternalDisplayAppearance appearance, FrameRolePartType part, int displayId)
        {
            switch (part)
            {
                case FrameRolePartType.Body:
                    appearance.BodyDisplayId = displayId;
                    break;
                case FrameRolePartType.Head:
                    appearance.HeadDisplayId = displayId;
                    break;
                case FrameRolePartType.Tail:
                    appearance.TailDisplayId = displayId;
                    break;
                case FrameRolePartType.Shirt:
                    appearance.ShirtDisplayId = displayId;
                    break;
                case FrameRolePartType.Pants:
                    appearance.PantsDisplayId = displayId;
                    break;
            }
        }
    }
}
