using ET;
using UnityEngine;
using UnityEngine.UI;
using TEngine;
using GameLogic;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoginComponent: Entity, IAwake,IDestroy
	{
		/// <summary>左侧创角槽当前展示用的外貌（与界面换装一致，创建角色时提交）。</summary>
		public RoleAvatarIds PendingCreateLeftAvatar;

		/// <summary>右侧创角槽当前展示用的外貌。</summary>
		public RoleAvatarIds PendingCreateRightAvatar;

		public GameObject m_goObj1;
		public InputField m_inputAccount;
		public InputField m_inputPassword;
		public Button m_btnLogin;
		public GameObject m_goObj2;
		public LayoutLoopList m_loopListVerticalScroll;
		public Button m_btnBack1;
		public Text m_textServerList;
		public GameObject m_goObj3;
		public Text m_textLeftTitle;
		public GameObject m_goLeft;
		public Button m_btnLeftCreate;
		public Button m_btnLeftTran;
		public Button m_btnLeftDelete;
		public Button m_btnLeftEnter;
		public InputField m_inputLeft;
		public Text m_textRightTitle;
		public GameObject m_goRight;
		public Button m_btnRightCreate;
		public Button m_btnRightTran;
		public Button m_btnRightDelete;
		public Button m_btnRightEnter;
		public InputField m_inputRight;
		public Button m_btnBack2;
	}
}
