using System;

namespace ET.Client
{
    [MessageHandler(SceneType.StateSync)]
    public class M2C_NoticeUnitNumericListHandler: MessageHandler<Scene, M2C_NoticeUnitNumericList>
    {
        protected override async ETTask Run(Scene root, M2C_NoticeUnitNumericList message)
        {
            NumericComponent numericComponent = root?.CurrentScene()?.GetComponent<UnitComponent>()?.Get(message.UnitId)?.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            if (message.NumericTypeList == null || message.NewValueList == null)
            {
                await ETTask.CompletedTask;
                return;
            }

            int count = Math.Min(message.NumericTypeList.Count, message.NewValueList.Count);
            for (int i = 0; i < count; i++)
            {
                int numericType = message.NumericTypeList[i];
                long newValue = message.NewValueList[i];
                numericComponent.Set(numericType, newValue);
                //Log.Console($"{numericType}_____{newValue}");
            }

            await ETTask.CompletedTask;
        }
    }
}

