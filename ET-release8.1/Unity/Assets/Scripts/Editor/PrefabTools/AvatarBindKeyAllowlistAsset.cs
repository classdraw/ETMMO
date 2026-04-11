using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 白名单单行：字符串 key（与挂 SpriteRenderer 的节点名一致）与整型 Value（与 <c>AvatarPartType</c> 枚举取值对应，通常从 0 递增）。
    /// </summary>
    [Serializable]
    public class AvatarBindKeyEntry
    {
        public string key;
        public int value;
    }

    /// <summary>
    /// ReferenceSpriteCollector「角色绑点收集」允许的 key 白名单（工程内资源，可入库版本管理）。
    /// </summary>
    [CreateAssetMenu(fileName = "AvatarBindKeyAllowlist", menuName = "ET/角色绑点 Key 白名单", order = 500)]
    public class AvatarBindKeyAllowlistAsset : ScriptableObject
    {
        [Tooltip("Key：节点名字符串；Value：与 AvatarPartType 一致的整型（可从 0 递增）。")]
        public List<AvatarBindKeyEntry> entries = new List<AvatarBindKeyEntry>();

        [Tooltip("预制体处理工具窗口内白名单列表区域高度（像素）。")]
        [Range(100f, 520f)]
        public float editorListScrollHeight = 180f;

        /// <summary> 旧版仅 string 列表时的字段名，用于自动迁移为 entries（Value 按行号 0,1,2…）。 </summary>
        [SerializeField, HideInInspector]
        private List<string> keys;

        /// <summary> 在编辑器加载后调用：若存在旧版 keys 且 entries 为空，则迁移。 </summary>
        public void MigrateLegacyKeysIfNeeded()
        {
            if (keys == null || keys.Count == 0)
            {
                return;
            }

            if (entries == null)
            {
                entries = new List<AvatarBindKeyEntry>();
            }

            if (entries.Count > 0)
            {
                keys = null;
                return;
            }

            int v = 0;
            foreach (string s in keys)
            {
                string k = s?.Trim() ?? string.Empty;
                if (k.Length > 0)
                {
                    entries.Add(new AvatarBindKeyEntry { key = k, value = v++ });
                }
            }

            keys = null;
            EditorUtility.SetDirty(this);
        }
    }
}
