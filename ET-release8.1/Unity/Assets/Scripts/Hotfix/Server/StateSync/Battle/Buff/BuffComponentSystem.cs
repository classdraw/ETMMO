using System;

namespace ET.Server
{
    [EntitySystemOf(typeof(BuffComponent))]
    [FriendOf(typeof(BuffComponent))]
    [FriendOf(typeof(Buff))]
    [FriendOf(typeof(BuffCreateInfo))]
    public static partial class BuffComponentSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.BuffComponent self)
        {
            self.AddComponent<BuffTempComponent>();
        }
        [EntitySystem]
        private static void Destroy(this ET.Server.BuffComponent self)
        {
            self.BuffsDict.Clear();
        }
        
        [EntitySystem]
        private static void Deserialize(this ET.Server.BuffComponent self)
        {
            self.BuffsDict.Clear();
            self.AddComponent<BuffTempComponent>();
            foreach (Entity entity in self.Children.Values)
            {
                if (entity is Buff buff)
                {
                    self.BuffsDict.Add(buff.ConfigId,buff);
                }
                
            }
        }

        public static BuffCreateInfo Create(this ET.Server.BuffComponent self,int configId)
        {
            BuffCreateInfo buffCreateInfo = self.GetComponent<BuffTempComponent>().AddChild<BuffCreateInfo, int>(configId);
            return buffCreateInfo;
        }
        
        public static bool CreateAddAdd(this ET.Server.BuffComponent self,int configId)
        {
            using (BuffCreateInfo buffCreateInfo =self.Create(configId))
            {
                return self.Add(buffCreateInfo);
            }
        }

        public static bool Add(this ET.Server.BuffComponent self,BuffCreateInfo buffCreateInfo)
        {
            if (buffCreateInfo==null||buffCreateInfo.IsDisposed)
            {
                return false;
            }

            if (self==null||self.IsDisposed)
            {
                return false;
            }

            Buff buff = self.AddChild<Buff, int>(buffCreateInfo.ConfigId);
            Unit owner = self.GetParent<Unit>();
            if (owner==null)//unit不存在非法
            {
                buff.Dispose();
                return false;
            }
            buff.Owner = owner;
            int configId = buff.ConfigId;
            //buff替换 迭代逻辑
            //jyytest
            return true;
        }


        public static void Remove(this BuffComponent self,int buffId)
        {
            if (!self.Children.TryGetValue(buffId,out Entity entity))
            {
                return;
            }
            Buff buff=entity as Buff;
            try
            {
                self.BuffsDict.Remove(buff.ConfigId);
                buff.Dispose();
            }
            catch(Exception ex)
            {
                Log.Error($"Remove Buff {buffId} Error!!!");
            }
        }
        
        
    }
}

