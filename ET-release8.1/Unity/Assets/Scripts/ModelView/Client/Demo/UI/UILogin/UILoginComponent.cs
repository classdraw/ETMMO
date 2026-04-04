using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoginComponent: Entity, IAwake,IDestroy
	{
		public InputField m_inputAccount;
		public InputField m_inputPassword;
		public Button m_btnLogin;
	}
}
