namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BattleResultHandler : MessageHandler<Scene, M2C_BattleResult>
    {
        protected override async ETTask Run(Scene root, M2C_BattleResult message)
        {
            Scene currentScene = root.CurrentScene();
            UnitComponent unitComponent = currentScene?.GetComponent<UnitComponent>();
            if (unitComponent == null)
            {
                return;
            }

            Unit target = unitComponent.Get(message.TargetId);
            if (target == null || target.IsDisposed)
            {
                return;
            }

            EventSystem.Instance.Publish(currentScene, new BattleResult
            {
                AttackerId = message.AttackerId,
                TargetId = message.TargetId,
                Damage = message.Damage,
                IsCrit = message.IsCrit,
            });

            await ETTask.CompletedTask;
        }
    }
}
