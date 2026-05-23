namespace ET
{
    [EntitySystemOf(typeof(MailInfo))]
    [FriendOfAttribute(typeof(ET.MailInfo))]
    public static partial class MailInfoSystem
    {
        [EntitySystem]
        private static void Awake(this MailInfo self)
        {

        }
        [EntitySystem]
        private static void Destroy(this MailInfo self)
        {
            self.ConfigId = default;
            self.Message = default;
            self.Title = default;
        }
        
        public static void FromMessage(this MailInfo self, MailInfoProto mailInfoProto)
        {
            self.ConfigId = mailInfoProto.ConfigId;
            self.Title = mailInfoProto.Title;
            self.Message = mailInfoProto.Message;
            foreach (ItemProto itemProto in mailInfoProto.RewardList)
            {
                Item item = self.AddChildWithId<Item,int>(itemProto.Id,itemProto.ConfigId);
                item.FromMessage(itemProto);
                self.RewardList.Add(item);
            }

            self.IsRead = mailInfoProto.IsRead;
            self.IsCollected = mailInfoProto.IsCollected;
        }
        
        public static MailInfoProto ToMessage(this MailInfo self)
        {
            MailInfoProto mailInfoProto = MailInfoProto.Create();
            mailInfoProto.ConfigId = self.ConfigId;
            mailInfoProto.Title = self.Title;
            mailInfoProto.Message = self.Message;
            mailInfoProto.MailId = self.Id;
            foreach (Item item in self.RewardList)
            {
                mailInfoProto.RewardList.Add(item.ToMessage());
            }

            mailInfoProto.IsCollected = self.IsCollected;
            mailInfoProto.IsRead = self.IsRead;
            return mailInfoProto;
        }
        
        [EntitySystem]
        private static void Deserialize(this ET.MailInfo self)
        {
            foreach (Entity childrenValue in self.Children.Values)
            {
                if (childrenValue is Item item)
                {
                    self.RewardList.Add(item);
                }
            }
        }
    }
    
}
