namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BuffRemoveHandler: MessageHandler<Scene,M2C_BuffRemove>
    {
        protected override async ETTask Run(Scene root, M2C_BuffRemove message)
        {
            Log.Console($" 玩家 {message.UnitId} 移除buff Id {message.BuffId} ");
            UnitComponent unitComponent = root.GetComponent<UnitComponent>();
            if (unitComponent==null)
            {
                return;
            }
            Unit unit = unitComponent.Get(message.UnitId);
            if (unit!=null&&!unit.IsDisposed)
            {
                BuffRemove buffRemove = new BuffRemove();
                buffRemove.Unit = unit;
                buffRemove.BuffId = message.BuffId;
                EventSystem.Instance.Publish(root,buffRemove);
            }
            await ETTask.CompletedTask;
        }
    }
}