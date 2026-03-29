namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class R2L_LoginAccountRequestHandler : MessageHandler<Scene, R2L_LoginAccountRequest, L2R_LoginAccountResponse>
    {
        protected override async ETTask Run(Scene scene, R2L_LoginAccountRequest request, L2R_LoginAccountResponse response)
        {
            long accountId = request.AccountName.GetLongHashCode();
            CoroutineLockComponent coroutineLockComponent = scene.GetComponent<CoroutineLockComponent>();
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginCenterLock,accountId))
            {
                if (!scene.GetComponent<LoginInfoRecordComponent>().IsExist(accountId))
                {
                    return;
                }
                //如果存在表示要踢那个人下线
                int zone = scene.GetComponent<LoginInfoRecordComponent>().Get(accountId);
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(zone, request.AccountName);
                
                L2G_DisConnectGateUnitRequest l2GDisConnectGateUnitRequest = L2G_DisConnectGateUnitRequest.Create();
                l2GDisConnectGateUnitRequest.AccountName = request.AccountName;
                var g2LDisConnectGateUnitResponse=(G2L_DisConnectGateUnitResponse)await scene.GetComponent<MessageSender>().Call(gateConfig.ActorId, l2GDisConnectGateUnitRequest);
                response.Error = g2LDisConnectGateUnitResponse.Error;
            }
            
        }
    }
}
