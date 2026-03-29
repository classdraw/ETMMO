namespace ET.Server
{
    [MessageHandler(SceneType.LoginCenter)]
    public class G2L_RemoveLoginRecordHandler : MessageHandler<Scene, G2L_RemoveLoginRecord, L2G_RemoveLoginRecord>
    {
        protected override async ETTask Run(Scene root, G2L_RemoveLoginRecord request, L2G_RemoveLoginRecord response)
        {
            var loginInfoRecordComponent=root.GetComponent<LoginInfoRecordComponent>();
            var accountId = request.AccountName.GetLongHashCode();
            //角色可能换区
            if (request.ServerId==loginInfoRecordComponent.Get(accountId))
            {
                loginInfoRecordComponent.Remove(accountId);
            }
            await ETTask.CompletedTask;
        }
    }
}

