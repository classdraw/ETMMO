namespace ET.Server
{
    //信件读取
    [MessageHandler(SceneType.Mail)]
    public class C2Mail_ReadMailHandler: MessageHandler<MailUnit,C2Mail_ReadMail>
    {
        protected override async ETTask Run(MailUnit unit, C2Mail_ReadMail message)
        {
            MailComponent mailComponent = unit.GetComponent<MailComponent>();
            mailComponent.ReadMail(message.MailId);
            await ETTask.CompletedTask;
        }
    }
}

