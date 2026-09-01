using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public static partial class RoleTextureComponentSystem
    {
        private static ExternalDisplayPartIdsQuery CreatePartIdsQuery(this RoleTextureComponent self)
        {
            return (partKey, race, gender, bodyDisplayId) =>
                self.GetPartDisplayIds((FrameRolePartType)partKey, race, gender, bodyDisplayId);
        }

        private static ExternalDisplayEntryQuery CreateEntryQuery(this RoleTextureComponent self)
        {
            return (int displayId, out ExternalDisplayEntryInfo entryInfo) =>
            {
                entryInfo = default;
                if (!FrameRoleTextureId.TryDecode(displayId, out int partKey, out _, out _, out _))
                {
                    return false;
                }

                if (!ExternalDisplayConfigHelper.TryGetConfig(displayId, out ExternalDisplayConfig config))
                {
                    return false;
                }

                entryInfo = new ExternalDisplayEntryInfo
                {
                    PartKey = partKey,
                    BodyType = config.BodyType,
                    NeedBodyType = config.NeedBodyType,
                };
                return true;
            };
        }

        public static bool IsExternalDisplayValid(this RoleTextureComponent self, string externalDisplay)
        {
            return ExternalDisplayHelper.IsExternalDisplayValid(externalDisplay, self.CreatePartIdsQuery());
        }

        public static ExternalDisplayAppearance CreateDefaultAppearance(this RoleTextureComponent self, int race, int gender, int bodyType)
        {
            return ExternalDisplayHelper.CreateDefaultAppearance(
                race,
                gender,
                bodyType,
                self.CreatePartIdsQuery(),
                self.CreateEntryQuery());
        }

        public static string CreateDefaultExternalDisplay(this RoleTextureComponent self, int race, int gender, int bodyType)
        {
            return ExternalDisplayHelper.ToExternalDisplayString(self.CreateDefaultAppearance(race, gender, bodyType));
        }

        public static void InitDefaultAppearance(this RoleTextureComponent self, ref ExternalDisplayAppearance appearance)
        {
            ExternalDisplayHelper.InitDefaultAppearance(ref appearance, self.CreatePartIdsQuery());
        }

        public static void ValidateAppearance(this RoleTextureComponent self, ref ExternalDisplayAppearance appearance)
        {
            ExternalDisplayHelper.ValidatePartSelections(ref appearance, self.CreatePartIdsQuery());
        }

        public static void CycleAppearanceRace(this RoleTextureComponent self, ref ExternalDisplayAppearance appearance, int delta)
        {
            ExternalDisplayHelper.CycleRace(
                ref appearance,
                delta,
                () => self.GetAvailableRaces(),
                race => self.GetAvailableGenders(race),
                self.CreatePartIdsQuery());
        }

        public static void CycleAppearanceGender(this RoleTextureComponent self, ref ExternalDisplayAppearance appearance, int delta)
        {
            ExternalDisplayHelper.CycleGender(
                ref appearance,
                delta,
                race => self.GetAvailableGenders(race),
                self.CreatePartIdsQuery());
        }

        public static void CycleAppearancePart(this RoleTextureComponent self, ref ExternalDisplayAppearance appearance, FrameRolePartType part, int delta)
        {
            ExternalDisplayHelper.CyclePart(ref appearance, part, delta, self.CreatePartIdsQuery());
        }

        public static bool TryGetEntry(this RoleTextureComponent self, int displayId, out FrameRoleTextureEntry entry)
        {
            entry = null;
            if (displayId <= 0 || !FrameRoleTextureId.TryDecode(displayId, out int partKey, out _, out _, out _))
            {
                return false;
            }

            if (!self.RoleTextureConfigs.TryGetValue((FrameRolePartType)partKey, out FrameRoleTextureConfig config))
            {
                return false;
            }

            return config.TryGetEntry(displayId, out entry);
        }

        public static int GetCharacterBodyType(this RoleTextureComponent self, int bodyDisplayId)
        {
            if (ExternalDisplayConfigHelper.TryGetConfig(bodyDisplayId, out ExternalDisplayConfig config))
            {
                return config.BodyType;
            }

            return 0;
        }

        public static bool TryGetTexture(this RoleTextureComponent self, int displayId, out Texture2D texture)
        {
            texture = null;
            if (displayId <= 0 || !FrameRoleTextureId.TryDecode(displayId, out int partKey, out _, out _, out _))
            {
                return false;
            }

            if (!self.RoleTextureConfigs.TryGetValue((FrameRolePartType)partKey, out FrameRoleTextureConfig config))
            {
                return false;
            }

            return config.TryGetTexture(displayId, out texture);
        }

        public static List<int> GetPartDisplayIds(
            this RoleTextureComponent self,
            FrameRolePartType part,
            int race,
            int gender,
            int bodyDisplayId = 0)
        {
            List<int> result = new List<int>();
            if (!self.RoleTextureConfigs.TryGetValue(part, out FrameRoleTextureConfig config) || config.races == null)
            {
                return result;
            }

            int characterBodyType = part == FrameRolePartType.Body ? 0 : self.GetCharacterBodyType(bodyDisplayId);

            for (int r = 0; r < config.races.Count; r++)
            {
                FrameRoleRaceGroup raceGroup = config.races[r];
                if (raceGroup == null || !ExternalDisplayHelper.MatchesRace(raceGroup.raceKey, race) || raceGroup.genders == null)
                {
                    continue;
                }

                for (int g = 0; g < raceGroup.genders.Count; g++)
                {
                    FrameRoleGenderGroup genderGroup = raceGroup.genders[g];
                    if (genderGroup == null || !ExternalDisplayHelper.MatchesGender(genderGroup.genderKey, gender) || genderGroup.textures == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < genderGroup.textures.Count; i++)
                    {
                        FrameRoleTextureEntry entry = genderGroup.textures[i];
                        if (entry == null)
                        {
                            continue;
                        }

                        int entryBodyType = entry.bodyType;
                        int entryNeedBodyType = entry.needBodyType;
                        if (ExternalDisplayConfigHelper.TryGetConfig(entry.displayId, out ExternalDisplayConfig wearConfig))
                        {
                            entryBodyType = wearConfig.BodyType;
                            entryNeedBodyType = wearConfig.NeedBodyType;
                        }

                        if (ExternalDisplayHelper.CanWearDisplay(
                                config.partKey,
                                entry.displayId,
                                race,
                                gender,
                                characterBodyType,
                                entryBodyType,
                                entryNeedBodyType))
                        {
                            result.Add(entry.displayId);
                        }
                    }
                }
            }

            return result;
        }

        public static List<int> GetAvailableRaces(this RoleTextureComponent self)
        {
            return CollectRaceKeys(self, 0);
        }

        public static string GetPartDisplayName(this RoleTextureComponent self, int displayId)
        {
            if (displayId <= 0)
            {
                return "无";
            }

            if (!FrameRoleTextureId.TryDecode(displayId, out int partKey, out _, out _, out _))
            {
                return displayId.ToString();
            }

            if (!self.RoleTextureConfigs.TryGetValue((FrameRolePartType)partKey, out FrameRoleTextureConfig config))
            {
                return displayId.ToString();
            }

            if (config.TryGetEntry(displayId, out FrameRoleTextureEntry entry))
            {
                if (!string.IsNullOrEmpty(entry.desc))
                {
                    return entry.desc;
                }

                if (!string.IsNullOrEmpty(entry.name))
                {
                    return entry.name;
                }
            }

            return displayId.ToString();
        }

        public static bool IsPartDisplayValid(
            this RoleTextureComponent self,
            FrameRolePartType part,
            int displayId,
            int race,
            int gender,
            int bodyDisplayId = 0)
        {
            if (displayId <= 0 || !ExternalDisplayHelper.MatchesEncodedRaceGender(displayId, race, gender))
            {
                return false;
            }

            if (!self.TryGetEntry(displayId, out FrameRoleTextureEntry entry))
            {
                return false;
            }

            int entryBodyType = entry.bodyType;
            int entryNeedBodyType = entry.needBodyType;
            if (ExternalDisplayConfigHelper.TryGetConfig(displayId, out ExternalDisplayConfig config))
            {
                entryBodyType = config.BodyType;
                entryNeedBodyType = config.NeedBodyType;
            }

            int characterBodyType = part == FrameRolePartType.Body ? 0 : self.GetCharacterBodyType(bodyDisplayId);
            if (!ExternalDisplayHelper.MatchesNeedBodyType((int)part, entryNeedBodyType, entryBodyType, characterBodyType))
            {
                return false;
            }

            List<int> ids = self.GetPartDisplayIds(part, race, gender, bodyDisplayId);
            return ids.Contains(displayId);
        }

        public static List<int> GetAvailableGenders(this RoleTextureComponent self, int race)
        {
            List<int> result = new List<int>();
            if (!self.RoleTextureConfigs.TryGetValue(FrameRolePartType.Body, out FrameRoleTextureConfig config) || config.races == null)
            {
                return result;
            }

            for (int r = 0; r < config.races.Count; r++)
            {
                FrameRoleRaceGroup raceGroup = config.races[r];
                if (raceGroup == null || !ExternalDisplayHelper.MatchesRace(raceGroup.raceKey, race) || raceGroup.genders == null)
                {
                    continue;
                }

                for (int g = 0; g < raceGroup.genders.Count; g++)
                {
                    FrameRoleGenderGroup genderGroup = raceGroup.genders[g];
                    if (genderGroup == null || genderGroup.genderKey == 0)
                    {
                        continue;
                    }

                    if (!result.Contains(genderGroup.genderKey))
                    {
                        result.Add(genderGroup.genderKey);
                    }
                }
            }

            result.Sort();
            return result;
        }

        private static List<int> CollectRaceKeys(RoleTextureComponent self, int genderFilter)
        {
            List<int> result = new List<int>();
            if (!self.RoleTextureConfigs.TryGetValue(FrameRolePartType.Body, out FrameRoleTextureConfig config) || config.races == null)
            {
                return result;
            }

            for (int r = 0; r < config.races.Count; r++)
            {
                FrameRoleRaceGroup raceGroup = config.races[r];
                if (raceGroup == null)
                {
                    continue;
                }

                if (genderFilter > 0 && raceGroup.genders != null)
                {
                    bool hasGender = false;
                    for (int g = 0; g < raceGroup.genders.Count; g++)
                    {
                        int genderKey = raceGroup.genders[g]?.genderKey ?? -1;
                        if (ExternalDisplayHelper.MatchesGender(genderKey, genderFilter))
                        {
                            hasGender = true;
                            break;
                        }
                    }

                    if (!hasGender)
                    {
                        continue;
                    }
                }

                if (raceGroup.raceKey == 0 || result.Contains(raceGroup.raceKey))
                {
                    continue;
                }

                result.Add(raceGroup.raceKey);
            }

            result.Sort();
            return result;
        }
    }
}
