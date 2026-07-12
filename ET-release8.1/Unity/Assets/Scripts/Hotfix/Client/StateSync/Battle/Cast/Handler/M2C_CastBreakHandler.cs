namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastBreakHandler: MessageHandler<Scene,M2C_CastBreak>
    {
        protected override async ETTask Run(Scene root, M2C_CastBreak message)
        {
            Log.Console($" 玩家 {message.CasterId} 技能 {message.CastId} 被打断 ！！！ ");
            Scene currentScene = root.CurrentScene();
            UnitComponent unitComponent = currentScene?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            Unit caster = unitComponent.Get(message.CasterId);
            if (caster == null || caster.IsDisposed)
            {
                return;
            }

            caster.GetComponent<CastComponent>()?.Remove(message.CastId);

            CastBreak castBreak = new CastBreak();
            castBreak.CastId = message.CastId;
            castBreak.CasterId = message.CasterId;
            EventSystem.Instance.Publish(currentScene, castBreak);
            await ETTask.CompletedTask;
        }
    }
}
