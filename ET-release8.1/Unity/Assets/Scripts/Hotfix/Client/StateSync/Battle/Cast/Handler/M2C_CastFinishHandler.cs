namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_CastFinishHandler: MessageHandler<Scene,M2C_CastFinish>
    {
        protected override async ETTask Run(Scene root, M2C_CastFinish message)
        {
            Log.Console($" 玩家 {message.CasterId} 技能 {message.CastId} 结束了 ！！！ ");
            //回到idle 动画还原  特效还原 技能状态切换
            
            CastFinish castFinish = new CastFinish();
            castFinish.CastId = message.CastId;
            castFinish.CasterId = message.CasterId;
            EventSystem.Instance.Publish(root,castFinish);
            
            await ETTask.CompletedTask;
        }
    }
}