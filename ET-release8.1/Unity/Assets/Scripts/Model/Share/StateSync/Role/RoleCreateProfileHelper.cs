using System;
using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 创角档案字符串：Race,Gender,Job（Race 为 RaceConfig.Type；Job 为 JobConfig.Id；Gender 1=Male 2=Female）。
    /// 持久化字段仍使用 <see cref="RoleInfo.BaseExternalDisplay"/> / Unit.BaseExternalDisplay。
    /// </summary>
    public static class RoleCreateProfileHelper
    {
        public const string DefaultProfileVal = "0,0,0";
        public const int ProfilePartCount = 3;
        public const char ProfileSeparator = ',';
        public const int JobUnitConfigIdBase = JobConfigHelper.LegacyJobUnitConfigIdBase;

        public static string ToProfileString(ExternalDisplayAppearance appearance)
        {
            return string.Join(
                ProfileSeparator.ToString(),
                appearance.Race,
                appearance.Gender,
                appearance.Job);
        }

        public static bool TryParseProfileString(string profile, out ExternalDisplayAppearance appearance)
        {
            appearance = default;
            if (string.IsNullOrWhiteSpace(profile))
            {
                return false;
            }

            string[] parts = profile.Split(ProfileSeparator);
            if (parts.Length != ProfilePartCount)
            {
                return false;
            }

            if (!TryParseEnumInt(parts[0], out int race)
                || !TryParseEnumInt(parts[1], out int gender)
                || !TryParseEnumInt(parts[2], out int job))
            {
                return false;
            }

            appearance.Race = race;
            appearance.Gender = gender;
            appearance.Job = JobConfigHelper.NormalizeJobId(job);
            return true;
        }

        public static bool IsBlankProfile(string profile)
        {
            if (string.IsNullOrWhiteSpace(profile))
            {
                return true;
            }

            if (!TryParseProfileString(profile, out ExternalDisplayAppearance appearance))
            {
                return false;
            }

            return appearance.Race <= 0 && appearance.Gender <= 0 && appearance.Job <= 0;
        }

        public static string NormalizeForStorage(string profile)
        {
            if (IsBlankProfile(profile))
            {
                return DefaultProfileVal;
            }

            if (TryParseProfileString(profile, out ExternalDisplayAppearance appearance))
            {
                return ToProfileString(appearance);
            }

            return profile.Trim();
        }

        public static void InitDefaultProfile(ref ExternalDisplayAppearance appearance)
        {
            appearance = default;
            appearance.Race = RaceConfigHelper.GetDefaultRaceType();
            appearance.Gender = ExternalDisplayHelper.DefaultGender;
            appearance.Job = JobConfigHelper.GetDefaultJobId();
        }

        public static void CycleJob(ref ExternalDisplayAppearance appearance, int delta, Func<List<int>> getAvailableJobs)
        {
            List<int> jobs = getAvailableJobs?.Invoke();
            if (jobs == null || jobs.Count == 0)
            {
                return;
            }

            appearance.Job = ExternalDisplayHelper.CycleValue(jobs, appearance.Job, delta);
        }

        public static void ValidateProfile(ref ExternalDisplayAppearance appearance, Func<List<int>> getAvailableRaces, Func<int, List<int>> getAvailableGenders, Func<List<int>> getAvailableJobs)
        {
            List<int> races = getAvailableRaces?.Invoke();
            if (races != null && races.Count > 0 && !races.Contains(appearance.Race))
            {
                appearance.Race = races[0];
            }

            List<int> genders = getAvailableGenders?.Invoke(appearance.Race);
            if (genders != null && genders.Count > 0 && !genders.Contains(appearance.Gender))
            {
                appearance.Gender = genders[0];
            }

            appearance.Job = JobConfigHelper.NormalizeJobId(appearance.Job);
            List<int> jobs = getAvailableJobs?.Invoke();
            if (jobs != null && jobs.Count > 0 && !jobs.Contains(appearance.Job))
            {
                appearance.Job = jobs[0];
            }
        }

        public static bool IsProfileValid(string profile)
        {
            if (IsBlankProfile(profile))
            {
                return true;
            }

            if (!TryParseProfileString(profile, out ExternalDisplayAppearance appearance))
            {
                return false;
            }

            if (appearance.Race <= 0 || appearance.Gender <= 0 || appearance.Job <= 0)
            {
                return false;
            }

            if (!IsKnownRace(appearance.Race) || !IsKnownGender(appearance.Gender))
            {
                return false;
            }

            return RoleModelPrefabHelper.IsJobGenderPrefabValid(appearance.Job, appearance.Gender)
                   && TryResolveUnitConfigId(appearance.Job, out _);
        }

        public static void ResolveRoleProfile(string profile, out int race, out int gender, out int configId)
        {
            race = ExternalDisplayHelper.DefaultRace;
            gender = ExternalDisplayHelper.DefaultGender;
            configId = ExternalDisplayHelper.DefaultUnitConfigId;

            if (!TryParseProfileString(profile, out ExternalDisplayAppearance appearance))
            {
                return;
            }

            if (appearance.Race > 0)
            {
                race = appearance.Race;
            }

            if (appearance.Gender > 0)
            {
                gender = appearance.Gender;
            }

            if (TryResolveUnitConfigId(appearance.Job, out int jobConfigId))
            {
                configId = jobConfigId;
            }
        }

        public static List<int> CollectAvailableJobIdsFromConfig()
        {
            return RoleModelPrefabHelper.GetAvailableJobIds();
        }

        public static List<int> GetAvailableRaceIds()
        {
            return RaceConfigHelper.GetAvailableRaceTypes();
        }

        public static List<int> GetAvailableGenderIds()
        {
            return new List<int>
            {
                (int)FrameRoleGenderType.Male,
                (int)FrameRoleGenderType.Female,
            };
        }

        public static bool TryResolveUnitConfigId(int job, out int unitConfigId)
        {
            return JobConfigHelper.TryGetUnitConfigId(job, out unitConfigId);
        }

        private static bool IsKnownRace(int race)
        {
            return GetAvailableRaceIds().Contains(race);
        }

        private static bool IsKnownGender(int gender)
        {
            return gender == (int)FrameRoleGenderType.Male || gender == (int)FrameRoleGenderType.Female;
        }

        private static bool TryParseEnumInt(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (!int.TryParse(text.Trim(), out value))
            {
                return false;
            }

            return value >= 0;
        }
    }
}
