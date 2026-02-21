namespace ET.Client
{
	[Event(SceneType.Demo)]
	public class AppStartInitFinish_CreateLoginUI: AEvent<Scene, AppStartInitFinish>
	{
		protected override async ETTask Run(Scene root, AppStartInitFinish args)
		{
			await UIHelper.Create(root, UIType.UILogin, UILayer.Mid);
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
