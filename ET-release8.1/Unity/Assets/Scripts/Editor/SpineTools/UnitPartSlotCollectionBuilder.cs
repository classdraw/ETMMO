using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class UnitPartSlotCollectionBuilder
    {
        public const string UnitSpriteFolder =
            "Assets/Art/Spine/SPUM/Resources/Addons/Legacy/0_Unit/0_Sprite";

        public const string HorseSpriteFolder =
            "Assets/Art/Spine/SPUM/Resources/Addons/Legacy/1_Horse/0_Sprite";
        public const string OutputFolder = "Assets/Bundles/ScriptObject";
        public const string OutputAssetPath = OutputFolder + "/UnitPartSlotCollection.asset";

        private static readonly Regex FolderTokenRegex = new Regex(@"^\d+_(.+)$", RegexOptions.Compiled);

        public static bool Collect(out string message)
        {
            EnsureOutputFolder();

            var allItems = new List<(UnitPartSlot slot, UnitPartSlotItem item)>();
            int skipped = 0;

            CollectFromSpriteFolder(HorseSpriteFolder, allItems, ref skipped);
            CollectFromSpriteFolder(UnitSpriteFolder, allItems, ref skipped);

            allItems.Sort((a, b) =>
            {
                int keyCompare = ((int)a.slot).CompareTo((int)b.slot);
                if (keyCompare != 0)
                {
                    return keyCompare;
                }

                int pathCompare = string.Compare(a.item.textureAssetPath, b.item.textureAssetPath, StringComparison.OrdinalIgnoreCase);
                if (pathCompare != 0)
                {
                    return pathCompare;
                }

                return string.Compare(a.item.spriteName, b.item.spriteName, StringComparison.OrdinalIgnoreCase);
            });

            int nextId = UnitPartSlotCollectionConfig.FirstItemId;
            for (int i = 0; i < allItems.Count; i++)
            {
                (UnitPartSlot slot, UnitPartSlotItem item) pair = allItems[i];
                pair.item.id = nextId++;
                allItems[i] = pair;
            }

            var grouped = new Dictionary<int, UnitPartSlotGroup>();
            for (int i = 0; i < allItems.Count; i++)
            {
                (UnitPartSlot slot, UnitPartSlotItem item) pair = allItems[i];
                int partSlotKey = (int)pair.slot;
                if (!grouped.TryGetValue(partSlotKey, out UnitPartSlotGroup group))
                {
                    group = new UnitPartSlotGroup { partSlotKey = partSlotKey };
                    grouped.Add(partSlotKey, group);
                }

                group.items.Add(pair.item);
            }

            UnitPartSlotCollectionConfig config = AssetDatabase.LoadAssetAtPath<UnitPartSlotCollectionConfig>(OutputAssetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<UnitPartSlotCollectionConfig>();
                AssetDatabase.CreateAsset(config, OutputAssetPath);
            }

            config.nextItemId = nextId;
            config.groups = grouped.Values.OrderBy(g => g.partSlotKey).ToList();
            config.RebuildLookup();

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            message = $"收集完成：{allItems.Count} 个子图，跳过 {skipped} 个。输出：{OutputAssetPath}";
            return true;
        }

        private static void EnsureOutputFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Bundles"))
            {
                AssetDatabase.CreateFolder("Assets", "Bundles");
            }

            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/Bundles", "ScriptObject");
            }
        }

        private static void CollectFromSpriteFolder(string spriteRoot,
            List<(UnitPartSlot slot, UnitPartSlotItem item)> output, ref int skipped)
        {
            if (!AssetDatabase.IsValidFolder(spriteRoot))
            {
                return;
            }

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { spriteRoot });
            var contextByTexture = new Dictionary<string, UnitPartSlotMapping.Context>(StringComparer.OrdinalIgnoreCase);

            foreach (string guid in guids)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guid);
                if (!texturePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!TryParseSpritePath(texturePath, out bool isHorse, out string partType, out string weaponSubType))
                {
                    skipped++;
                    continue;
                }

                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                if (texture == null)
                {
                    skipped++;
                    continue;
                }

                Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(texturePath).OfType<Sprite>().ToArray();
                if (sprites.Length == 0)
                {
                    skipped++;
                    continue;
                }

                if (!contextByTexture.TryGetValue(texturePath, out UnitPartSlotMapping.Context context))
                {
                    context = new UnitPartSlotMapping.Context();
                    contextByTexture.Add(texturePath, context);
                }

                foreach (Sprite sprite in sprites)
                {
                    if (sprite == null)
                    {
                        skipped++;
                        continue;
                    }

                    string subType = ResolveSubType(partType, texture.name, sprite, sprites);
                    if (!UnitPartSlotMapping.TryResolve(isHorse, partType, weaponSubType, subType, context, out UnitPartSlot slot))
                    {
                        skipped++;
                        continue;
                    }

                    var item = new UnitPartSlotItem
                    {
                        id = 0,
                        textureAssetPath = texturePath,
                        spriteName = sprite.name,
                        sprite = sprite,
                    };

                    output.Add((slot, item));
                }
            }
        }

        private static bool TryParseSpritePath(string texturePath, out bool isHorse, out string partType, out string weaponSubType)
        {
            isHorse = false;
            partType = string.Empty;
            weaponSubType = string.Empty;

            string normalized = texturePath.Replace('\\', '/');
            if (normalized.IndexOf("/1_Horse/0_Sprite/", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                isHorse = true;
            }
            else if (normalized.IndexOf("/0_Unit/0_Sprite/", StringComparison.OrdinalIgnoreCase) < 0)
            {
                return false;
            }

            string[] parts = normalized.Split('/');
            int spriteIndex = Array.IndexOf(parts, "0_Sprite");
            if (spriteIndex < 0 || spriteIndex + 1 >= parts.Length)
            {
                return false;
            }

            string partFolder = parts[spriteIndex + 1];
            partType = StripFolderPrefix(partFolder);

            if (string.Equals(partType, "Weapons", StringComparison.OrdinalIgnoreCase) && spriteIndex + 2 < parts.Length)
            {
                weaponSubType = StripFolderPrefix(parts[spriteIndex + 2]);
            }

            // 马匹仅在 0_Body 下图集，子图名对应 Horse* 槽位
            if (isHorse)
            {
                partType = "Body";
            }

            return !string.IsNullOrEmpty(partType);
        }

        private static string StripFolderPrefix(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                return folderName;
            }

            Match match = FolderTokenRegex.Match(folderName);
            return match.Success ? match.Groups[1].Value : folderName;
        }

        private static string ResolveSubType(string partType, string textureName, Sprite sprite, Sprite[] allSprites)
        {
            string subType = sprite.name;
            var conditionTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Front", "Body", "Horse", "Left", "Right", "Back",
            };

            bool filteredCondition = !conditionTypes.Contains(sprite.name);
            if (allSprites.Length == 2 && filteredCondition)
            {
                subType = textureName;
            }

            var tokens = new[] { "Body", "Left", "Right" };
            var armorKeywords = new[] { "armor", "cloth", "pant" };
            bool isArmorOrCloth = armorKeywords.Any(k => partType.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);

            if (isArmorOrCloth && subType == sprite.name)
            {
                string[] nameParts = sprite.name.Split('_');
                if (nameParts.Length == 3 &&
                    armorKeywords.Any(k => nameParts[0].IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    string middleToken = nameParts[1];
                    if (tokens.Any(t => string.Equals(t, middleToken, StringComparison.OrdinalIgnoreCase)))
                    {
                        subType = middleToken;
                    }
                }
            }

            return subType;
        }

    }
}
