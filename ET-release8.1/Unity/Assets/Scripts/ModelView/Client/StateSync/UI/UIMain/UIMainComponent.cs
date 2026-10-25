using ET;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIMainComponent: Entity, IAwake,IDestroy
    {
        public Button m_btnBag;
    }
}