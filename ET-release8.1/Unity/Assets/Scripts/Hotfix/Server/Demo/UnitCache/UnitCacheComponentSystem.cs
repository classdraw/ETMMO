using System;
using System.Collections.Generic;

namespace ET.Server
{
    [EntitySystemOf(typeof(UnitCacheComponent))]
    [FriendOf(typeof(UnitCacheComponent))]
    [FriendOf(typeof(UnitCache))]
    public static partial class UnitCacheComponentSystem
    {
        [EntitySystem]
        private static void Awake(this UnitCacheComponent self)
        {
            self.UnitCacheKeyList.Clear();
            foreach (Type type in CodeTypes.Instance.GetTypes().Values)
            {
                //集成IUnitCache，并且不是IUnitCache
                if (type!=typeof(IUnitCache)&&typeof(IUnitCache).IsAssignableFrom(type))
                {
                    self.UnitCacheKeyList.Add(type.FullName);
                }
            }

            foreach (var key in self.UnitCacheKeyList)
            {
                UnitCache unitCache = self.AddChild<UnitCache>();
                unitCache.key = key;
                self.UnitCaches.Add(key,unitCache);
            }
        }
        
        [EntitySystem]
        private static void Destroy(this UnitCacheComponent self)
        {
            foreach (var unitCacheRef in self.UnitCaches.Values)
            {
                UnitCache unitCache = unitCacheRef;
                unitCache?.Dispose();
            }
            self.UnitCaches.Clear();
            self.UnitCacheKeyList.Clear();
        }
        
        public static async ETTask<Entity> Get(this UnitCacheComponent self,long unitId,string key)
        {
            UnitCache unitCache = null;
            if (!self.UnitCaches.TryGetValue(key,out EntityRef<UnitCache> unitCacheRef))
            {
                unitCache = self.AddChild<UnitCache>();
                unitCache.key = key;
                self.UnitCaches.Add(key,unitCache);
            }
            else
            {
                unitCache = unitCacheRef;
            }

            return await unitCache.Get(unitId);
        }
        
        public static async ETTask Delete(this UnitCacheComponent self,long unitId)
        {
            using (await self.Root().GetComponent<CoroutineLockComponent>().Wait(CoroutineLockType.UnitCacheGet,unitId))
            {
                foreach (UnitCache unitCache in self.UnitCaches.Values)
                {
                    unitCache.Delete(unitId);
                }
            }
        }
        
        public static async ETTask AddOrUpdate(this UnitCacheComponent self,long unitId,List<Entity> entitList)
        {
            using (ListComponent<Entity> list=ListComponent<Entity>.Create())
            {
                foreach (var entity in entitList)
                {
                    string key = entity.GetType().FullName;
                    UnitCache unitCache = null;
                    if (!self.UnitCaches.TryGetValue(key,out EntityRef<UnitCache> unitCacheRef))
                    {
                        unitCache = self.AddChild<UnitCache>();
                        unitCache.key = key;
                        self.UnitCaches.Add(key,unitCache);
                    }
                    else
                    {
                        unitCache = unitCacheRef;
                    }
                    
                    unitCache.AddOrUpdate(entity);
                    list.Add(entity);
                }


                if (list.Count>0)
                {
                    await self.Root().GetComponent<DBManagerComponent>().GetZoneDB(self.Zone()).Save(unitId, list);
                }

                await ETTask.CompletedTask;
            }
        }
    }
}

