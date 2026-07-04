namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastHitHandler: MessageHandler<Scene,M2C_CastHit>
    {
        protected override async ETTask Run(Scene root, M2C_CastHit message)
        {
            //技能命中 特效动作等
            foreach (long targetId in message.TargetsId)
            {
                Log.Console($" 玩家 {message.CasterId} 技能 {message.CastId} 命中 {targetId} ");
                CastHit castHit = new CastHit();
                castHit.CastId = message.CastId;
                castHit.CasterId = message.CasterId;
                castHit.TargetId = targetId;
                EventSystem.Instance.Publish(root,castHit);
                
            }
            
            await ETTask.CompletedTask;
            
        }
    }
}