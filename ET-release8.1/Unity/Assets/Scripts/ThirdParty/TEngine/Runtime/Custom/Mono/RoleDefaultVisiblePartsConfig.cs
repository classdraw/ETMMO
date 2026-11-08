using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    [Serializable]
    public class RoleDefaultVisiblePartsEntry
    {
        public string rolePrefabName;
        public string[] visiblePartNames = Array.Empty<string>();
    }

    /// <summary>
    /// 各 Role 预制体默认显示的 Synty 部件（Chr_*，带 Renderer 且源预制体中 active）。
    /// </summary>
    [CreateAssetMenu(fileName = "RoleDefaultVisiblePartsConfig", menuName = "ET/Role Default Visible Parts Config")]
    public class RoleDefaultVisiblePartsConfig : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/Bundles/ScriptObject/RoleDefaultVisiblePartsConfig.asset";

        public List<RoleDefaultVisiblePartsEntry> entries = new List<RoleDefaultVisiblePartsEntry>();

        public string[] GetVisiblePartNames(string rolePrefabName)
        {
            if (string.IsNullOrEmpty(rolePrefabName) || entries == null)
            {
                return null;
            }

            foreach (RoleDefaultVisiblePartsEntry entry in entries)
            {
                if (entry != null && entry.rolePrefabName == rolePrefabName)
                {
                    return entry.visiblePartNames;
                }
            }

            return null;
        }
    }
}
