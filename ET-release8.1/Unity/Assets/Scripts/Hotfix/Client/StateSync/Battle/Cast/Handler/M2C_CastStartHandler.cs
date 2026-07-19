using Unity.Mathematics;

namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    [FriendOf(typeof(ClientCastComponent))]
    public class M2C_CastStartHandler: MessageHandler<Scene,M2C_CastStart>
    {
        private const float MinTurnDirectionSqr = 0.01f;

        protected override async ETTask Run(Scene root, M2C_CastStart message)
        {
            Log.Console($"玩家 {message.CasterId} 开始释放 {message.CastConfigId} 技能 {message.CastId} ！！！");
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
            if (clientCastComponent == null||clientCastComponent.IsDisposed)
            {
                return;
            }

            if (math.lengthsq(message.Forward) > MinTurnDirectionSqr)
            {
                caster.Forward = math.normalize(message.Forward);
            }
            
            caster.CreateAndAddCast(message);

            CastStart castStart = new CastStart();
            castStart.CastId = message.CastId;
            castStart.CasterId = message.CasterId;
            castStart.CasterConfigId = message.CastConfigId;
            EventSystem.Instance.Publish(currentScene, castStart);
            await ETTask.CompletedTask;
        }
    }
}
