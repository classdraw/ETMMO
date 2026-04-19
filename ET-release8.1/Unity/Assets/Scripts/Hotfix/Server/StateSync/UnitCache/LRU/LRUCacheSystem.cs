using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(LRUCache))]
    [FriendOfAttribute(typeof(ET.Server.LRUNode))]
    [FriendOfAttribute(typeof(ET.Server.LRUCache))]
    public static partial class LRUCacheSystem
    {
        [EntitySystem]
        private static void Awake(this ET.Server.LRUCache self)
        {
            self.MinFrequency = 0;
            self.FrequencyDict.Add(0, new LinkedList<EntityRef<LRUNode>>());
        }

        [EntitySystem]
        private static void Destroy(this ET.Server.LRUCache self)
        {
            self.LRUNodeDict.Clear();
            self.FrequencyDict.Clear();
            self.MinFrequency = 0;
        }

        public static void Call(this LRUCache self,long key)
        {
            EntityRef<LRUNode> nodeRef=null;
            LRUNode n = null;
            if (self.LRUNodeDict.TryGetValue(key,out nodeRef))
            {
                n = nodeRef;
                self.FrequencyDict[n.Frequency].Remove(n);
                n.Frequency++;
                if (!self.FrequencyDict.ContainsKey(n.Frequency))
                {
                    self.FrequencyDict.Add(n.Frequency,new LinkedList<EntityRef<LRUNode>>());
                }

                self.FrequencyDict[n.Frequency].AddLast(n);
                if (self.FrequencyDict[self.MinFrequency].Count==0)
                {
                    self.MinFrequency = n.Frequency;
                }
            }
            else
            {
                n = self.AddChild<LRUNode, long>(key);
                n.Frequency = 0;
                self.FrequencyDict[0].AddLast(n);
                self.MinFrequency = 0;
                self.LRUNodeDict[key] = n;

                if (self.LRUNodeDict.Count>=3000)
                {
                    LRUNode fn = self.FrequencyDict[self.MinFrequency].First.Value;
                    long unitId = fn.Key;
                    self.FrequencyDict[self.MinFrequency].RemoveFirst();
                    self.LRUNodeDict.Remove(unitId);
                    fn?.Dispose();
                    
                    EventSystem.Instance.Invoke((long)SceneType.UnitCache, new LRUUnitCacheDelete() { LRUCache = self, Key = unitId });
                }


            }
        }
    }
    
}
