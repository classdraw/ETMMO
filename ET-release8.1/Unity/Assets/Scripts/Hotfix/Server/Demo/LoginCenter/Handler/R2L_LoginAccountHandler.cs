namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class R2L_LoginAccountHandler : MessageHandler<Scene, R2L_LoginAccount, L2R_LoginAccount>
    {
        protected override async ETTask Run(Scene root, R2L_LoginAccount request, L2R_LoginAccount response)
        {
            long accountId = request.AccountName.GetLongHashCode();
            CoroutineLockComponent coroutineLockComponent = root.GetComponent<CoroutineLockComponent>();
            using (await coroutineLockComponent.Wait(CoroutineLockType.LoginCenterLock,accountId))
            {
                if (!root.GetComponent<LoginInfoRecordComponent>().IsExist(accountId))
                {
                    return;
                }
                //如果存在表示要踢那个人下线
                int zone = root.GetComponent<LoginInfoRecordComponent>().Get(accountId);
                StartSceneConfig gateConfig = RealmGateAddressHelper.GetGate(zone, request.AccountName);
                
                L2G_DisConnectGateUnit l2GDisConnectGateUnitRequest = L2G_DisConnectGateUnit.Create();
                l2GDisConnectGateUnitRequest.AccountName = request.AccountName;
                
                var g2LDisConnectGateUnitResponse=(G2L_DisConnectGateUnit)await root.GetComponent<MessageSender>().Call(gateConfig.ActorId, l2GDisConnectGateUnitRequest);
                response.Error = g2LDisConnectGateUnitResponse.Error;
            }
            
        }
    }
}
