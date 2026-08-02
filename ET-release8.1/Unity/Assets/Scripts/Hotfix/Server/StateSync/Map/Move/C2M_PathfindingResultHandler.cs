
namespace ET.Server
{
	[MessageLocationHandler(SceneType.Map)]
	public class C2M_PathfindingResultHandler : MessageLocationHandler<Unit, C2M_PathfindingResult>
	{
		protected override async ETTask Run(Unit unit, C2M_PathfindingResult message)
		{
			int breakErr = unit.TryBreakCastingBeforeCast();
			if (breakErr != ErrorCode.ERR_Success)
			{
				Log.Console($"[Move] 玩家 {unit.Id} 移动被拒绝，Error={breakErr}");
				unit.SendStop(breakErr);
				return;
			}

			NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
			if (numericComponent==null||numericComponent[NumericType.ForbidMove]>0)
			{
				//某些状态禁止移动
				return;
			}

			unit.FindPathMoveToAsync(message.Position).Coroutine();
			await ETTask.CompletedTask;
		}
	}
}