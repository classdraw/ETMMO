namespace ET.Server
{
    [MessageHandler(SceneType.Mail)]
    public class C2Mail_GMAddMailHandler: MessageHandler<MailUnit, C2Mail_GMAddMail, Mail2C_GMAddMail>
    {
        protected override async ETTask Run(MailUnit mailUnit, C2Mail_GMAddMail request, Mail2C_GMAddMail response)
        {
            response.Error = await mailUnit.Root().GetComponent<MailCenterComponent>().SendMail(mailUnit.Id, request.ConfigId);
            /** 离线会存 这个不存了
            using (await mailUnit.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.OperateEmail, mailUnit.Id))
            {
                mailUnit.GetComponent<MailComponent>().BeginInit();
                await mailUnit.Root().GetComponent<DBManagerComponent>().GetZoneDB(mailUnit.Zone()).Save(mailUnit.GetComponent<MailComponent>());
            }*/
        }
    }
}

