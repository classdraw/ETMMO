namespace ET.Server
{
    [EntitySystemOf(typeof(UnitCache))]
    [FriendOf(typeof(UnitCache))]
    public static partial class UnitCacheSystem
    {
        [EntitySystem]
        private static void Awake(this UnitCache self)
        {
            
        }
        
        [EntitySystem]
        private static void Destroy(this UnitCache self)
        {
            foreach (var entityRef in self.CacheComponentDics.Values)
            {
                Entity entity = entityRef;
                entity?.Dispose();
            }
            self.CacheComponentDics.Clear();
            self.key = null;
        }
        /// <summary>
        /// 得到一个角色缓存数据
        /// </summary>
        public static async ETTask<Entity> Get(this UnitCache self,long unitId)
        {
            Entity entity = null;
            if (!self.CacheComponentDics.TryGetValue(unitId,out EntityRef<Entity> entityRef))
            {
                entity = await self.Root().GetComponent<DBManagerComponent>().GetZoneDB(self.Zone()).Query<Entity>(unitId, self.key);
                if (entity!=null)
                {
                    //更新本地缓存数据
                    self.AddOrUpdate(entity);
                }
            }
            else
            {
                entity = entityRef;
            }

            return entity;
        }
        
        public static void Delete(this UnitCache self,long unitId)
        {

            if (self.CacheComponentDics.TryGetValue(unitId,out EntityRef<Entity> entityRef))
            {
                self.CacheComponentDics.Remove(unitId);
                Entity entity = entityRef;
                entity?.Dispose();
            }

        }

        public static void AddOrUpdate(this UnitCache self,Entity entity)
        {
            if (entity==null)
            {
                return;
            }

            if (self.CacheComponentDics.TryGetValue(entity.Id,out EntityRef<Entity>entityRef))
            {
                Entity oldEntity = entityRef;
                if (oldEntity!=null&&oldEntity!=entity)
                {
                    oldEntity?.Dispose();
                }

                self.CacheComponentDics.Remove(entity.Id);
            }
            self.CacheComponentDics.Add(entity.Id,entity);
        }
    }
}