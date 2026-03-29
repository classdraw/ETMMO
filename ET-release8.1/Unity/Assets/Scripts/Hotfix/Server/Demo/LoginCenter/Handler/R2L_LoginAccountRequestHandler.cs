namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class R2L_LoginAccountRequestHandler : MessageHandler<Scene, R2L_LoginAccountRequest, L2R_LoginAccountResponse>
    {
        protected override async ETTask Run(Scene scene, R2L_LoginAccountRequest request, L2R_LoginAccountResponse response)
        {
            // 空实现
            response.Error = ErrorCode.ERR_Success;
            await ETTask.CompletedTask;
        }
    }
}
