using System;

namespace ET.Server
{
    public static class UnitCacheHelper
    {
        public static async ETTask<Unit> GetUnitCache(Scene gateScene,Scene mapScene,long unitId)
        {
            ActorId instanceId = StartSceneConfigCategory.Instance.GetBySceneType(gateScene.Zone(),SceneType.UnitCache).ActorId;
            //得到角色身上有多少个相关的组件数据（数据保存的）
            Other2UnitCache_GetUnit messaage = Other2UnitCache_GetUnit.Create();
            messaage.UnitId = unitId;

            UnitCache2Other_GetUnit unitCache2OtherGetUnit = await gateScene.Root().GetComponent<MessageSender>().Call(instanceId, messaage) as UnitCache2Other_GetUnit;
            if (unitCache2OtherGetUnit.Error!=ErrorCode.ERR_Success||unitCache2OtherGetUnit.EntityList.Count<=0)
            {
                return null;//没有组件 那么啥也不处理
            }

            Unit unit = null;
            int indexOf = unitCache2OtherGetUnit.ComponentNameList.IndexOf("ET.Unit");
            if (indexOf >= 0)
            {
                if (unitCache2OtherGetUnit.EntityList[indexOf] != null)
                {
                    unit = MongoHelper.Deserialize<Entity>(unitCache2OtherGetUnit.EntityList[indexOf]) as Unit;
                }
            }
            
            if (unit == null)
            {
                return null;
            }
            //增加这个实体
            mapScene.GetComponent<UnitComponent>().AddChild(unit);
            if (unit.GetComponent<UnitDBSaveComponent>() == null)
            {
                unit.AddComponent<UnitDBSaveComponent>();
            }

            for (int i = 0; i < unitCache2OtherGetUnit.EntityList.Count; i++)
            {
                if (i == indexOf)
                {
                    continue;
                }

                byte[] entityByte = unitCache2OtherGetUnit.EntityList[i];
                Type type = CodeTypes.Instance.GetType(unitCache2OtherGetUnit.ComponentNameList[i]);
                
                EventSystem.Instance.Invoke((long)SceneType.UnitCache,new AddToBytes()
                {
                    Unit = unit,Type = type,Bytes = entityByte
                });
            }

            return unit;
        }
        
        
        public static void AddOrUpdateUnitAllCache(Unit unit)
        {
            Other2UnitCache_AddOrUpdateUnit message = Other2UnitCache_AddOrUpdateUnit.Create();
            message.UnitId = unit.Id;
            
            message.EntityTypes.Add(unit.GetType().FullName);
            message.EntityBytes.Add(unit.ToBson());
            
            foreach (Entity entity in unit.Components.Values)
            {
                Type type = entity.GetType();
                if (!typeof(IUnitCache).IsAssignableFrom(type))
                {
                    continue;
                }
                
                message.EntityTypes.Add(type.FullName);
                byte[] bytes = entity.ToBson();
                message.EntityBytes.Add(bytes);
                
                EventSystem.Instance.Invoke((long)SceneType.UnitCache,new AddToBytes(){Unit = unit,Type = type,Bytes = bytes});
            }

            StartSceneConfig cfg = StartSceneConfigCategory.Instance.GetBySceneName(unit.Zone(), "UnitCache");
            unit.Root().GetComponent<MessageSender>().Call(cfg.ActorId,message).Coroutine();
        }

    }
}

