using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class UnitSpineCtrlItem
    {
        [UnitPartSlotKey]
        [Tooltip("部位槽 Key，对应 UnitPartSlot 枚举整型值")]
        public int slotPart;

        public SpriteRenderer render;

        [Tooltip("相对 UnitSpineCtrl 根节点的子路径，可用于 transform.Find(itemPath)")]
        public string itemPath;

        public Color color = Color.white;
    }

    /// <summary>
    /// 单位 Spine 换装控制：维护 SpriteRenderer 与部位槽、资源路径的映射。
    /// </summary>
    public class UnitSpineCtrl : MonoBehaviour
    {
        public List<UnitSpineCtrlItem> items = new List<UnitSpineCtrlItem>();
    }
}
