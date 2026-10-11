using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class UnitPartSlotItem
    {
        [Tooltip("全局唯一 Id，收集时递增分配")]
        public int id;

        [Tooltip("源贴图 Assets 路径")]
        public string textureAssetPath;

        [Tooltip("子图（Sprite）名称")]
        public string spriteName;

        [Tooltip("子图引用")]
        public Sprite sprite;
    }

    [Serializable]
    public class UnitPartSlotGroup
    {
        [UnitPartSlotKey]
        [Tooltip("部位槽 Key，对应 Model.Share 中 UnitPartSlot 枚举整型值")]
        public int partSlotKey;

        public List<UnitPartSlotItem> items = new List<UnitPartSlotItem>();
    }

    /// <summary>
    /// SPUM Legacy 部件收集结果：按 partSlotKey（<see cref="UnitPartSlot"/> 整型）分组，每项对应一张子图。
    /// </summary>
    public class UnitPartSlotCollectionConfig : ScriptableObject
    {
        public const int FirstItemId = 1001;

        public int nextItemId = FirstItemId;

        public List<UnitPartSlotGroup> groups = new List<UnitPartSlotGroup>();

        [NonSerialized]
        private Dictionary<int, UnitPartSlotItem> idLookup;

        [NonSerialized]
        private Dictionary<int, UnitPartSlotGroup> slotLookup;

        public void RebuildLookup()
        {
            idLookup = new Dictionary<int, UnitPartSlotItem>();
            slotLookup = new Dictionary<int, UnitPartSlotGroup>();

            for (int i = 0; i < groups.Count; i++)
            {
                UnitPartSlotGroup group = groups[i];
                if (group == null)
                {
                    continue;
                }

                slotLookup[group.partSlotKey] = group;

                if (group.items == null)
                {
                    continue;
                }

                for (int j = 0; j < group.items.Count; j++)
                {
                    UnitPartSlotItem item = group.items[j];
                    if (item == null || item.id <= 0)
                    {
                        continue;
                    }

                    idLookup[item.id] = item;
                }
            }
        }

        public bool TryGetItem(int itemId, out UnitPartSlotItem item)
        {
            if (idLookup == null)
            {
                RebuildLookup();
            }

            return idLookup.TryGetValue(itemId, out item);
        }

        public bool TryGetGroup(int partSlotKey, out UnitPartSlotGroup group)
        {
            if (slotLookup == null)
            {
                RebuildLookup();
            }

            return slotLookup.TryGetValue(partSlotKey, out group);
        }

        private void OnEnable()
        {
            idLookup = null;
            slotLookup = null;
        }

        private void OnValidate()
        {
            idLookup = null;
            slotLookup = null;
        }
    }
}
