using System;
using Unity.Mathematics;

namespace ET.Server
{
    [MessageHandler(SceneType.Map)]
    public class M2M_UnitTransferRequestHandler: MessageHandler<Scene, M2M_UnitTransferRequest, M2M_UnitTransferResponse>
    {
        protected override async ETTask Run(Scene root, M2M_UnitTransferRequest request, M2M_UnitTransferResponse response)
        {
            UnitComponent unitComponent = root.GetComponent<UnitComponent>();
            Unit unit = MongoHelper.Deserialize<Unit>(request.Unit);

            unitComponent.AddChild(unit);
            unitComponent.Add(unit);

            foreach (byte[] bytes in request.Entitys)
            {
                Entity entity = MongoHelper.Deserialize<Entity>(bytes);
                unit.AddComponent(entity);
            }
            
            unit.LastMapId = unit.MapId;
            unit.MapId = request.MapId;
            unit.MapUid = root.Fiber.Id;
            
            UnitHelper.AfterTransfer(unit, root,request);
            if (request.IsEnterGame)
            {
                await UnitHelper.DealOfflineMsg(unit);
            }

            // 通知客户端开始切场景
            M2C_StartSceneChange m2CStartSceneChange = M2C_StartSceneChange.Create();
            m2CStartSceneChange.SceneInstanceId = root.InstanceId;
            m2CStartSceneChange.SceneName = root.Name;
            await unit.SendToClient(m2CStartSceneChange);
            
            // 通知客户端创建My Unit
            M2C_CreateMyUnit m2CCreateUnits = M2C_CreateMyUnit.Create();
            m2CCreateUnits.Unit = UnitHelper.CreateUnitInfo(unit);
            await unit.SendToClient(m2CCreateUnits);

            // 加入aoi
            unit.AddComponent<AOIEntity, int, float3>(9 * 1000, unit.Position);
            
            if (request.IsEnterGame)
            {
                //unit.GetComponent<UnitBasic>().LastLoginTime = TimeInfo.Instance.Frame;
                EventSystem.Instance.Publish(root, new UnitEnterGame() { Unit = unit });
            }

            // 解锁location，可以接收发给Unit的消息
            await root.Root().GetComponent<LocationProxyComponent>().UnLock(LocationType.Unit, unit.Id, request.OldActorId, unit.GetActorId());
        }
    }
}