using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ET
{
    public static class ExternalDisplayExporter
    {
        public const string DefaultExcelAssetPath = "Assets/Config/Excel/ExternalDisplayConfig.xlsx";

        public static List<ExternalDisplayExportRow> CollectRows(string folder)
        {
            var rows = new List<ExternalDisplayExportRow>();
            if (string.IsNullOrWhiteSpace(folder) || !AssetDatabase.IsValidFolder(folder))
            {
                return rows;
            }

            string[] guids = AssetDatabase.FindAssets("t:FrameRoleTextureConfig", new[] { folder });
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                FrameRoleTextureConfig config = AssetDatabase.LoadAssetAtPath<FrameRoleTextureConfig>(assetPath);
                if (config == null)
                {
                    continue;
                }

                config.RebuildDisplayIds();
                CollectConfigRows(config, rows);
            }

            rows.Sort((a, b) => a.DisplayId.CompareTo(b.DisplayId));
            return rows;
        }

        private static void CollectConfigRows(FrameRoleTextureConfig config, List<ExternalDisplayExportRow> rows)
        {
            if (config.races == null)
            {
                return;
            }

            for (int r = 0; r < config.races.Count; r++)
            {
                FrameRoleRaceGroup raceGroup = config.races[r];
                if (raceGroup?.genders == null)
                {
                    continue;
                }

                for (int g = 0; g < raceGroup.genders.Count; g++)
                {
                    FrameRoleGenderGroup genderGroup = raceGroup.genders[g];
                    if (genderGroup?.textures == null)
                    {
                        continue;
                    }

                    for (int t = 0; t < genderGroup.textures.Count; t++)
                    {
                        FrameRoleTextureEntry entry = genderGroup.textures[t];
                        if (entry == null)
                        {
                            continue;
                        }

                        string name = entry.name;
                        if (string.IsNullOrWhiteSpace(name) && entry.texture != null)
                        {
                            name = entry.texture.name;
                        }

                        rows.Add(new ExternalDisplayExportRow(
                            entry.displayId,
                            genderGroup.genderKey,
                            raceGroup.raceKey,
                            name,
                            entry.desc));
                    }
                }
            }
        }
    }

    public class ExternalDisplayExportWindow : EditorWindow
    {
        private const string DefaultFolderPath = "Assets/Bundles/ScriptObject/FrameRole";

        [SerializeField] private string sourceFolder = DefaultFolderPath;
        [SerializeField] private DefaultAsset excelAsset;

        private Vector2 scrollPosition;
        private string statusMessage = string.Empty;
        private Rect folderDropRect;

        [MenuItem("Tools/Excel/外显导出Excel", false, 60)]
        public static void Open()
        {
            ExternalDisplayExportWindow window = GetWindow<ExternalDisplayExportWindow>(true, "外显导出Excel", true);
            window.minSize = new Vector2(520f, 360f);
        }

        private void OnEnable()
        {
            if (excelAsset == null)
            {
                excelAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(ExternalDisplayExporter.DefaultExcelAssetPath);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("外显导出 Excel", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "拖入包含 FrameRoleTextureConfig 的文件夹，导出到 ExternalDisplayConfig.xlsx。\n" +
                "新增 DisplayId 会追加行；已存在的 DisplayId 会更新 Gender / Race / Name / Desc 等字段（Id 不变）。",
                MessageType.Info);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("FrameRoleTextureConfig 文件夹", EditorStyles.boldLabel);
            folderDropRect = DrawDropBox(string.IsNullOrEmpty(sourceFolder) ? "拖拽文件夹到此处" : sourceFolder, 56f);
            HandleFolderDragAndDrop();

            EditorGUILayout.BeginHorizontal();
            sourceFolder = EditorGUILayout.TextField("路径", sourceFolder);
            if (GUILayout.Button("选择", GUILayout.Width(56f)))
            {
                string picked = EditorUtility.OpenFolderPanel("选择 FrameRoleTextureConfig 文件夹", "Assets", string.Empty);
                if (!string.IsNullOrEmpty(picked))
                {
                    sourceFolder = ToAssetRelativePath(picked);
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);
            excelAsset = (DefaultAsset)EditorGUILayout.ObjectField(
                "目标 Excel",
                excelAsset,
                typeof(DefaultAsset),
                false);

            EditorGUILayout.Space(8);
            using (new EditorGUI.DisabledScope(!CanExport(out _)))
            {
                if (GUILayout.Button("导出到 Excel", GUILayout.Height(34f)))
                {
                    Export();
                }
            }

            if (!string.IsNullOrEmpty(statusMessage))
            {
                EditorGUILayout.Space(8);
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
                EditorGUILayout.TextArea(statusMessage, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        private void Export()
        {
            if (!CanExport(out string excelPath))
            {
                return;
            }

            try
            {
                List<ExternalDisplayExportRow> collected = ExternalDisplayExporter.CollectRows(sourceFolder);
                if (collected.Count == 0)
                {
                    statusMessage = $"目录内未找到 FrameRoleTextureConfig：\n{sourceFolder}";
                    return;
                }

                ExternalDisplayExcelAppendResult result = ExternalDisplayExcelWriter.AppendRows(excelPath, collected);
                AssetDatabase.Refresh();

                var builder = new StringBuilder();
                builder.AppendLine($"源目录：{sourceFolder}");
                builder.AppendLine($"Excel：{excelPath}");
                builder.AppendLine($"扫描配置：{AssetDatabase.FindAssets("t:FrameRoleTextureConfig", new[] { sourceFolder }).Length} 个");
                builder.AppendLine($"收集条目：{collected.Count} 条");
                builder.AppendLine($"新增：{result.AddedCount} 条");
                builder.AppendLine($"更新：{result.UpdatedCount} 条");
                builder.AppendLine($"下一个可用 Id：{result.NextIdStart}");
                statusMessage = builder.ToString();

                EditorUtility.DisplayDialog("导出完成", $"新增 {result.AddedCount} 条，更新 {result.UpdatedCount} 条。", "确定");
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
                statusMessage = exception.ToString();
                EditorUtility.DisplayDialog("导出失败", exception.Message, "确定");
            }
        }

        private bool CanExport(out string excelPath)
        {
            excelPath = GetExcelPath();
            if (string.IsNullOrEmpty(sourceFolder) || !AssetDatabase.IsValidFolder(sourceFolder))
            {
                return false;
            }

            return !string.IsNullOrEmpty(excelPath)
                && excelPath.StartsWith("Assets/")
                && excelPath.EndsWith(".xlsx", System.StringComparison.OrdinalIgnoreCase)
                && File.Exists(excelPath);
        }

        private string GetExcelPath()
        {
            if (excelAsset == null)
            {
                return ExternalDisplayExporter.DefaultExcelAssetPath;
            }

            string path = AssetDatabase.GetAssetPath(excelAsset);
            return string.IsNullOrEmpty(path) ? ExternalDisplayExporter.DefaultExcelAssetPath : path;
        }

        private void HandleFolderDragAndDrop()
        {
            Event evt = Event.current;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform)
            {
                return;
            }

            if (!folderDropRect.Contains(evt.mousePosition))
            {
                return;
            }

            string folder = ExtractFolderFromDrag();
            if (string.IsNullOrEmpty(folder))
            {
                return;
            }

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                sourceFolder = folder;
                GUI.FocusControl(null);
            }

            evt.Use();
            Repaint();
        }

        private static string ExtractFolderFromDrag()
        {
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
            {
                return null;
            }

            for (int i = 0; i < DragAndDrop.paths.Length; i++)
            {
                string path = DragAndDrop.paths[i]?.Replace('\\', '/');
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (Directory.Exists(path))
                {
                    return ToAssetRelativePath(path);
                }

                if (path.StartsWith("Assets/"))
                {
                    string dir = Path.GetDirectoryName(path)?.Replace('\\', '/');
                    if (!string.IsNullOrEmpty(dir) && AssetDatabase.IsValidFolder(dir))
                    {
                        return dir;
                    }
                }
            }

            return null;
        }

        private static string ToAssetRelativePath(string absoluteOrAssetPath)
        {
            string normalized = absoluteOrAssetPath.Replace('\\', '/');
            if (normalized.StartsWith("Assets/"))
            {
                return normalized;
            }

            string dataPath = Application.dataPath.Replace('\\', '/');
            if (normalized.StartsWith(dataPath, System.StringComparison.OrdinalIgnoreCase))
            {
                return "Assets" + normalized.Substring(dataPath.Length);
            }

            return normalized;
        }

        private static Rect DrawDropBox(string label, float height)
        {
            Rect dropArea = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, label);
            return dropArea;
        }
    }
}
