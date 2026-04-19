using System.Collections.Generic;

namespace ET.Server
{
    [ComponentOf(typeof(Unit))]
    public class NumericNoticeComponent :Entity,IAwake,IDestroy
    {
        public Dictionary<int, M2C_NoticeNumericMsg> OutPutMessageDict = new Dictionary<int, M2C_NoticeNumericMsg>();
        public Queue<IMessage> QueueMessage = new Queue<IMessage>();

        public long SyncTime = 0;
        public long SyncTimeId = 0;
        public long LastSyncTime = 0;
    }
}
