namespace ET.Client
{
    public static class MailHelper
    {
        public static async ETTask<int> GMCollectAttachmentMail(Scene root, long mailId)
        {
            C2Mail_CollectAttachment c2MailCollectAttachment = C2Mail_CollectAttachment.Create();
            c2MailCollectAttachment.MailId = mailId;
            root.GetComponent<ClientSenderComponent>().Send(c2MailCollectAttachment);
            Log.Info($"邮件领取 mailId={mailId}");
            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }

        
        
        public static async ETTask<int> GMReadMail(Scene root, long mailId)
        {
            C2Mail_ReadMail c2MailReadMail = C2Mail_ReadMail.Create();
            c2MailReadMail.MailId = mailId;
            root.GetComponent<ClientSenderComponent>().Send(c2MailReadMail);
            Log.Info($"邮件读取 mailId={mailId}");
            await ETTask.CompletedTask;
            return ErrorCode.ERR_Success;
        }
        
        public static async ETTask<int> GMAddMail(Scene root,int configId)
        {
            C2Mail_GMAddMail c2MailGmAddMail = C2Mail_GMAddMail.Create();
            c2MailGmAddMail.ConfigId = configId;
            Mail2C_GMAddMail g2CGmAddMail=await root.GetComponent<ClientSenderComponent>().Call(c2MailGmAddMail) as Mail2C_GMAddMail;
            if (g2CGmAddMail.Error != ErrorCode.ERR_Success) 
            {
                return g2CGmAddMail.Error;
            }
            Log.Console("发送测试邮件成功!!!");
            return g2CGmAddMail.Error;
        }

        public static async ETTask<int> GMGetMail(Scene root)
        {
            C2Mail_GetAllMailList c2MailGetAllMailList = C2Mail_GetAllMailList.Create();
            Mail2C_GetAllMailList mail2CGetAllMailList=await root.GetComponent<ClientSenderComponent>().Call(c2MailGetAllMailList) as Mail2C_GetAllMailList;
            if (mail2CGetAllMailList.Error != ErrorCode.ERR_Success) 
            {
                return mail2CGetAllMailList.Error;
            }

            Log.Info($"Mail GetAllMail count={mail2CGetAllMailList.MailInfoList.Count}");
            foreach (MailInfoProto mailInfo in mail2CGetAllMailList.MailInfoList)
            {
                Log.Info($"Mail MailId={mailInfo.MailId} ConfigId={mailInfo.ConfigId} Title={mailInfo.Title} Message={mailInfo.Message} IsRead={mailInfo.IsRead} IsCollected={mailInfo.IsCollected} RewardCount={mailInfo.RewardList.Count}");
                foreach (ItemProto itemProto in mailInfo.RewardList)
                {
                    Log.Info($"Mail Reward ItemId={itemProto.Id} ConfigId={itemProto.ConfigId} Count={itemProto.Count}");
                }
            }

            return mail2CGetAllMailList.Error;
        }
    }
}

