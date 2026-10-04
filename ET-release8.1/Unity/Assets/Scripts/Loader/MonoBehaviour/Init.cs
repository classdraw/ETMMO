using System;
using CommandLine;
using UnityEngine;

namespace ET
{
	public class Init: MonoBehaviour
	{
		[Header("客户端调试")]
		[Tooltip("勾选后，所有 Unit 头顶名字显示阵营 Id；否则显示实际名字")]
		public bool ReplaceUnitNameWithFaction;

		private void Awake()
		{
			SyncViewDebugSettings();
		}

		private void OnValidate()
		{
			SyncViewDebugSettings();
		}

		private void SyncViewDebugSettings()
		{
			ClientViewDebugSettings.ReplaceUnitNameWithFaction = ReplaceUnitNameWithFaction;
		}

		private void Start()
		{
			this.StartAsync().Coroutine();
		}
		
		private async ETTask StartAsync()
		{
			DontDestroyOnLoad(gameObject);
			
			AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
			{
				Log.Error(e.ExceptionObject.ToString());
			};

			// 命令行参数
			string[] args = "".Split(" ");
			Parser.Default.ParseArguments<Options>(args)
				.WithNotParsed(error => throw new Exception($"命令行格式错误! {error}"))
				.WithParsed((o)=>World.Instance.AddSingleton(o));
			Options.Instance.StartConfig = $"StartConfig/Localhost";
			
			World.Instance.AddSingleton<Logger>().Log = new UnityLogger();
			ETTask.ExceptionHandler += Log.Error;
			
			World.Instance.AddSingleton<TimeInfo>();
			World.Instance.AddSingleton<FiberManager>();

			await World.Instance.AddSingleton<ResourcesComponent>().CreatePackageAsync("DefaultPackage", true);
			
			CodeLoader codeLoader = World.Instance.AddSingleton<CodeLoader>();
			await codeLoader.DownloadAsync();
			
			codeLoader.Start();
		}

		private void Update()
		{
			TimeInfo.Instance.Update();
			FiberManager.Instance.Update();
		}

		private void LateUpdate()
		{
			FiberManager.Instance.LateUpdate();
		}

		private void OnApplicationQuit()
		{
			World.Instance.Dispose();
		}
	}
	
	
}