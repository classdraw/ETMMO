namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    [FriendOf(typeof(ClientCast))]
    [FriendOf(typeof(ClientCastComponent))]
    public class M2C_CastHitHandler: MessageHandler<Scene,M2C_CastHit>
    {
        protected override async ETTask Run(Scene root, M2C_CastHit message)
        {
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
            if (clientCastComponent == null || clientCastComponent.IsDisposed)
            {
                return;
            }

            ClientCast clientCast = clientCastComponent.Get(message.CastId);
            if (clientCast == null || clientCast.IsDisposed)
            {
                return;
            }

            clientCast.TargetsId.Clear();
            if (message.TargetsId != null)
            {
                clientCast.TargetsId.AddRange(message.TargetsId);
            }

            foreach (long targetId in clientCast.TargetsId)
            {
                Unit target = unitComponent.Get(targetId);
                if (target==null||target.IsDisposed)
                {
                    continue;
                }
                Log.Console($" 玩家 {message.CasterId} 技能 {message.CastId} 命中 {targetId} ");
                CastHit castHit = new CastHit();
                castHit.CastId = message.CastId;
                castHit.CasterId = message.CasterId;
                castHit.TargetId = targetId;
                castHit.HitIndex = message.HitIndex;
                castHit.IsSelf = message.IsSelf;
                EventSystem.Instance.Publish(currentScene, castHit);
            }

            await ETTask.CompletedTask;
        }
    }
}
