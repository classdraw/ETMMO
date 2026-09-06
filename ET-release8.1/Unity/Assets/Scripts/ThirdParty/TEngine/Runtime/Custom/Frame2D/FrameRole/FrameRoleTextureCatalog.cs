using System;
using System.Collections.Generic;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// 汇总各部位 ScriptableObject。Excel 的 display 解码出部位后，从这里取对应配置再取图。
    /// </summary>
    [CreateAssetMenu(fileName = "FrameRoleTextureCatalog", menuName = "Tools/Frame2D/Frame Role Texture Catalog", order = 211)]
    public class FrameRoleTextureCatalog : ScriptableObject
    {
        public List<FrameRoleTextureConfig> partConfigs = new List<FrameRoleTextureConfig>();

        [NonSerialized]
        private Dictionary<int, FrameRoleTextureConfig> partLookup;

        public void RebuildLookup()
        {
            if (partLookup == null)
            {
                partLookup = new Dictionary<int, FrameRoleTextureConfig>();
            }
            else
            {
                partLookup.Clear();
            }

            for (int i = 0; i < partConfigs.Count; i++)
            {
                FrameRoleTextureConfig config = partConfigs[i];
                if (config == null)
                {
                    continue;
                }

                partLookup[config.partKey] = config;
                config.RebuildDisplayIds();
                config.RebuildLookup();
            }
        }

        public bool TryGetPartConfig(int partKey, out FrameRoleTextureConfig config)
        {
            if (partLookup == null)
            {
                RebuildLookup();
            }

            return partLookup.TryGetValue(partKey, out config);
        }

        public bool TryGetEntry(int displayId, out FrameRoleTextureEntry entry)
        {
            entry = null;
            if (!FrameRoleTextureDisplay.TryDecode(displayId, out int partKey, out _, out _, out _))
            {
                return false;
            }

            return TryGetPartConfig(partKey, out FrameRoleTextureConfig config) && config.TryGetEntry(displayId, out entry);
        }

        public bool TryGetTexture(int displayId, out Texture2D texture)
        {
            if (TryGetEntry(displayId, out FrameRoleTextureEntry entry) && entry.texture != null)
            {
                texture = entry.texture;
                return true;
            }

            texture = null;
            return false;
        }

        private void OnEnable()
        {
            partLookup = null;
        }

        private void OnValidate()
        {
            partLookup = null;
        }
    }
}
