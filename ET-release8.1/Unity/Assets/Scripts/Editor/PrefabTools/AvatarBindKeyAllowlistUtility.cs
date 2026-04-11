using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 默认路径下的白名单资源加载；与 <see cref="AvatarBindKeyAllowlistAsset"/> 配套使用。
    /// </summary>
    public static class AvatarBindKeyAllowlistUtility
    {
        public const string DefaultAssetPath = "Assets/Settings/AvatarBindKeyAllowlist.asset";

        public static AvatarBindKeyAllowlistAsset Load()
        {
            AvatarBindKeyAllowlistAsset asset = AssetDatabase.LoadAssetAtPath<AvatarBindKeyAllowlistAsset>(DefaultAssetPath);
            asset?.MigrateLegacyKeysIfNeeded();
            return asset;
        }

        public static AvatarBindKeyAllowlistAsset CreateAtDefaultPath()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
            {
                AssetDatabase.CreateFolder("Assets", "Settings");
            }

            AvatarBindKeyAllowlistAsset asset = ScriptableObject.CreateInstance<AvatarBindKeyAllowlistAsset>();
            AssetDatabase.CreateAsset(asset, DefaultAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return asset;
        }

        /// <summary> 绑点收集：所有非空 Key 的集合（Ordinal）。 </summary>
        public static HashSet<string> ParseToSet()
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            AvatarBindKeyAllowlistAsset asset = Load();
            if (asset == null || asset.entries == null)
            {
                return set;
            }

            foreach (AvatarBindKeyEntry e in asset.entries)
            {
                string t = e?.key?.Trim() ?? string.Empty;
                if (t.Length > 0)
                {
                    set.Add(t);
                }
            }

            return set;
        }
    }
}
