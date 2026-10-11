using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// SPUM / Spine 相关编辑器工具。
    /// </summary>
    public class SpineToolsWindow : EditorWindow
    {
        private static readonly string[] TabNames = { "收集角色部件" };

        private int selectedTab;

        [SerializeField]
        private UnityEngine.GameObject matchingListPrefab;

        [MenuItem("Tools/Spine", false, 54)]
        public static void Open()
        {
            var window = GetWindow<SpineToolsWindow>(true, "Spine", true);
            window.minSize = new Vector2(480, 360);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            selectedTab = GUILayout.Toolbar(selectedTab, TabNames);
            EditorGUILayout.Space(8);

            switch (selectedTab)
            {
                case 0:
                    DrawCollectCharacterPartsTab();
                    break;
            }
        }

        private void DrawCollectCharacterPartsTab()
        {
            EditorGUILayout.LabelField("Legacy 资源目录", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            DrawReadOnlyFolderRow(UnitPartSlotCollectionBuilder.HorseSpriteFolder);
            DrawReadOnlyFolderRow(UnitPartSlotCollectionBuilder.UnitSpriteFolder);

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Matching 预制体", EditorStyles.boldLabel);
            matchingListPrefab = (UnityEngine.GameObject)EditorGUILayout.ObjectField(
                "SPUM_MatchingList 预制体",
                matchingListPrefab,
                typeof(UnityEngine.GameObject),
                false);

            EditorGUILayout.Space(8);
            if (GUILayout.Button("收集", GUILayout.Height(28)))
            {
                if (!UnitPartSlotCollectionBuilder.Collect(out string message))
                {
                    EditorUtility.DisplayDialog("Spine 收集", message, "确定");
                    return;
                }

                if (matchingListPrefab != null)
                {
                    if (!SpineDataConfigBuilder.TryBuildFromMatchingPrefab(matchingListPrefab, out string spineDataMessage))
                    {
                        EditorUtility.DisplayDialog("Spine 收集", message + "\n\n" + spineDataMessage, "确定");
                        return;
                    }

                    message = message + "\n\n" + spineDataMessage;
                }

                EditorUtility.DisplayDialog("Spine 收集", message, "确定");
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(
                    matchingListPrefab != null
                        ? SpineDataConfigBuilder.OutputAssetPath
                        : UnitPartSlotCollectionBuilder.OutputAssetPath);
                if (asset == null)
                {
                    asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(UnitPartSlotCollectionBuilder.OutputAssetPath);
                }

                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                    Selection.activeObject = asset;
                }
            }

            if (GUILayout.Button("导出到 ExternalDisplayConfig", GUILayout.Height(28)))
            {
                if (UnitPartSlotCollectionExcelExporter.ExportToExternalDisplay(out string exportMessage))
                {
                    EditorUtility.DisplayDialog("导出 Excel", exportMessage, "确定");
                }
                else
                {
                    EditorUtility.DisplayDialog("导出 Excel", exportMessage, "确定");
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "扫描上述 0_Sprite 目录内 PNG 的子图（Sprite），按 UnitPartSlot 分组写入 "
                + UnitPartSlotCollectionBuilder.OutputAssetPath
                + "；导出会清空 ExternalDisplayConfig.xlsx 旧数据后全量写入（C~F：Id、PartSlotKey、SpriteName、desc）。\n"
                + "若指定带 SPUM_MatchingList 的预制体，会额外写入 "
                + SpineDataConfigBuilder.OutputAssetPath
                + "（slotPart=行序，index=MatchingElement.Index）。",
                MessageType.Info);
        }

        private static void DrawReadOnlyFolderRow(string assetsFolderPath)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(assetsFolderPath);
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("定位", GUILayout.Width(52)))
            {
                PingFolder(assetsFolderPath);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void PingFolder(string assetsFolderPath)
        {
            UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetsFolderPath);
            if (folder == null)
            {
                EditorUtility.DisplayDialog("定位失败", "找不到文件夹：\n" + assetsFolderPath, "确定");
                return;
            }

            EditorGUIUtility.PingObject(folder);
            Selection.activeObject = folder;
        }
    }
}
