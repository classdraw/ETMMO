using System;

namespace XEngine.Utilities{
	/// <summary>
	/// 单利父类
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class Singleton<T> where T:class,new(){
		private static T _instance;
		public static bool HasInstance(){
			return _instance!=null;
		}

		public static T GetInstance(){
			return Instance;
		}
		public static T Instance{
			get{
				if(Singleton<T>._instance==null){
					Singleton<T>._instance=Activator.CreateInstance<T>();
					(Singleton<T>._instance as Singleton<T>).Init();
				}
				return Singleton<T>._instance;
			}
		}

		/// <summary>
		/// 实例销毁操作
		/// </summary>
		public static void DestroyInstance(){
			if(Singleton<T>._instance!=null){
				(Singleton<T>._instance as Singleton<T>).Release();
			}
			Singleton<T>._instance=null;
		}


		/// <summary>
		/// 初始化
		/// </summary>
		protected virtual void Init(){

		}

		/// <summary>
		/// 销毁
		/// </summary>
		protected virtual void Release(){
				
		}

	}
}
