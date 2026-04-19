namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_NoticeUnitNumericHandler:MessageHandler<Scene,M2C_NoticeUnitNumeric>
    {
        protected override async ETTask Run(Scene root, M2C_NoticeUnitNumeric message)
        {
            var numericComponent = root?.CurrentScene()?.GetComponent<UnitComponent>()?.Get(message.UnitId)?.GetComponent<NumericComponent>();
            numericComponent?.Set(message.NumericType,message.NewValue);
            //Log.Console(message.NumericType+"_____"+message.NewValue);
            await ETTask.CompletedTask;
        }
    }
}

