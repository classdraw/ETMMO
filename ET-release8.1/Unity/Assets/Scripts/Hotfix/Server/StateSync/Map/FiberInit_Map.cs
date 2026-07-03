using System.Net;

namespace ET.Server
{
    /// <summary>
    /// 地图场景 实体表演的场景
    /// </summary>
    [Invoke((long)SceneType.Map)]
    public class FiberInit_Map: AInvokeHandler<FiberInit, ETTask>
    {
        public override async ETTask Handle(FiberInit fiberInit)
        {
            Scene root = fiberInit.Fiber.Root;
            root.AddComponent<MailBoxComponent, MailBoxType>(MailBoxType.UnOrderedMessage);
            root.AddComponent<TimerComponent>();
            root.AddComponent<CoroutineLockComponent>();
            root.AddComponent<ProcessInnerSender>();
            root.AddComponent<DBManagerComponent>();
            root.AddComponent<MessageSender>();
            root.AddComponent<LocationProxyComponent>();
            root.AddComponent<MessageLocationSenderComponent>();
            
            root.AddComponent<UnitComponent>();
            root.AddComponent<AOIManagerComponent>();
            //root.AddComponent<RoomManagerComponent>();
            root.AddComponent<MapComponent>();

            root.AddComponent<ActionsDispatcherComponent>();//所有技能actions


            root.AddComponent<MonsterMapComponent>();//怪物地图刷新器
            
            await ETTask.CompletedTask;
        }
    }
}