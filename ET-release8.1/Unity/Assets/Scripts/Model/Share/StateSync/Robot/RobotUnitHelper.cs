namespace ET

{

    /// <summary>

    /// 压测机器人：地图服按刷怪方式创建 UnitConfig 9101（Type=Robot），服务端巡逻/施法。

    /// </summary>

    public static class RobotUnitHelper

    {

        public const int RobotUnitConfigId = 9101;



        public const string RobotViewPrefabName = "Robot001";

        public const string RobotViewPrefabAssetPath = "Assets/Bundles/Unit/Robot001.prefab";



        public const float RobotSpawnBaseX = 0.55f;

        public const float RobotSpawnBaseY = 0f;

        public const float RobotSpawnBaseZ = -7f;

        public const int RobotSpawnRadiusMm = 3000;

        /// <summary>Run 3（AutoSkill Case）默认 AI 组，对应 AIConfig 表 AIConfigId。</summary>
        public const int RunCase3AiConfigId = 3;
    }

}

