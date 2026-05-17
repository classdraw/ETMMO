using System;
using System.Collections.Generic;
using ET;
using ET.Server;
using UnityEngine;
using UnityEngine.UI;
using GameLogic;
using TEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(UIHelpComponent))]
    [FriendOf(typeof(UIHelpComponent))]
    public static partial class UIHelpComponentSystem
    {
        [EntitySystem]
		private static void Awake(this UIHelpComponent self)
		{
			UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
			self.m_goPanel = m_bindComponent.GetComponent<RectTransform>(0).gameObject;
			self.m_btnTap1 = m_bindComponent.GetComponent<Button>(1);
			self.m_btnTap1.onClick.AddListener(() => { self.OnTap1(); });
			self.m_btnTap2 = m_bindComponent.GetComponent<Button>(2);
			self.m_btnTap2.onClick.AddListener(() => { self.OnTap2(); });
			self.m_btnTap3 = m_bindComponent.GetComponent<Button>(3);
			self.m_btnTap3.onClick.AddListener(() => { self.OnTap3(); });
			self.m_btnTap4 = m_bindComponent.GetComponent<Button>(4);
			self.m_btnTap4.onClick.AddListener(() => { self.OnTap4(); });
			self.m_inputAll = m_bindComponent.GetComponent<InputField>(5);
			self.m_goP1 = m_bindComponent.GetComponent<RectTransform>(6).gameObject;
			self.m_goP2 = m_bindComponent.GetComponent<RectTransform>(7).gameObject;
			self.m_goP3 = m_bindComponent.GetComponent<RectTransform>(8).gameObject;
			self.m_btnP3bag = m_bindComponent.GetComponent<Button>(9);
			self.m_btnP3bag.onClick.AddListener(() => { self.OnP3bag(); });
			self.m_btnP3addItem = m_bindComponent.GetComponent<Button>(10);
			self.m_btnP3addItem.onClick.AddListener(() => { self.OnP3addItem(); });
			self.m_btnP3removeItem = m_bindComponent.GetComponent<Button>(11);
			self.m_btnP3removeItem.onClick.AddListener(() => { self.OnP3removeItem(); });
			self.m_goP4 = m_bindComponent.GetComponent<RectTransform>(12).gameObject;
			self.m_btnGM = m_bindComponent.GetComponent<Button>(13);
			self.m_btnGM.onClick.AddListener(() => { self.OnGM(); });
			
			self.m_GOs.Clear();
			self.m_GOs.Add(self.m_goP1);
			self.m_GOs.Add(self.m_goP2);
			self.m_GOs.Add(self.m_goP3);
			self.m_GOs.Add(self.m_goP4);

			self.HideAllPanel();
			self.m_goP1.SetActive(true);
			self.m_goPanel.SetActive(false);
		}
		
		[EntitySystem]
		private static void Destroy(this UIHelpComponent self)
		{
			self.m_GOs.Clear();
		}

		private static void HideAllPanel(this UIHelpComponent self)
		{
			for (int i=0;i<self.m_GOs.Count;i++) {
				self.m_GOs[i].SetActive(false);
			}
		}

		public static void OnGM(this UIHelpComponent self)
		{
			self.m_goPanel.SetActive(!self.m_goPanel.activeSelf);
		}

		public static void OnTap1(this UIHelpComponent self)
		{
			self.HideAllPanel();
			self.m_goP1.SetActive(true);
		}

		public static void OnTap2(this UIHelpComponent self)
		{
			self.HideAllPanel();
			self.m_goP2.SetActive(true);
		}

		public static void OnTap3(this UIHelpComponent self)
		{
			self.HideAllPanel();
			self.m_goP3.SetActive(true);
		}

		public static void OnTap4(this UIHelpComponent self)
		{
			self.HideAllPanel();
			self.m_goP4.SetActive(true);
		}


		#region p3
		//背包按钮
		public static void OnP3bag(this UIHelpComponent self)
		{
			KnapsackHelper.GetAllItems(self.Root()).Coroutine();
		}
		
		//增加道具
		public static void OnP3addItem(this UIHelpComponent self)
		{
			var val = self.m_inputAll.text;
			if (string.IsNullOrEmpty(val))
			{
				return;
			}

			if (int.TryParse(val,out int result))
			{
				KnapsackHelper.RequestAddItem(self.Root(),KnapsackContainerType.Inventory,result).Coroutine();
			}
		}
		
		//移除道具
		public static void OnP3removeItem(this UIHelpComponent self)
		{
			var val = self.m_inputAll.text;
			if (string.IsNullOrEmpty(val))
			{
				return;
			}

			if (long.TryParse(val,out long result))
			{
				KnapsackHelper.RequestRemoveItem(self.Root(),KnapsackContainerType.Inventory,result).Coroutine();
			}
		}

		#endregion
    }
}
