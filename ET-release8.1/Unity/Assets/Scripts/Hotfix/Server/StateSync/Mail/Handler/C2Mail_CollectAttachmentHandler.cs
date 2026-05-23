namespace ET.Server
{
    [MessageHandler(SceneType.Mail)]
    public class C2Mail_CollectAttachmentHandler : MessageHandler<MailUnit,C2Mail_CollectAttachment,Mail2C_CollectAttachment>
    {
        protected override async ETTask Run(MailUnit unit, C2Mail_CollectAttachment request, Mail2C_CollectAttachment response)
        {
            await ETTask.CompletedTask;
            MailComponent mailComponent = unit.GetComponent<MailComponent>();
            var errorCode = await mailComponent.CollectAttachment(request.MailId);
            response.Error = errorCode;
        }
    } 
}