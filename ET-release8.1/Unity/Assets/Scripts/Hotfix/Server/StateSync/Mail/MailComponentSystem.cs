namespace ET.Server
{
    [EntitySystemOf(typeof(MailComponent))]
    [FriendOfAttribute(typeof(ET.Server.MailComponent))]
    [FriendOfAttribute(typeof(ET.MailInfo))]
    public static partial class MailComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.MailComponent self)
        {
            
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.MailComponent self)
        {

            foreach (MailInfo mailInfo in self.MailInfosList)
            {
                mailInfo?.Dispose();
            }
            self.MailInfosList.Clear();

        }
        [EntitySystem]
        private static void Deserialize(this ET.Server.MailComponent self)
        {
            foreach (Entity childrenValue in self.Children.Values)
            {
                if (childrenValue is MailInfo mailInfoEntity)
                {
                    self.MailInfosList.Add(mailInfoEntity);
                }
            }
        }

        public static async ETTask<int> CollectAttachment(this ET.Server.MailComponent self, long mailId)
        {
            MailUnit unit = (MailUnit)self.Parent;
            MailInfo mail = null;
            foreach (MailInfo mailInfo in self.MailInfosList)
            {
                if (mailInfo.Id == mailId)
                {
                    mail = mailInfo;
                    break;
                }
            }
            if (mail == null)
            {
                return ErrorCode.ERR_MailNotExist;
            }
            if (mail.IsCollected)
            {
                return ErrorCode.ERR_MailCollected;
            }
            
            Mail2M_CollectAttachment mail2MCollectAttachment = Mail2M_CollectAttachment.Create();
            foreach (Item item in mail.RewardList)
            {
                mail2MCollectAttachment.AttachItems.Add(item.ToMessage());
            }
            
            M2Mail_CollectAttachment m2MailCollectAttachment =
                    (M2Mail_CollectAttachment)await self.Root()
                            .GetComponent<MessageLocationSenderComponent>()
                            .Get(LocationType.Unit)
                            .Call(unit.Id, mail2MCollectAttachment);

            if (m2MailCollectAttachment.Error == ErrorCode.ERR_Success)
            {
                mail.IsCollected = true;
                Mail2C_UpdateMailInfo mail2CUpdateMailInfo = Mail2C_UpdateMailInfo.Create();
                mail2CUpdateMailInfo.MailInfo = mail.ToMessage();
                MailHelper.SendToClient(unit, mail2CUpdateMailInfo);
            }
            return m2MailCollectAttachment.Error;
        }


        public static void ReadMail(this MailComponent self,long mailId)
        {
            MailUnit unit = (MailUnit)self.Parent;
            MailInfo mail = null;
            foreach (MailInfo mailInfo in self.MailInfosList)
            {
                if (mailInfo.Id == mailId)
                {
                    mail = mailInfo;
                    break;
                }
            }

            if (mail != null)
            {
                mail.IsRead = true;
                Mail2C_UpdateMailInfo mail2CUpdateMailInfo = Mail2C_UpdateMailInfo.Create();
                mail2CUpdateMailInfo.MailInfo = mail.ToMessage();
                MailHelper.SendToClient(unit,mail2CUpdateMailInfo);
            }
        }
    }
}

