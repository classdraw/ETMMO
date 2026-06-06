using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UIHelpComponent : Entity, IAwake,IDestroy
	{
		public GameObject m_goPanel;
		public Button m_btnTap1;
		public Button m_btnTap2;
		public Button m_btnTap3;
		public Button m_btnTap4;
		public InputField m_inputAll;
		public GameObject m_goP1;
		public Button m_btnP1rankList;
		public Button m_btnP1addMail;
		public Button m_btnP1GetMail;
		public Button m_btnP1ReadMail;
		public Button m_btnP1CollectMail;
		public Button m_btnP1TransferMap;
		public GameObject m_goP2;
		public GameObject m_goP3;
		public Button m_btnP3bag;
		public Button m_btnP3addItem;
		public Button m_btnP3removeItem;
		public GameObject m_goP4;
		public Button m_btnGM;
		
		public List<GameObject> m_GOs = new List<GameObject>();
	}
	
}
