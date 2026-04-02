namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class G2M_SecondLoginHandler : MessageLocationHandler<Unit, G2M_SecondLogin, M2G_SecondLogin>
    {
        protected override async ETTask Run(Unit unit, G2M_SecondLogin request, M2G_SecondLogin response)
        {
            Scene scene = unit.Scene();
            EventSystem.Instance.Publish(unit.Scene(), new UnitCheckCfg() { Unit = unit });
            EventSystem.Instance.Publish(unit.Scene(), new UnitReEffect() { Unit = unit });

            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = M2C_StartSceneChange.Create();
            m2CStartSceneChange.SceneInstanceId = scene.InstanceId;
            m2CStartSceneChange.SceneName = scene.Name;
            await unit.SendToClient(m2CStartSceneChange);
            

            M2C_CreateMyUnit m2CCreateMyUnit = M2C_CreateMyUnit.Create();
            m2CCreateMyUnit.Unit = UnitHelper.CreateUnitInfo(unit);
            await unit.SendToClient(m2CCreateMyUnit);

            EventSystem.Instance.Publish(scene, new UnitEnterGame() { Unit = unit });
        }
    }
}
