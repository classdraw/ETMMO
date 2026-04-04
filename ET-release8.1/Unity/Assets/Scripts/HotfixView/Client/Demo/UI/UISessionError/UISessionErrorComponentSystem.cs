using GameLogic;
using UnityEngine.UI;

namespace ET.Client
{
	[EntitySystemOf(typeof(UISessionErrorComponent))]
	[FriendOf(typeof(UISessionErrorComponent))]
	public static partial class UISessionErrorComponentSystem
	{

		[EntitySystem]
		private static void Awake(this UISessionErrorComponent self)
		{
			UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
			self.m_textLoading = m_bindComponent.GetComponent<Text>(0);
			self.m_btnGoLogin = m_bindComponent.GetComponent<Button>(1);
			self.m_btnGoLogin.onClick.AddListener(() => { self.OnGoLoginAsync().Coroutine(); });
		}

		[EntitySystem]
		private static void Destroy(this UISessionErrorComponent self)
		{
		}

		public static async ETTask OnGoLoginAsync(this UISessionErrorComponent self)
		{
			Scene root = self.Root();
			await UIHelper.Remove(root, UIType.UISessionError);
			await SceneChangeHelper.SceneChangeToSimple(root, "Login", 0);
			
		}
	}
}