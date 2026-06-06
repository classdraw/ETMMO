using System;

namespace ET.Server
{
	[MessageLocationHandler(SceneType.Map)]
	public class C2M_TransferMapHandler : MessageLocationHandler<Unit, C2M_TransferMap, M2C_TransferMap>
	{
		protected override async ETTask Run(Unit unit, C2M_TransferMap request, M2C_TransferMap response)
		{
			/**
			string currentMap = unit.Scene().Name;
			string toMap = null;
			if (currentMap == "Map1")
			{
				toMap = "Map2";
			}
			else
			{
				toMap = "Map1";
			}
			
			StartSceneConfig startSceneConfig = StartSceneConfigCategory.Instance.GetBySceneName(unit.Fiber().Zone, toMap);
			
			TransferHelper.TransferAtFrameFinish(unit, startSceneConfig.ActorId, toMap,0,false).Coroutine();
			 */
			(int errno, ActorId mapActorId) r = await MapManagerHelper.GetMapActorId(unit.Scene(), request.MapConfigId, request.MapFiberId);
			if (r.errno != ErrorCode.ERR_Success)
			{
				response.Error = r.errno;
				return;
			}

			TransferHelper.TransferAtFrameFinish(unit, r.mapActorId,request.MapConfigId,false).Coroutine();
			await ETTask.CompletedTask;
		}
	}
}