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
            return AssetDatabase.LoadAssetAtPath<AvatarBindKeyAllowlistAsset>(DefaultAssetPath);
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

        public static HashSet<string> ParseToSet()
        {
            var set = new HashSet<string>(StringComparer.Ordinal);
            AvatarBindKeyAllowlistAsset asset = Load();
            if (asset == null || asset.keys == null)
            {
                return set;
            }

            foreach (string s in asset.keys)
            {
                string t = s?.Trim() ?? string.Empty;
                if (t.Length > 0)
                {
                    set.Add(t);
                }
            }

            return set;
        }
    }
}
