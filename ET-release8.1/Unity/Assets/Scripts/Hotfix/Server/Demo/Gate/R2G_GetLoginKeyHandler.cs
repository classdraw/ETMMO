using System;


namespace ET.Server
{
	[MessageHandler(SceneType.Gate)]
	public class R2G_GetLoginKeyHandler : MessageHandler<Scene, R2G_GetLoginKey, G2R_GetLoginKey>
	{
		protected override async ETTask Run(Scene root, R2G_GetLoginKey request, G2R_GetLoginKey response)
		{
			string keyStr = RandomGenerator.RandInt64().ToString() + TimeInfo.Instance.ServerNow().ToString();
			long key =keyStr.GetLongHashCode();
			root.GetComponent<GateSessionKeyComponent>().Add(key, request.Account);
			response.Key = key;
			response.GateId = root.Id;
			await ETTask.CompletedTask;
		}
	}
}