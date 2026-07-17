namespace ET.Client
{
    //专门处理跳字
    [MessageHandler(SceneType.StateSync)]
    public class M2C_BattleResultHandler: MessageHandler<Scene, M2C_BattleResult>
    {
        protected override async ETTask Run(Scene root, M2C_BattleResult message)
        {
            Scene currentScene = root.CurrentScene();
            Unit target = currentScene?.GetComponent<UnitComponent>()?.Get(message.TargetId);
            if (target != null)
            {
                BattleHudHelper.ShowBattleResultNumber(target, message.Damage, message.IsCrit);
            }

            await ETTask.CompletedTask;
        }
    }
}
