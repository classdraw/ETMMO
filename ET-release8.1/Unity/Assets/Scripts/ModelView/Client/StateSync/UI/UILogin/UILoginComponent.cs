using ET;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoginComponent: Entity, IAwake,IDestroy
	{
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
		public Text m_textJobL;
		public Button m_btnJobRightL;
		public Button m_btnJobLeftL;
		public GameObject m_goRightChoose;
		public Text m_textRaceR;
		public Button m_btnRaceRightR;
		public Button m_btnRaceLeftR;
		public Text m_textGenderR;
		public Button m_btnGenderRightR;
		public Button m_btnGenderLeftR;
		public Text m_textJobR;
		public Button m_btnJobRightR;
		public Button m_btnJobLeftR;
		/// <summary>scene_CreateRole 下 RoleLeft / RoleRight 挂点。</summary>
		public Transform m_roleLeftAnchor;
		public Transform m_roleRightAnchor;
		public ExternalDisplayAppearance LeftAppearance;
		public ExternalDisplayAppearance RightAppearance;
		

	}
}
