namespace ET.Server
{
    [Event(SceneType.All)]
    public class NumericChangeEvent_NoticeToClient :AEvent<Scene,NumbericChange>
    {
        protected override async ETTask Run(Scene scene, NumbericChange args)
        {
            await ETTask.CompletedTask;
            NumbericChange numbericChange = args;
            Unit unit = args.Unit;
            unit.GetComponent<NumericNoticeComponent>()?.Notice(numbericChange.NumericType,numbericChange.New);
        }
    }
}

