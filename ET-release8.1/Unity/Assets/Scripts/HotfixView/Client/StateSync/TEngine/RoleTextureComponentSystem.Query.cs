using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    public static partial class RoleTextureComponentSystem
    {
        private static bool MatchesRace(int groupRaceKey, int race)
        {
            return groupRaceKey == 0 || groupRaceKey == race;
        }

        private static bool MatchesGender(int groupGenderKey, int gender)
        {
            return groupGenderKey == 0 || groupGenderKey == gender;
        }

        private static bool MatchesEncodedRaceGender(int displayId, int race, int gender)
        {
            if (!FrameRoleTextureId.TryDecode(displayId, out _, out int encodedRace, out int encodedGender, out _))
            {
                return false;
            }

            return MatchesRace(encodedRace, race) && MatchesGender(encodedGender, gender);
        }

        private static bool MatchesNeedBodyType(FrameRoleTextureEntry entry, int characterBodyType, FrameRolePartType part)
        {
            if (part == FrameRolePartType.Body || entry == null || entry.needBodyType <= 0)
            {
                return true;
            }

            return entry.bodyType == characterBodyType;
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
            if (!self.TryGetEntry(bodyDisplayId, out FrameRoleTextureEntry bodyEntry))
            {
                return 0;
            }

            return bodyEntry.bodyType;
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
                if (raceGroup == null || !MatchesRace(raceGroup.raceKey, race) || raceGroup.genders == null)
                {
                    continue;
                }

                for (int g = 0; g < raceGroup.genders.Count; g++)
                {
                    FrameRoleGenderGroup genderGroup = raceGroup.genders[g];
                    if (genderGroup == null || !MatchesGender(genderGroup.genderKey, gender) || genderGroup.textures == null)
                    {
                        continue;
                    }

                    for (int i = 0; i < genderGroup.textures.Count; i++)
                    {
                        FrameRoleTextureEntry entry = genderGroup.textures[i];
                        if (entry != null
                            && MatchesEncodedRaceGender(entry.displayId, race, gender)
                            && MatchesNeedBodyType(entry, characterBodyType, part))
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
            if (displayId <= 0 || !MatchesEncodedRaceGender(displayId, race, gender))
            {
                return false;
            }

            if (!self.TryGetEntry(displayId, out FrameRoleTextureEntry entry))
            {
                return false;
            }

            int characterBodyType = part == FrameRolePartType.Body ? 0 : self.GetCharacterBodyType(bodyDisplayId);
            if (!MatchesNeedBodyType(entry, characterBodyType, part))
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
                if (raceGroup == null || !MatchesRace(raceGroup.raceKey, race) || raceGroup.genders == null)
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
                        if (MatchesGender(genderKey, genderFilter))
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
