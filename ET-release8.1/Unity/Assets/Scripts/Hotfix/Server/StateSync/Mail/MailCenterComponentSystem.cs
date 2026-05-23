using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(MailCenterComponent))]
    [FriendOfAttribute(typeof(ET.MailInfo))]
    [FriendOfAttribute(typeof(ET.Server.MailComponent))]
    public static partial class MailCenterComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.MailCenterComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.MailCenterComponent self)
        {
        }
        
        //发送邮件
        public static async ETTask<int> SendMail(this MailCenterComponent self, long receiverId, string title, string content,
        List<ItemProto> rewardList)
        {
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.OperateEmail, receiverId))
            {
                var mailUnitsComponent = self.Root().GetComponent<MailUnitsComponent>();
                //在线
                if (mailUnitsComponent.Children.TryGetValue(receiverId, out var mailUnitRef))
                {
                    MailUnit receiver = (MailUnit)mailUnitRef;
                    var mailComponent = receiver.GetComponent<MailComponent>();
                    MailInfo mailInfo = mailComponent.AddChild<MailInfo>();
                    mailInfo.Title = title;
                    mailInfo.Message = content;
                    foreach (ItemProto itemProto in rewardList)
                    {
                        Item item = mailInfo.AddChild<Item, int>(itemProto.ConfigId);
                        item.Count = itemProto.Count;
                        item.FromMessage(itemProto);
                        mailInfo.RewardList.Add(item);
                    }

                    mailComponent.MailInfosList.Add(mailInfo);

                    Mail2C_NewMail mail2CNewMail = Mail2C_NewMail.Create();
                    mail2CNewMail.MailInfo = mailInfo.ToMessage();
                    MailHelper.SendToClient(receiver, mail2CNewMail);
                }
                //离线
                else
                {
                    MailUnit mailUnit = mailUnitsComponent.AddChildWithId<MailUnit>(receiverId);
                    MailComponent mailComponent =
                            await self.Root().GetComponent<DBManagerComponent>().GetZoneDB(self.Root().Zone()).Query<MailComponent>(receiverId);

                    if (mailComponent == null)
                    {
                        mailComponent = mailUnit.AddComponent<MailComponent>();
                    }
                    else
                    {
                        mailUnit.AddComponent(mailComponent);
                    }

                    MailInfo mailInfo = mailComponent.AddChild<MailInfo>();
                    mailInfo.Title = title;
                    mailInfo.Message = content;
                    foreach (ItemProto itemProto in rewardList)
                    {
                        Item item = mailInfo.AddChild<Item, int>(itemProto.ConfigId);
                        item.Count = itemProto.Count;
                        item.FromMessage(itemProto);
                        mailInfo.RewardList.Add(item);
                    }

                    mailComponent.MailInfosList.Add(mailInfo);
                    mailInfo.BeginInit();
                    mailComponent.BeginInit();
                    await self.Root().GetComponent<DBManagerComponent>().GetZoneDB(self.Zone()).Save(mailComponent);
                    mailUnit?.Dispose();
                }
            }

            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }
    }
}

