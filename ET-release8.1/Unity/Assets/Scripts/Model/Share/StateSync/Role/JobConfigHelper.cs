using System.Collections.Generic;
using System.Linq;

namespace ET
{
    /// <summary>
    /// 创角/玩家视图：职业以 <see cref="JobConfig"/> 表 Id 为准；模型名为 Model + _F/_M；数值 Unit 用 UnitId。
    /// </summary>
    public static class JobConfigHelper
    {
        public const string UnitBundleFolder = UnitBundlePathHelper.Root;
        public const string UnitRoleBundleFolder = UnitBundlePathHelper.RoleFolder;
        public const string UiObjectLayerName = "UIObject";

        /// <summary>旧档案 Job 为 1~5 枚举时，映射为 Job 表 Id（1001 起）。</summary>
        public const int LegacyJobUnitConfigIdBase = 1000;

        public static int NormalizeJobId(int job)
        {
            if (job > 0 && job < 100)
            {
                return LegacyJobUnitConfigIdBase + job;
            }

            return job;
        }

        public static bool TryGetJobConfig(int job, out JobConfig config)
        {
            config = null;
            job = NormalizeJobId(job);
            if (job <= 0 || !JobConfigCategory.Instance.Contain(job))
            {
                return false;
            }

            config = JobConfigCategory.Instance.Get(job);
            return config != null;
        }

        public static List<int> GetAvailableJobIds()
        {
            if (JobConfigCategory.Instance == null)
            {
                return new List<int>();
            }

            return JobConfigCategory.Instance.GetAll().Keys.OrderBy(id => id).ToList();
        }

        public static int GetDefaultJobId()
        {
            List<int> jobs = GetAvailableJobIds();
            if (jobs.Count == 0)
            {
                return LegacyJobUnitConfigIdBase + (int)FrameRoleJobType.Archer;
            }

            if (jobs.Contains(1005))
            {
                return 1005;
            }

            return jobs[0];
        }

        public static string GetJobDisplayName(int job)
        {
            return TryGetJobConfig(job, out JobConfig config) ? config.Name : string.Empty;
        }

        public static bool TryGetUnitConfigId(int job, out int unitConfigId)
        {
            unitConfigId = 0;
            if (!TryGetJobConfig(job, out JobConfig config) || config.UnitId <= 0)
            {
                return false;
            }

            unitConfigId = config.UnitId;
            return true;
        }

        public static bool TryGetPrefabName(int job, int gender, out string prefabName)
        {
            prefabName = null;
            if (!TryGetJobConfig(job, out JobConfig config) || string.IsNullOrWhiteSpace(config.Model))
            {
                return false;
            }

            if (!TryGetGenderSuffix(gender, out string genderSuffix))
            {
                return false;
            }

            prefabName = $"{config.Model.Trim()}_{genderSuffix}";
            return true;
        }

        public static bool TryGetPrefabAssetPath(int job, int gender, out string assetPath)
        {
            assetPath = null;
            if (!TryGetPrefabName(job, gender, out string prefabName))
            {
                return false;
            }

            assetPath = UnitBundlePathHelper.GetRolePrefabPath(prefabName);
            return true;
        }

        public static bool IsJobGenderPrefabValid(int job, int gender)
        {
            return TryGetPrefabAssetPath(job, gender, out _);
        }

        private static bool TryGetGenderSuffix(int gender, out string suffix)
        {
            suffix = gender switch
            {
                (int)FrameRoleGenderType.Male => "M",
                (int)FrameRoleGenderType.Female => "F",
                _ => null,
            };

            return suffix != null;
        }
    }
}
