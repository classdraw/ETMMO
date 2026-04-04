using System.Collections.Generic;

namespace ET.Client
{
	/// <summary>
	/// 管理Scene上的UI
	/// </summary>
	[EntitySystemOf(typeof(UIComponent))]
	[FriendOf(typeof(UIComponent))]
	public static partial class UIComponentSystem
	{
		[EntitySystem]
		private static void Awake(this UIComponent self)
		{
			self.UIGlobalComponent = self.Root().GetComponent<UIGlobalComponent>();
		}
		
		public static async ETTask<UI> Create(this UIComponent self, string uiType)
		{
			UI ui = await self.UIGlobalComponent.OnCreate(self, uiType);
			if (UIEventComponent.Instance.UIFullScreens.TryGetValue(uiType, out bool fullScreen))
			{
				ui.FullScreen = fullScreen;
			}
			else
			{
				ui.FullScreen = false;
			}

			self.UIs.Add(uiType, ui);
			self.UIStack.Add(ui);
			if (ui.FullScreen)
			{
				RefreshUIStackVisibility(self);
			}

			return ui;
		}

		public static void Remove(this UIComponent self, string uiType)
		{
			if (!self.UIs.TryGetValue(uiType, out EntityRef<UI> uiRef))
			{
				return;
			}
			
			self.UIGlobalComponent.OnRemove(self, uiType);
			
			self.UIs.Remove(uiType);
			UI ui = uiRef;
			RemoveUIFromStack(self, ui);
			ui?.Dispose();
			RefreshUIStackVisibility(self);
			
		}

		public static void RefreshUIStackVisibility(UIComponent self)
		{
			bool isHideNext = false;
			for (int i = self.UIStack.Count - 1; i >= 0; i--)
			{
				UI stackUi = self.UIStack[i];
				if (stackUi == null)
				{
					continue;
				}

				if (!isHideNext)
				{
					if (stackUi.IsHide)
					{
						continue;
					}

					stackUi.Visible(true);
					if (stackUi.FullScreen)
					{
						isHideNext = true;
					}
				}
				else
				{
					stackUi.Visible(false);
				}
			}
		}

		private static void RemoveUIFromStack(UIComponent self, UI ui)
		{
			if (ui == null)
			{
				return;
			}

			long instanceId = ui.InstanceId;
			for (int i = self.UIStack.Count - 1; i >= 0; i--)
			{
				UI stackUi = self.UIStack[i];
				if (stackUi != null && stackUi.InstanceId == instanceId)
				{
					self.UIStack.RemoveAt(i);
					return;
				}
			}
		}

		public static UI Get(this UIComponent self, string name)
		{
			self.UIs.TryGetValue(name, out EntityRef<UI> uiRef);
			return uiRef;
		}
	}
}