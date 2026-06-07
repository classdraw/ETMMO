namespace ET.Server
{
    [MessageHandler(SceneType.Mail)]
    public class G2Mail_LoginMailServerHandler : MessageHandler<Scene,G2Mail_LoginMailServer,Mail2G_LoginMailServer>
    {
        protected override async ETTask Run(Scene root, G2Mail_LoginMailServer request, Mail2G_LoginMailServer response)
        {
            await ETTask.CompletedTask;

            MailUnitsComponent mailUnitsComponent = root.GetComponent<MailUnitsComponent>();
            using (await root.GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.OperateEmail,request.UnitId))
            {
                mailUnitsComponent.Children.TryGetValue(request.UnitId, out var mail);
                MailUnit mailUnit = (MailUnit)mail;
                if (mailUnit != null)
                {
                    return;
                }

                mailUnit = mailUnitsComponent.AddChildWithId<MailUnit>(request.UnitId);
                mailUnit.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.OrderedMessage);

                MailComponent mailComponent = await root.GetComponent<DBManagerComponent>().GetZoneDB(root.Zone()).Query<MailComponent>(request.UnitId);

                if (mailComponent == null)
                {
                    mailUnit.AddComponent<MailComponent>();
                }
                else
                {
                    mailUnit.AddComponent(mailComponent);
                }

                await mailUnit.AddLocation(LocationType.Mail);
            }
            
            
        }
    }
}