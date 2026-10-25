namespace ET
{
    /// <summary>
    /// Unit 目录下 Role / Monster 子文件夹的 prefab 路径。
    /// </summary>
    public static class UnitBundlePathHelper
    {
        public const string Root = "Assets/Bundles/Unit";
        public const string RoleFolder = Root + "/Role";
        public const string MonsterFolder = Root + "/Monster";

        public static string GetRolePrefabPath(string prefabName)
        {
            return $"{RoleFolder}/{prefabName}.prefab";
        }

        public static string GetMonsterPrefabPath(string modelName)
        {
            return $"{MonsterFolder}/{modelName}.prefab";
        }
    }
}
