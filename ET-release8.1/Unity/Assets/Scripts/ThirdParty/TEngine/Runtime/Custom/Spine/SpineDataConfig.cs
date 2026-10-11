using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class SpineDataConfigEntry
    {
        [UnitPartSlotKey]
        [Tooltip("部位槽 Key，与 matchingTables 行序 / UnitPartSlot 一致")]
        public int slotPart;

        [Tooltip("SPUM MatchingElement.Index")]
        public int index;
    }

    /// <summary>
    /// 从 SPUM 角色预制体 matchingTables 导出的槽位索引配置。
    /// </summary>
    public class SpineDataConfig : ScriptableObject
    {
        [Tooltip("来源预制体 Assets 路径")]
        public string sourcePrefabPath;

        public List<SpineDataConfigEntry> entries = new List<SpineDataConfigEntry>();
    }
}
