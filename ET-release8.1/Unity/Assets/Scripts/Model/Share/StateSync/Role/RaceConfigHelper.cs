using System.Collections.Generic;
using System.Linq;

namespace ET
{
    /// <summary>
    /// 创角档案 Race 以 <see cref="RaceConfig.Type"/>（与贴图 raceKey 一致）为准；展示名读表 <see cref="RaceConfig.Name"/>。
    /// </summary>
    public static class RaceConfigHelper
    {
        public const int LegacyRaceConfigIdBase = 1000;

        public static int NormalizeRaceType(int race)
        {
            if (race > LegacyRaceConfigIdBase && RaceConfigCategory.Instance != null && RaceConfigCategory.Instance.Contain(race))
            {
                return RaceConfigCategory.Instance.Get(race).Type;
            }

            return race;
        }

        public static bool TryGetRaceConfigByType(int raceType, out RaceConfig config)
        {
            config = null;
            raceType = NormalizeRaceType(raceType);
            if (raceType <= 0 || RaceConfigCategory.Instance == null)
            {
                return false;
            }

            foreach (KeyValuePair<int, RaceConfig> pair in RaceConfigCategory.Instance.GetAll())
            {
                RaceConfig row = pair.Value;
                if (row != null && row.Type == raceType)
                {
                    config = row;
                    return true;
                }
            }

            return false;
        }

        public static List<int> GetAvailableRaceTypes()
        {
            if (RaceConfigCategory.Instance == null)
            {
                return new List<int>();
            }

            return RaceConfigCategory.Instance.GetAll().Values
                .Where(c => c != null && c.Type > 0)
                .OrderBy(c => c.Id)
                .Select(c => c.Type)
                .ToList();
        }

        public static int GetDefaultRaceType()
        {
            List<int> races = GetAvailableRaceTypes();
            if (races.Count == 0)
            {
                return ExternalDisplayHelper.DefaultRace;
            }

            int human = (int)FrameRoleRaceType.Human;
            if (races.Contains(human))
            {
                return human;
            }

            return races[0];
        }

        public static string GetRaceDisplayName(int race)
        {
            if (TryGetRaceConfigByType(race, out RaceConfig config))
            {
                return config.Name;
            }

            return string.Empty;
        }
    }
}
