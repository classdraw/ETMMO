namespace ET.Server
{
    [MessageHandler(SceneType.Mail)]
    [FriendOfAttribute(typeof(ET.Server.MailComponent))]
    public class C2Mail_GetAllMailListHandler : MessageHandler<MailUnit, C2Mail_GetAllMailList, Mail2C_GetAllMailList>
    {
        protected override async ETTask Run(MailUnit mailUnit, C2Mail_GetAllMailList request, Mail2C_GetAllMailList response)
        {
            await ETTask.CompletedTask;

            MailComponent mailComponent = mailUnit.GetComponent<MailComponent>();
            foreach (var entityRef in mailComponent.MailInfosList)
            {
                MailInfo mailInfo = entityRef;
                response.MailInfoList.Add(mailInfo.ToMessage());
            }

            await ETTask.CompletedTask;
        }
    }
}