namespace ET.Server
{
	/// <summary>
	/// Gate 上 Session 销毁时会发 G2M_SessionDisconnect。<see cref="G2M_RequestExitGameHandler"/> 走主动下线；
	/// 掉线/强退只走本处理器，必须同样移除 AOI 与 Unit，否则会一直占格子且其他玩家视野不刷新。
	/// </summary>
	[MessageLocationHandler(SceneType.Map)]
	public class G2M_SessionDisconnectHandler : MessageLocationHandler<Unit, G2M_SessionDisconnect>
	{
		protected override async ETTask Run(Unit unit, G2M_SessionDisconnect message)
		{
			await UnitHelper.ForceUnitOfflineFromMapAsync(unit, "SessionDisconnect");
		}
	}
}