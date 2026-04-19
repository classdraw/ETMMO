namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class G2M_RequestExitGameHandler: MessageLocationHandler<Unit, G2M_RequestExitGame,M2G_RequestExitGame>
    {
        protected override async ETTask Run(Unit unit, G2M_RequestExitGame request, M2G_RequestExitGame response)
        {
            await UnitHelper.ForceUnitOfflineFromMapAsync(unit, "RequestExitGame");
        }
    }
}

