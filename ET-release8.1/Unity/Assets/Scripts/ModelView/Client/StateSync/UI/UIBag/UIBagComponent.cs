using System.Collections.Generic;
using SuperScrollView;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIBagComponent : Entity, IAwake, IDestroy
    {
        public const int SlotsPerRow = 5;
        public const string RowItemPrefabName = "UIBagItem";

        public const string ItemIconAtlasLocation = "Assets/Bundles/Atlas/IconAtlas.spriteatlasv2";
        public const string ItemIconAtlasShortLocation = "IconAtlas.spriteatlasv2";
        public const string ItemIconSingleFolder = "Assets/Res/UI/Icons_Atlas";

        public Button m_btnClose;
        public LoopListView2 m_loopList2;

        public EntityRef<Item>[] SlotItems;
        public bool ListInited;

        public Dictionary<string, Sprite> IconSpriteCache;
        public bool ItemIconAtlasSpritesIndexed;
        public bool ItemIconAtlasIndexing;
        public bool ItemIconAtlasLoadFailed;
    }
}
