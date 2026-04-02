using System;

namespace ET.Server
{
    [MessageLocationHandler(SceneType.Map)]
    public class G2M_RequestExitGameHandler: MessageLocationHandler<Unit, G2M_RequestExitGame,M2G_RequestExitGame>
    {
        protected override async ETTask Run(Unit unit, G2M_RequestExitGame request, M2G_RequestExitGame response)
        {
            //unit角色下线业务 然后保存unit及组件数据至数据库
            Log.Console($"玩家从map逻辑服下线 roleId:{unit.Id}");
            unit.RemoveComponent<AOIEntity>();
            //这里数据库业务保存unit数据保存数据库
            //1游戏角色装备  副本  状态等重置
            //unit.GetComponent<UnitDBSaveComponent>()?.SaveChangeNoWait();
            
            //正式释放unit
            RemoveUnit(unit).Coroutine();
            await ETTask.CompletedTask;
            
            
            
            
        }

        private async ETTask RemoveUnit(Unit unit)
        {
            await unit.Fiber().WaitFrameFinish();//等待一帧，先把消息回复给请求方
            await unit.RemoveLocation(LocationType.Unit);
            unit.Root().GetComponent<MessageLocationSenderComponent>().Get(LocationType.GateSession).Remove(unit.Id);
            UnitComponent unitComponent = unit.Root().GetComponent<UnitComponent>();
            unitComponent.Remove(unit.Id);
        }
    }
}

