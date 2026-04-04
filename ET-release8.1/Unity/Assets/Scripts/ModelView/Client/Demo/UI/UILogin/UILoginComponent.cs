using UnityEngine;

namespace ET.Client
{
	[ComponentOf(typeof(UI))]
	public class UILoginComponent: Entity, IAwake,IDestroy
	{
		public GameObject account;
		public GameObject password;
		public GameObject loginBtn;
	}
}
