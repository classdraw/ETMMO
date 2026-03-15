namespace ET.Server
{
    [EntitySystemOf(typeof(AccountSessionsComponent))]
    [FriendOf(typeof(AccountSessionsComponent))]
    public static partial class AccountSessionsComponentSystem
    {
        [EntitySystem]
        private static void Awake(this AccountSessionsComponent self)
        {
            
            
        }
        
        [EntitySystem]
        private static void Destroy(this AccountSessionsComponent self)
        {
            self.AccountSessionDictionary.Clear();
        }

        public static Session Get(this AccountSessionsComponent self,string accountName)
        {
            if (!self.AccountSessionDictionary.TryGetValue(accountName, out EntityRef<Session> session))
            {
                return null;
            }

            return session;
        }

        public static bool Add(this AccountSessionsComponent self,string accountName,EntityRef<Session> session)
        {
            if (!self.AccountSessionDictionary.ContainsKey(accountName))
            {
                self.AccountSessionDictionary.Add(accountName,session);
            }
            else
            {
                self.AccountSessionDictionary[accountName] = session;
            }

            return true;
        }
        
        public static bool Remove(this AccountSessionsComponent self,string accountName)
        {
            if (self.AccountSessionDictionary.ContainsKey(accountName))
            {
                self.AccountSessionDictionary.Remove(accountName);
                return true;
            }

            return false;
        }
    }
    
}
