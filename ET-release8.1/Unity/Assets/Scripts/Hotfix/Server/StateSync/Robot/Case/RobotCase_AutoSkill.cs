namespace ET.Server
{
    [Invoke(RobotCaseType.AutoSkill)]
    [FriendOf(typeof(RobotCase))]
    public class RobotCase_AutoSkill : ARobotCase
    {
        protected override async ETTask Run(RobotCase robotCase)
        {
            if (!RobotSpawnHelper.TryParseAutoSkillArgs(robotCase.CommandLine, out int count, out int mapConfigId))
            {
                Log.Console("命令格式: Run 3 <数量> <地图Id>  示例: Run 3 10 10001");
                return;
            }

            RobotSpawnContext ctx = RobotSpawnHelper.BuildSpawnContext(mapConfigId);
            Log.Console($"[Robot] Run 解析成功 count={count} map={mapConfigId} aiConfigId={ctx.AiConfigId} unitCfg={RobotUnitHelper.RobotUnitConfigId}");

            int spawned = await RobotSpawnHelper.SpawnPressureRobotsAsync(robotCase.Root(), ctx.MapConfigId, count);
            Log.Console($"[Robot] Run 完成 spawned={spawned}/{count} map={mapConfigId}");
            await ETTask.CompletedTask;
        }
    }
}
