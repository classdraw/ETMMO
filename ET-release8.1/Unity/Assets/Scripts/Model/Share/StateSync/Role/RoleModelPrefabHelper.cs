using System.Collections.Generic;

namespace ET
{
    /// <summary>
    /// 创角/玩家视图 prefab 路径（委托 <see cref="JobConfigHelper"/>）。
    /// </summary>
    public static class RoleModelPrefabHelper
    {
        public const string UnitBundleFolder = JobConfigHelper.UnitBundleFolder;
        public const string UiObjectLayerName = JobConfigHelper.UiObjectLayerName;

        public static bool TryGetPrefabName(int job, int gender, out string prefabName)
            => JobConfigHelper.TryGetPrefabName(job, gender, out prefabName);

        public static bool TryGetPrefabAssetPath(int job, int gender, out string assetPath)
            => JobConfigHelper.TryGetPrefabAssetPath(job, gender, out assetPath);

        public static List<int> GetAvailableJobIds()
            => JobConfigHelper.GetAvailableJobIds();

        public static string GetJobDisplayName(int job)
            => JobConfigHelper.GetJobDisplayName(job);

        public static bool IsJobGenderPrefabValid(int job, int gender)
            => JobConfigHelper.IsJobGenderPrefabValid(job, gender);
    }
}
