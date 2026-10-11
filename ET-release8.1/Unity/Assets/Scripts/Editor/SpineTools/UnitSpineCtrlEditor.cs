using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    [CustomEditor(typeof(UnitSpineCtrl))]
    public class UnitSpineCtrlEditor : UnityEditor.Editor
    {
        private SerializedProperty itemsProp;

        private void OnEnable()
        {
            itemsProp = serializedObject.FindProperty("items");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(itemsProp, true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("收集", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("收集 SpriteRenderer", GUILayout.Height(28)))
                {
                    if (EditorUtility.DisplayDialog(
                            "收集 SpriteRenderer",
                            "将清空并重新扫描当前物体下所有 SpriteRenderer（含未激活）。是否继续？",
                            "收集",
                            "取消"))
                    {
                        CollectRenderers((UnitSpineCtrl)target);
                    }
                }

                if (GUILayout.Button("按 SlotPart 排序"))
                {
                    SortBySlotPart((UnitSpineCtrl)target);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void CollectRenderers(UnitSpineCtrl ctrl)
        {
            if (ctrl == null)
            {
                return;
            }

            Undo.RecordObject(ctrl, "Collect UnitSpineCtrl Items");

            var preserved = new Dictionary<SpriteRenderer, UnitSpineCtrlItem>();
            for (int i = 0; i < ctrl.items.Count; i++)
            {
                UnitSpineCtrlItem row = ctrl.items[i];
                if (row?.render == null || preserved.ContainsKey(row.render))
                {
                    continue;
                }

                preserved[row.render] = row;
            }

            var mappingContext = new UnitPartSlotMapping.Context();
            int unresolvedCount = 0;

            ctrl.items.Clear();
            SpriteRenderer[] renderers = ctrl.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer sr = renderers[i];
                preserved.TryGetValue(sr, out UnitSpineCtrlItem oldRow);

                string itemPath = UnitSpineCtrlCollectUtil.GetHierarchyPath(ctrl.transform, sr.transform);

                if (!UnitSpineCtrlSlotResolver.TryResolveSlotPart(sr, ctrl.transform, mappingContext, out int slotPart))
                {
                    slotPart = oldRow != null ? oldRow.slotPart : -1;
                    unresolvedCount++;
                }

                ctrl.items.Add(new UnitSpineCtrlItem
                {
                    slotPart = slotPart,
                    render = sr,
                    itemPath = itemPath,
                    color = sr != null ? sr.color : Color.white,
                });
            }

            SortItemsBySlotPart(ctrl.items);

            EditorUtility.SetDirty(ctrl);
            Debug.Log(unresolvedCount > 0
                ? $"[UnitSpineCtrl] 收集完成：{ctrl.items.Count} 个 SpriteRenderer，{unresolvedCount} 个未能解析 SlotPart（slotPart=-1，请手填）。"
                : $"[UnitSpineCtrl] 收集完成：{ctrl.items.Count} 个 SpriteRenderer。");
        }

        private static void SortBySlotPart(UnitSpineCtrl ctrl)
        {
            if (ctrl?.items == null)
            {
                return;
            }

            Undo.RecordObject(ctrl, "Sort UnitSpineCtrl Items");
            SortItemsBySlotPart(ctrl.items);
            EditorUtility.SetDirty(ctrl);
        }

        private static void SortItemsBySlotPart(List<UnitSpineCtrlItem> items)
        {
            items.Sort((a, b) =>
            {
                int sa = a.slotPart < 0 ? int.MaxValue : a.slotPart;
                int sb = b.slotPart < 0 ? int.MaxValue : b.slotPart;
                int cmp = sa.CompareTo(sb);
                if (cmp != 0)
                {
                    return cmp;
                }

                return string.Compare(a.itemPath, b.itemPath, System.StringComparison.OrdinalIgnoreCase);
            });
        }
    }

    internal static class UnitSpineCtrlCollectUtil
    {
        /// <summary>
        /// 从 root 到 target 的层级路径，供 transform.Find 使用（如 BodySet/P_Body/P_Back/Back）。
        /// </summary>
        public static string GetHierarchyPath(Transform root, Transform target)
        {
            if (root == null || target == null)
            {
                return string.Empty;
            }

            if (target == root)
            {
                return string.Empty;
            }

            if (!target.IsChildOf(root))
            {
                return string.Empty;
            }

            var segments = new System.Collections.Generic.List<string>();
            for (Transform current = target; current != null && current != root; current = current.parent)
            {
                segments.Add(current.name);
            }

            if (segments.Count == 0)
            {
                return string.Empty;
            }

            segments.Reverse();
            return string.Join("/", segments);
        }

        private const string AddonsResourcesPrefix = "Resources/Addons/";

        /// <summary>
        /// 贴图 Assets 路径，仅用于 SlotPart 推断。
        /// </summary>
        public static string ResolveSpriteAssetPath(SpriteRenderer renderer)
        {
            if (renderer == null || renderer.sprite == null)
            {
                return string.Empty;
            }

            string assetPath = AssetDatabase.GetAssetPath(renderer.sprite);
            if (string.IsNullOrEmpty(assetPath))
            {
                assetPath = AssetDatabase.GetAssetPath(renderer.sprite.texture);
            }

            return ToSpumItemPath(assetPath);
        }

        public static string ToSpumItemPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return string.Empty;
            }

            string normalized = assetPath.Replace('\\', '/');
            int addonsIndex = normalized.IndexOf(AddonsResourcesPrefix, System.StringComparison.OrdinalIgnoreCase);
            if (addonsIndex >= 0)
            {
                return StripTextureExtension(normalized.Substring(addonsIndex + AddonsResourcesPrefix.Length));
            }

            const string legacyToken = "Legacy/";
            int legacyIndex = normalized.IndexOf(legacyToken, System.StringComparison.OrdinalIgnoreCase);
            if (legacyIndex >= 0)
            {
                return StripTextureExtension(normalized.Substring(legacyIndex));
            }

            const string addonsToken = "Addons/";
            int addonsShort = normalized.IndexOf(addonsToken, System.StringComparison.OrdinalIgnoreCase);
            if (addonsShort >= 0)
            {
                normalized = normalized.Substring(addonsShort + addonsToken.Length);
            }

            return StripTextureExtension(normalized);
        }

        private static string StripTextureExtension(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            if (path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                return path.Substring(0, path.Length - 4);
            }

            return System.IO.Path.ChangeExtension(path, null);
        }
    }
}
