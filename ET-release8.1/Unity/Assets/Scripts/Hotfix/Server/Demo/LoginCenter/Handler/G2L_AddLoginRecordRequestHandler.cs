namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class G2L_AddLoginRecordRequestHandler : MessageHandler<Scene, G2L_AddLoginRecord, L2G_AddLoginRecord>
    {
        protected override async ETTask Run(Scene root, G2L_AddLoginRecord request, L2G_AddLoginRecord response)
        {
            var loginInfoRecordComponent=root.GetComponent<LoginInfoRecordComponent>();
            var accountId = request.AccountName.GetLongHashCode();
            loginInfoRecordComponent.Remove(accountId);
            loginInfoRecordComponent.Add(accountId,request.ServerId);
            await ETTask.CompletedTask;
        }
    }
    
}

