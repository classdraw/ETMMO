using ET.Client;

namespace ET.Server
{
    [Event(SceneType.All)]
    public class UnitGetComponent_GetComponent: AEvent<Scene,UnitGetComponent>
    {
        protected override async ETTask Run(Scene scene, UnitGetComponent args)
        {
            Unit unit = args.Unit;
            System.Type type = args.Type;
            unit.GetComponent<UnitDBSaveComponent>()?.AddChange(type);
            
            if (unit.Components.ContainsKey(type.TypeHandle.Value.ToInt64()))
            {
                return;
            }
            
            UnitDBSaveComponent unitDBSaveComponent = unit.GetComponent<UnitDBSaveComponent>();

            if (unitDBSaveComponent == null)
            {
                return;
            }
            
            //没有这个组件那么需要反序列化挂到自己身上
            if (!unit.GetComponent<UnitDBSaveComponent>().Bytes.TryGetValue(type, out byte[] bs))
            {
                return;
            }

            if (bs == null)
            {
                return;
            }
            //延迟组件反序列化的时机，玩家有用到对应组件再对需要的组件反序列化，降低cpu消耗尖端 
            Entity t = MongoHelper.Deserialize(type,bs) as Entity;
            //不存在需要自己添加进去
            unit.AddComponent(t);
            await ETTask.CompletedTask;
            
            
        }
    }
}

