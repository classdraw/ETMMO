namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    [FriendOfAttribute(typeof(ET.Server.AOIEntity))]
    public class G2M_SecondLoginHandler : MessageLocationHandler<Unit, G2M_SecondLogin, M2G_SecondLogin>
    {
        protected override async ETTask Run(Unit unit, G2M_SecondLogin request, M2G_SecondLogin response)
        {
            Scene scene = unit.Scene();
            EventSystem.Instance.Publish(unit.Scene(), new UnitCheckCfg() { Unit = unit });
            EventSystem.Instance.Publish(unit.Scene(), new UnitReEffect() { Unit = unit });
            await MailHelper.LoginMailServer(scene, unit);
            await RelationshipHelper.LoginRelationshipServer(scene, unit);

            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = M2C_StartSceneChange.Create();
            m2CStartSceneChange.SceneInstanceId = scene.InstanceId;
            m2CStartSceneChange.SceneName = scene.Name;
            await unit.SendToClient(m2CStartSceneChange);


            M2C_CreateMyUnit m2CCreateMyUnit = M2C_CreateMyUnit.Create();
            m2CCreateMyUnit.Unit = UnitHelper.CreateUnitInfo(unit);
            await unit.SendToClient(m2CCreateMyUnit);
            await CoolDownNoticeHelper.SyncAllCoolDowns(unit);

            // 顶号/二次登录：地图 Unit 与 AOI 未销毁，SeeUnits 已含 A，EnterSight 不会再次触发，客户端收不到 M2C_CreateUnits
            AOIEntity aoi = unit.GetComponent<AOIEntity>();
            if (aoi != null)
            {
                foreach (AOIEntity other in aoi.SeeUnits.Values)
                {
                    if (other.Unit.Id == unit.Id)
                    {
                        continue;
                    }

                    MapMessageHelper.NoticeUnitAdd(unit, other.Unit);
                }
            }

            EventSystem.Instance.Publish(scene, new UnitEnterGame() { Unit = unit });
        }
    }
}
