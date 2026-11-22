
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
				MoveLogHelper.LogMoveRejected(unit, breakErr);
				unit.SendStop(breakErr);
				return;
			}

			if (unit.IsForbidMove())
			{
				MoveLogHelper.LogForbidMove(unit);
				return;
			}

			MoveLogHelper.LogPathRequest(unit, message.Position);
			unit.FindPathMoveToAsync(message.Position).Coroutine();
			await ETTask.CompletedTask;
		}
	}
}