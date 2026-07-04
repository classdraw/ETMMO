namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastBreakHandler: MessageHandler<Scene,M2C_CastBreak>
    {
        protected override async ETTask Run(Scene root, M2C_CastBreak message)
        {
            Log.Console($" 玩家 {message.CasterId} 技能 {message.CastId} 被打断 ！！！ ");
            //回到idle 动画还原  特效还原
            
            
            CastBreak castBreak = new CastBreak();
            castBreak.CastId = message.CastId;
            castBreak.CasterId = message.CasterId;
            EventSystem.Instance.Publish(root,castBreak);
            
            await ETTask.CompletedTask;
            
        }
    }
}