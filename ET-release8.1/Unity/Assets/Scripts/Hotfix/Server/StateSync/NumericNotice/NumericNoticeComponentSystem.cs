namespace ET.Server
{
    [Invoke(TimerInvokeType.NumericSync)]
    public class NumericSyncTimerHandler : ATimer<NumericNoticeComponent>
    {
        protected override void Run(NumericNoticeComponent self)
        {
            self?.NoticeQueueMsgImmediately();
        }
    }

    
    [EntitySystemOf(typeof(NumericNoticeComponent))]
    [FriendOfAttribute(typeof(ET.Server.NumericNoticeComponent))]
    public static partial class NumericNoticeComponentSystem
    {
    
        [EntitySystem]
        private static void Awake(this ET.Server.NumericNoticeComponent self)
        {

        }

        [EntitySystem]
        private static void Destroy(this ET.Server.NumericNoticeComponent self)
        {
            self.Root().GetComponent<TimerComponent>().Remove(ref self.SyncTimeId);
            self.LastSyncTime = 0;
            self.SyncTime = 0;
            for (var i = 0; i < self.QueueMessage.Count; i++)
            {
                M2C_NoticeNumericMsg queueMsg = (M2C_NoticeNumericMsg)self.QueueMessage.Dequeue();
                queueMsg?.Dispose();
            }
            foreach (M2C_NoticeNumericMsg m2CNoticeNumericMsg in self.OutPutMessageDict.Values)
            {
                m2CNoticeNumericMsg?.Dispose();
            }
            
            self.OutPutMessageDict.Clear();
            self.QueueMessage.Clear();
            self.QueueMessage = default;
            self.OutPutMessageDict = default;
        }
        
        
        public static void Notice(this NumericNoticeComponent self, int numericType, long newValue)
        {
            if (self.LastSyncTime > 0 && TimeInfo.Instance.ServerNow() - self.LastSyncTime < 100)
            {
                self.AddQueueMessage(numericType, newValue);
                self.CheckSyncTimer();
            }
            else
            {
                self.NoticeImmediately(numericType,newValue);
            }
        }
        
        public static void NoticeImmediately(this NumericNoticeComponent self, int numericType, long newValue)
        {
            Unit unit = self.GetParent<Unit>();
            M2C_NoticeUnitNumeric singleNumericMessage = M2C_NoticeUnitNumeric.Create();
            singleNumericMessage.UnitId = unit.Id;
            singleNumericMessage.NumericType = numericType;
            singleNumericMessage.NewValue = newValue;

            self.LastSyncTime = TimeInfo.Instance.ServerNow();
            
            MapMessageHelper.SendClient(unit,singleNumericMessage,NoticeClientType.Broadcast);

        }
        public static void AddQueueMessage(this NumericNoticeComponent self, int numericType, long newValue)
        {
            if (self.OutPutMessageDict.TryGetValue(numericType, out M2C_NoticeNumericMsg message))
            {
                message.NewValue = newValue;
            }
            else
            {
                message = M2C_NoticeNumericMsg.Create(true);
                message.NumericType = numericType;
                message.NewValue = newValue;
                self.OutPutMessageDict.Add(numericType,message);
                self.QueueMessage.Enqueue(message);
            }
        }
        
        public static void CheckSyncTimer(this NumericNoticeComponent self)
        {
            if (self.SyncTime <TimeInfo.Instance.ServerNow())
            {
                if (self.SyncTimeId != 0)
                {
                    self.Root().GetComponent<TimerComponent>().Remove(ref self.SyncTimeId);
                
                }

                self.SyncTime = TimeInfo.Instance.ServerNow() + 100;
                self.SyncTimeId = self.Root().GetComponent<TimerComponent>().NewOnceTimer(self.SyncTime, TimerInvokeType.NumericSync, self);
            }
        }
        
        public static void NoticeQueueMsgImmediately(this NumericNoticeComponent self)
        {
            int queueMsgNum = self.QueueMessage.Count;
            if (queueMsgNum <= 0)
            {
                return;
            }

            Unit unit = self.GetParent<Unit>();
            self.OutPutMessageDict.Clear();
            
            M2C_NoticeUnitNumericList MultiNumericMessage = M2C_NoticeUnitNumericList.Create();
            MultiNumericMessage.UnitId = unit.Id;

            int messageCount = self.QueueMessage.Count;
            for (int i = 0; i < messageCount; i++)
            {
                M2C_NoticeNumericMsg queueMsg = (M2C_NoticeNumericMsg)self.QueueMessage.Dequeue();
                MultiNumericMessage.NumericTypeList.Add(queueMsg.NumericType);
                MultiNumericMessage.NewValueList.Add(queueMsg.NewValue);
                queueMsg?.Dispose();
            }

            self.LastSyncTime = TimeInfo.Instance.ServerNow();
            MapMessageHelper.SendClient(unit,MultiNumericMessage,NoticeClientType.Broadcast);
        }
    }
}

