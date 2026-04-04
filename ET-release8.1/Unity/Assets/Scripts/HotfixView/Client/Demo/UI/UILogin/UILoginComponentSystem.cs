using UnityEngine;
using UnityEngine.UI;
using GameLogic;

namespace ET.Client
{
	[EntitySystemOf(typeof(UILoginComponent))]
	[FriendOf(typeof(UILoginComponent))]
	public static partial class UILoginComponentSystem
	{
		[EntitySystem]
		private static void Awake(this UILoginComponent self)
		{
			UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
			self.m_inputAccount = m_bindComponent.GetComponent<InputField>(0);
			self.m_inputPassword = m_bindComponent.GetComponent<InputField>(1);
			self.m_btnLogin = m_bindComponent.GetComponent<Button>(2);
			self.m_btnLogin.onClick.AddListener(() => { self.OnLogin(); });
		}
		
		[EntitySystem]
		private static void Destroy(this UILoginComponent self)
		{
			
		}
 
		//按钮点击登录流程
		public static void OnLogin(this UILoginComponent self)
		{
			LoginHelper.Login(
				self.Root(), 
				self.m_inputAccount.text, 
				self.m_inputPassword.text).Coroutine();

		}
	}
}
