namespace ET.Server
{
    public static class MailHelper
    {
        public static void SendToClient(MailUnit unit, IMessage message)
        {
            unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Send(unit.Id, message).Coroutine();
        }

        /// <summary>
        /// 登录 Mail 服，创建/挂载玩家邮箱
        /// </summary>
        public static async ETTask<int> LoginMailServer(Scene scene, Unit unit)
        {
            if (unit == null || unit.IsDisposed)
            {
                return ErrorCode.ERR_NonePlayerError;
            }

            if (!StartSceneConfigCategory.Instance.MailConfigs.TryGetValue(scene.Zone(), out StartSceneConfig startSceneConfig))
            {
                Log.Warning($"[Mail] 未配置 Mail 场景 Zone={scene.Zone()}");
                return ErrorCode.ERR_Success;
            }

            G2Mail_LoginMailServer request = G2Mail_LoginMailServer.Create();
            request.UnitId = unit.Id;

            Mail2G_LoginMailServer response = (Mail2G_LoginMailServer)await scene.Root().GetComponent<MessageSender>()
                    .Call(startSceneConfig.ActorId, request);

            return response.Error;
        }
    }
}
