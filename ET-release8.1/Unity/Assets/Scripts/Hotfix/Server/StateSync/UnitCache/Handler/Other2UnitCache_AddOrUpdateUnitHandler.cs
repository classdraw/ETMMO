namespace ET.Server
{
    [MessageHandler(SceneType.UnitCache)]
    public class Other2UnitCache_AddOrUpdateUnitHandler : MessageHandler<Scene,Other2UnitCache_AddOrUpdateUnit,UnitCache2Other_AddOrUpdateUnit>
    {
        protected override async ETTask Run(Scene root, Other2UnitCache_AddOrUpdateUnit request, UnitCache2Other_AddOrUpdateUnit response)
        {
            UpdateUnitCacheAsync(root, request, response).Coroutine();
            await ETTask.CompletedTask;
        }

        private async ETTask UpdateUnitCacheAsync(Scene root, Other2UnitCache_AddOrUpdateUnit request, UnitCache2Other_AddOrUpdateUnit response)
        {
            UnitCacheComponent unitCacheComponent = root.GetComponent<UnitCacheComponent>();
            long unitId = request.UnitId;
            using (ListComponent<Entity> entityList = ListComponent<Entity>.Create())
            {
                for (int i=0;i<request.EntityTypes.Count;i++) {
                    Entity entity= MongoHelper.Deserialize<Entity>(request.EntityBytes[i]);
                    entityList.Add(entity);
                }
                await unitCacheComponent.AddOrUpdate(unitId, entityList);
            }
        }
    }
}

