
namespace ET.Server
{
    [EntitySystemOf(typeof(TokenComponent))]
    [FriendOf(typeof(TokenComponent))]
    public static partial class TokenComponentSystem
    {
        [EntitySystem]
        private static void Awake(this TokenComponent self)
        {
            
            
        }
        [EntitySystem]
        private static void Destroy(this TokenComponent self)
        {
            
            
        }
        
        public static string Get(this TokenComponent self,string accountName)
        {
            string value = string.Empty;
            self.AccountTokenDictionary.TryGetValue(accountName, out value);
            return value;
        }

        public static bool Add(this TokenComponent self,string accountName,string token)
        {
            self.AccountTokenDictionary.Add(accountName,token);
            self.TimeOutRemoveKey(accountName,token).Coroutine();
            return true;
        }
        
        public static bool Remove(this TokenComponent self,string accountName)
        {
            if (self.AccountTokenDictionary.ContainsKey(accountName))
            {
                self.AccountTokenDictionary.Remove(accountName);
                return true;
            }

            return false;
        }
        
        //token超时 每加一个token启动一个携程
        private static async ETTask TimeOutRemoveKey(this TokenComponent self, string key, string tokenKey)
        {
            await self.Root().GetComponent<TimerComponent>().WaitAsync(600000);
            string onlineToken = self.Get(key);
            if (!string.IsNullOrEmpty(onlineToken)&&onlineToken==tokenKey)
            {
                self.Remove(key);
            }

        }
    }
}

