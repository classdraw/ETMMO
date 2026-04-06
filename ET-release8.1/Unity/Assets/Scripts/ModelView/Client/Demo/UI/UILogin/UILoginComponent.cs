using UnityEngine;
using UnityEngine.UI;
using TEngine;
using GameLogic;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoginComponent: Entity, IAwake,IDestroy
	{
		public InputField m_inputAccount;
		public InputField m_inputPassword;
		public Button m_btnLogin;
		public LayoutLoopList m_loopListVerticalScroll;
		public Button m_btnBack;
		public Text m_textServerList;
	}
}
