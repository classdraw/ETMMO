namespace ET.Client
{
	[Event(SceneType.StateSync)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			await SceneChangeHelper.SceneChangeToSimple(root, "Login", 0);
			// UILogin 由 SceneChangeFinishEvent_CreateUIHelp 在 Login 场景下创建
			/*
			var computer1=root.GetComponent<ComputersComponent>().AddChild<Computer>();
			computer1.AddComponent<ComputerTest1Component>();
			await root.GetComponent<TimerComponent>().WaitAsync(3000);
			Log.Console("121212");
			computer1?.Dispose();
			*/
		}
	}
}
