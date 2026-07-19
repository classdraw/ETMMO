namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastFinishHandler: MessageHandler<Scene,M2C_CastFinish>
    {
        protected override async ETTask Run(Scene root, M2C_CastFinish message)
        {
            Log.Console($" 玩家 {message.CasterId} 技能 {message.CastId} 结束了 ！！！ ");
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

            ClientCastComponent clientCastComponent = caster.GetComponent<ClientCastComponent>();
            if (clientCastComponent==null||clientCastComponent.IsDisposed)
            {
                return;
            }

            ClientCast clientCast = clientCastComponent.Get(message.CastId);
            if (clientCast==null||clientCast.IsDisposed)
            {
                return;
            }
             
            CastFinish castFinish = new CastFinish();
            castFinish.CastId = message.CastId;
            castFinish.CasterId = message.CasterId;
            EventSystem.Instance.Publish(currentScene, castFinish);
            
            clientCastComponent.Remove(message.CastId);
            await ETTask.CompletedTask;
        }
    }
}
