namespace ET.Server
{
    /// <summary>
    /// 下线时更新 RoleInfo 等基础库表；可按项目扩展为写坐标、背包等。
    /// </summary>
    [Event(SceneType.Map)]
    [FriendOf(typeof(RoleInfo))]
    public class UnitOfflinePersist_SaveRoleInfoHandler : AEvent<Scene, UnitOfflinePersist>
    {
        protected override async ETTask Run(Scene scene, UnitOfflinePersist args)
        {
            /**
            Unit unit = args.Unit;
            if (unit == null || unit.IsDisposed)
            {
                return;
            }

            DBManagerComponent dbManager = unit.Root().GetComponent<DBManagerComponent>();
            if (dbManager == null)
            {
                Log.Warning($"UnitOfflinePersist: 无 DBManagerComponent，跳过存库 unitId={unit.Id}");
                return;
            }

            DBComponent db = dbManager.GetZoneDB(unit.Zone());
            RoleInfo roleInfo = await db.Query<RoleInfo>(unit.Id);
            if (roleInfo == null)
            {
                Log.Warning($"UnitOfflinePersist: 未找到 RoleInfo，跳过存库 unitId={unit.Id}");
                return;
            }

            roleInfo.LastOfflineTime = TimeInfo.Instance.ServerNow();
            await db.Save(roleInfo);
            roleInfo.Dispose();*/
            await ETTask.CompletedTask;
        }
    }
}
