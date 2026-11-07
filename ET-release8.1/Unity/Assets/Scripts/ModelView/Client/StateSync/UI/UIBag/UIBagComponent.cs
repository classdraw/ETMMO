using SuperScrollView;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIBagComponent : Entity, IAwake, IDestroy
    {
        public const int SlotsPerRow = 5;
        public const string RowItemPrefabName = "UIBagItem";

        public Button m_btnClose;
        public LoopListView2 m_loopList2;

        public EntityRef<Item>[] SlotItems;
        public bool ListInited;
    }
}
