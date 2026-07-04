namespace ET.Client
{
    [MessageHandler(SceneType.All)]
    public class Mail2C_UpdateMailInfoHandler : MessageHandler<Scene, Mail2C_UpdateMailInfo>
    {
        protected override async ETTask Run(Scene root, Mail2C_UpdateMailInfo message)
        {
            MailInfoProto mailInfo = message.MailInfo;
            Log.Info($"Mail2C_UpdateMailInfo MailId={mailInfo.MailId} ConfigId={mailInfo.ConfigId} Title={mailInfo.Title} IsRead={mailInfo.IsRead} IsCollected={mailInfo.IsCollected} RewardCount={mailInfo.RewardList.Count}");
            await ETTask.CompletedTask;
        }
    }
}
