using System.Collections.Generic;

namespace ET.Server
{
    [FriendOf(typeof(UnitCacheComponent))]
    [MessageHandler(SceneType.UnitCache)]
    public class Other2UnitCache_GetUnitHandler: MessageHandler<Scene,Other2UnitCache_GetUnit,UnitCache2Other_GetUnit>
    {
        protected override async ETTask Run(Scene root, Other2UnitCache_GetUnit request, UnitCache2Other_GetUnit response)
        {
            UnitCacheComponent unitCacheComponent = root.GetComponent<UnitCacheComponent>();
            Dictionary<string, Entity> dict = ObjectPool.Instance.Fetch(typeof(Dictionary<string, Entity>)) as Dictionary<string, Entity>;

            try
            {
                if (request.ComponentNameList.Count == 0)
                {
                    dict.Add("ET.Unit", null);
                    foreach (string se in unitCacheComponent.UnitCacheKeyList)
                    {
                        if (se == "ET.Unit")
                        {
                            continue;
                        }

                        dict.Add(se, null);
                    }
                }
                else
                {
                    foreach (string se in request.ComponentNameList)
                    {
                        dict.Add(se, null);
                    }
                }

                long unitId = request.UnitId;
                CoroutineLockComponent coroutineLockComponent = root.GetComponent<CoroutineLockComponent>();
                using (await coroutineLockComponent.Wait(CoroutineLockType.UnitCacheGet,unitId))
                {
                    unitCacheComponent.CallCache(unitId);
                    using (ListComponent<string> keyList=ListComponent<string>.Create())
                    {
                        foreach (string key in dict.Keys)
                        {
                            keyList.Add(key);
                        }
                        //这里会去数据库取 如果取不到 那么肯定是新用户
                        foreach (string key in keyList)
                        {
                            Entity entity = await unitCacheComponent.Get(request.UnitId, key);
                            dict[key] = entity;
                        }
                        
                        foreach (var info in dict)
                        {
                            response.ComponentNameList.Add(info.Key);
                            response.EntityList.Add(info.Value?.ToBson() ?? null);
                        }
                        
                    }//using
                }



            }
            finally
            {
                dict.Clear();
                ObjectPool.Instance.Recycle(dict);
            }
        }
    }
}
