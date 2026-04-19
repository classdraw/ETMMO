using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UISessionErrorComponent: Entity, IAwake,IDestroy
	{
		public Text m_textLoading;
		public Button m_btnGoLogin;
	}
}
