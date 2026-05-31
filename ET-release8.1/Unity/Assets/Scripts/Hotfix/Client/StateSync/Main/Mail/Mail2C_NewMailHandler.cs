namespace ET.Client
{
    [MessageHandler(SceneType.All)]
    public class Mail2C_NewMailHandler : MessageHandler<Scene, Mail2C_NewMail>
    {
        protected override async ETTask Run(Scene root, Mail2C_NewMail message)
        {
            MailInfoProto mailInfo = message.MailInfo;
            Log.Info($"Mail2C_NewMail MailId={mailInfo.MailId} ConfigId={mailInfo.ConfigId} Title={mailInfo.Title} RewardCount={mailInfo.RewardList.Count}");
            await ETTask.CompletedTask;
        }
    }
}

