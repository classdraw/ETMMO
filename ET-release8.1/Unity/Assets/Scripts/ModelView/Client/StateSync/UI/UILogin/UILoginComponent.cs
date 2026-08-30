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
		/// <summary>左侧空槽「创建角色」待提交的 <see cref="RoleInfoProto.ConfigId"/>（换装按 1001→1002→…→1005→1001，未选时界面默认 1001）。</summary>
		public int PendingCreateLeftConfigId;
		/// <summary>右侧空槽「创建角色」待提交的 <see cref="RoleInfoProto.ConfigId"/>（换装按 1001→1002→…→1005→1001，未选时界面默认 1001）。</summary>
		public int PendingCreateRightConfigId;
		
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
		public Text m_textLeftModel;
		public GameObject m_goLeft;
		public Button m_btnLeftCreate;
		public Button m_btnLeftDelete;
		public Button m_btnLeftEnter;
		public InputField m_inputLeft;
		public Text m_textRightTitle;
		public Text m_textRightModel;
		public GameObject m_goRight;
		public Button m_btnRightCreate;
		public Button m_btnRightDelete;
		public Button m_btnRightEnter;
		public InputField m_inputRight;
		public Button m_btnBack2;
		public GameObject m_goLeftChoose;
		public Text m_textRaceL;
		public Button m_btnRaceRightL;
		public Button m_btnRaceLeftL;
		public Text m_textGenderL;
		public Button m_btnGenderRightL;
		public Button m_btnGenderLeftL;
		public Text m_textBodyL;
		public Button m_btnBodyRightL;
		public Button m_btnBodyLeftL;
		public Text m_textHeadL;
		public Button m_btnHeadRightL;
		public Button m_btnHeadLeftL;
		public Text m_textTailL;
		public Button m_btnTailRightL;
		public Button m_btnTailLeftL;
		public Text m_textShirtL;
		public Button m_btnShirtRightL;
		public Button m_btnShirtLeftL;
		public Text m_textPantsL;
		public Button m_btnPantsRightL;
		public Button m_btnPantsLeftL;
		public GameObject m_goRightChoose;
		public Text m_textRaceR;
		public Button m_btnRaceRightR;
		public Button m_btnRaceLeftR;
		public Text m_textGenderR;
		public Button m_btnGenderRightR;
		public Button m_btnGenderLeftR;
		public Text m_textBodyR;
		public Button m_btnBodyRightR;
		public Button m_btnBodyLeftR;
		public Text m_textHeadR;
		public Button m_btnHeadRightR;
		public Button m_btnHeadLeftR;
		public Text m_textTailR;
		public Button m_btnTailRightR;
		public Button m_btnTailLeftR;
		public Text m_textShirtR;
		public Button m_btnShirtRightR;
		public Button m_btnShirtLeftR;
		public Text m_textPantsR;
		public Button m_btnPantsRightR;
		public Button m_btnPantsLeftR;
		public GameObject m_goUIRoleLeft;
		public GameObject m_goUIRoleRight;
		
		
		public FrameSheetAnimPlayer m_playerUIRoleLeft;
		public FrameSheetAnimPlayer m_playerUIRoleRight;
		public LoginRoleAppearance LeftAppearance;
		public LoginRoleAppearance RightAppearance;
		

	}
}
