using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(AccountChectOutTimeComponent))]
    [FriendOf(typeof(AccountChectOutTimeComponent))]
    public static partial class AccountChectOutTimeComponentSystem
    {
        [Invoke(TimerInvokeType.AccountChectOutTimer)]
        public class AccountChectOutTimerHandler: ATimer<AccountChectOutTimeComponent>
        {
            protected override void Run(AccountChectOutTimeComponent self)
            {
                try
                {
                    self?.DeleteSession();
                }
                catch (Exception e)
                {
                    Log.Error($"move timer error: {self.Id}\n{e}");
                }
            }
        }
        //10分钟之后定时器断开连接
        [EntitySystem]
        private static void Awake(this AccountChectOutTimeComponent self,string accountName)
        {
            self.AccountName = accountName;
            
            if(self.Timer!=0)
                self.Root().GetComponent<TimerComponent>()?.Remove(ref self.Timer);
            
            self.Timer = self.Root().GetComponent<TimerComponent>().NewOnceTimer(TimeInfo.Instance.ServerNow() + 600000, TimerInvokeType.AccountChectOutTimer, self);
        }
        
        [EntitySystem]
        private static void Destroy(this AccountChectOutTimeComponent self)
        {
            self.Root().GetComponent<TimerComponent>()?.Remove(ref self.Timer);
        }

        public static void DeleteSession(this AccountChectOutTimeComponent self)
        {
            Session session = self.GetParent<Session>();
            Session originSession = session.Root().GetComponent<AccountSessionsComponent>().Get(self.AccountName);
            if (originSession!=null&&session.InstanceId==originSession.InstanceId)
            {
                session.Root().GetComponent<AccountSessionsComponent>().Remove(self.AccountName);
            }

            //断开连接 太久没有操作
            var a2CDisconnet= A2C_Disconnet.Create();
            a2CDisconnet.Error = 1;//0重复登陆 1超时 2顶号
            session?.Send(a2CDisconnet);
            session?.Disconnect().Coroutine();
        }
    }
    
}

