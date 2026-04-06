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
			if (unit == null || unit.IsDisposed)
			{
				return;
			}

			Log.Console($"会话断开，从地图移除角色 roleId:{unit.Id}");
			unit.RemoveComponent<AOIEntity>();
			RemoveUnitAfterDisconnect(unit).Coroutine();
			await ETTask.CompletedTask;
		}

		private static async ETTask RemoveUnitAfterDisconnect(Unit unit)
		{
			await unit.Fiber().WaitFrameFinish();
			await unit.RemoveLocation(LocationType.Unit);
			unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Remove(unit.Id);
			UnitComponent unitComponent = unit.Root().GetComponent<UnitComponent>();
			unitComponent.Remove(unit.Id);
		}
	}
}