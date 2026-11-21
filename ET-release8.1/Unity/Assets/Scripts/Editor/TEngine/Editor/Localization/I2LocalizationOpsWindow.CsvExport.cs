#if UNITY_EDITOR
using System.IO;
using System.Text;
using TEngine;
using UnityEditor;
using UnityEngine;

namespace TEngine.Localization
{
    public partial class I2LocalizationOpsWindow
    {
        /// <summary>编辑器内副本（可选，便于对照）。</summary>
        private const string EditorInnerCsvAssetPath = "Assets/Editor/I2Localization/I2Languages.csv";

        /// <summary>运行时内置表：与 GameEntry 同处于 Bundles/Tools，打进 DefaultPackage。</summary>
        public const string RuntimeInnerCsvAssetPath = "Assets/Bundles/Tools/I2Localization/I2Languages.csv";

        private const string DefaultInnerCsvAssetPath = RuntimeInnerCsvAssetPath;

        private string _innerCsvAssetPath = DefaultInnerCsvAssetPath;

        private void DrawFileTabCsvSection()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("CSV 导出（运行时内置表）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "运行时表路径：Assets/Bundles/Tools/I2Localization（与 GameEntry 同包，须打进 DefaultPackage）。\n" +
                "推荐流程：内容配置保存 → 点下方「一键同步 CSV → GameEntry」→ YooAsset 构建 DefaultPackage → Play。",
                MessageType.Info);

            GUI.enabled = _sourceData != null;
            if (GUILayout.Button("一键同步 CSV → GameEntry（Bundles/Tools）", GUILayout.Height(34)))
            {
                SyncRuntimeCsvAndBindGameEntry();
            }

            GUI.enabled = true;
            EditorGUILayout.Space(4);

            _innerCsvAssetPath = EditorGUILayout.TextField("导出为 TextAsset 路径", _innerCsvAssetPath);

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _sourceData != null;
            if (GUILayout.Button("导出 CSV 到工程", GUILayout.Height(28)))
            {
                ExportCsvToProjectAsset(_innerCsvAssetPath, showDialog: true);
            }

            if (GUILayout.Button("导出到外部文件…", GUILayout.Height(28)))
            {
                ExportCsvToExternalFile();
            }

            GUI.enabled = _languageAsset != null;
            if (GUILayout.Button("定位 Inner CSV 资源", GUILayout.Height(28)))
            {
                TextAsset existing = AssetDatabase.LoadAssetAtPath<TextAsset>(_innerCsvAssetPath);
                if (existing != null)
                {
                    EditorGUIUtility.PingObject(existing);
                    Selection.activeObject = existing;
                }
                else
                {
                    EditorUtility.DisplayDialog("多语言操作", $"工程内尚无：\n{_innerCsvAssetPath}\n请先点「导出 CSV 到工程」。", "确定");
                }
            }

            GUI.enabled = _sourceData != null;
            if (GUILayout.Button("绑定到 GameEntry 预制体", GUILayout.Height(28)))
            {
                BindLocalizationToGameEntryPrefab();
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>从当前 I2 表导出 Bundles CSV 并绑定 GameEntry。</summary>
        private void SyncRuntimeCsvAndBindGameEntry()
        {
            if (_sourceData == null)
            {
                EditorUtility.DisplayDialog("多语言操作", "请先在「文件操作」加载 I2Languages 资源。", "确定");
                return;
            }

            _innerCsvAssetPath = RuntimeInnerCsvAssetPath;
            ExportCsvToProjectAsset(RuntimeInnerCsvAssetPath, showDialog: false);
            BindLocalizationToGameEntryPrefab(showBuildHint: true);
        }

        private const string GameEntryPrefabPath = "Assets/Bundles/Tools/GameEntry.prefab";

        /// <summary>GameEntry 上挂 RootModule + LocalizationManager 并引用 Inner CSV。</summary>
        private void BindLocalizationToGameEntryPrefab(bool showBuildHint = false)
        {
            string csvPath = string.IsNullOrEmpty(_innerCsvAssetPath) ? RuntimeInnerCsvAssetPath : _innerCsvAssetPath;
            TextAsset csv = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
            if (csv == null)
            {
                if (EditorUtility.DisplayDialog(
                        "多语言操作",
                        $"未找到 CSV：\n{csvPath}\n是否先导出再绑定？",
                        "导出并绑定",
                        "取消"))
                {
                    ExportCsvToProjectAsset(csvPath, showDialog: false);
                    csv = AssetDatabase.LoadAssetAtPath<TextAsset>(csvPath);
                }

                if (csv == null)
                {
                    return;
                }
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(GameEntryPrefabPath);
            if (prefabRoot == null)
            {
                EditorUtility.DisplayDialog("多语言操作", $"无法打开预制体：\n{GameEntryPrefabPath}", "确定");
                return;
            }

            try
            {
                Transform localizationNode = prefabRoot.transform.Find("Localization");
                GameObject localizationGo = localizationNode != null
                    ? localizationNode.gameObject
                    : new GameObject("Localization");

                if (localizationNode == null)
                {
                    localizationGo.transform.SetParent(prefabRoot.transform, false);
                }

                RootModule rootModule = localizationGo.GetComponent<RootModule>();
                if (rootModule == null)
                {
                    rootModule = localizationGo.AddComponent<RootModule>();
                }

                global::TEngine.LocalizationManager locManager = localizationGo.GetComponent<global::TEngine.LocalizationManager>();
                if (locManager == null)
                {
                    locManager = localizationGo.AddComponent<global::TEngine.LocalizationManager>();
                }

                SerializedObject soRoot = new SerializedObject(rootModule);
                soRoot.FindProperty("editorLanguage").enumValueIndex = (int)Language.ChineseSimplified;
                soRoot.ApplyModifiedPropertiesWithoutUndo();

                SerializedObject soLoc = new SerializedObject(locManager);
                soLoc.FindProperty("innerLocalizationCsv").objectReferenceValue = csv;
                soLoc.FindProperty("useRuntimeModule").boolValue = true;
                soLoc.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, GameEntryPrefabPath);
                string buildNote = showBuildHint
                    ? "\n\n请构建 YooAsset DefaultPackage 后再 Play（Tools → 常用 可清理 PlayerPrefs）。"
                    : "\n\n构建 DefaultPackage 后 Play。";
                EditorUtility.DisplayDialog(
                    "绑定成功",
                    $"已为 GameEntry 添加/更新 Localization 节点：\n" +
                    $"- RootModule（编辑器语言：简体中文）\n" +
                    $"- LocalizationManager（Inner CSV = {csvPath}）" +
                    buildNote,
                    "确定");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private void ExportCsvToProjectAsset(string assetPath, bool showDialog)
        {
            if (_sourceData == null)
            {
                EditorUtility.DisplayDialog("多语言操作", "请先加载 I2Languages 资源。", "确定");
                return;
            }

            if (string.IsNullOrWhiteSpace(assetPath)
                || !assetPath.StartsWith("Assets/")
                || !assetPath.EndsWith(".csv", System.StringComparison.OrdinalIgnoreCase))
            {
                EditorUtility.DisplayDialog("多语言操作", "路径须为 Assets 下以 .csv 结尾的资源路径。", "确定");
                return;
            }

            EnsureAssetFoldersExist(assetPath);

            string csv = _sourceData.Export_I2CSV(string.Empty, ',', specializationsAsRows: true, sortRows: true);
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
            File.WriteAllText(fullPath, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            if (!string.Equals(assetPath, EditorInnerCsvAssetPath, System.StringComparison.OrdinalIgnoreCase))
            {
                string editorFull = Path.GetFullPath(Path.Combine(Application.dataPath, "..", EditorInnerCsvAssetPath));
                File.WriteAllText(editorFull, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            if (File.Exists(Path.GetFullPath(Path.Combine(Application.dataPath, "..", EditorInnerCsvAssetPath))))
            {
                AssetDatabase.ImportAsset(EditorInnerCsvAssetPath, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (textAsset != null)
            {
                EditorGUIUtility.PingObject(textAsset);
                Selection.activeObject = textAsset;
            }

            if (showDialog)
            {
                EditorUtility.DisplayDialog(
                    "导出成功",
                    $"已写入：\n{assetPath}\n\n请拖到场景 LocalizationManager 的 Inner Localization Csv，并保持 useRuntimeModule 勾选。",
                    "确定");
            }

            ShowNotification(new GUIContent("CSV 已导出到工程"));
        }

        private void ExportCsvToExternalFile()
        {
            if (_sourceData == null)
            {
                EditorUtility.DisplayDialog("多语言操作", "请先加载 I2Languages 资源。", "确定");
                return;
            }

            string defaultName = Path.GetFileName(DefaultInnerCsvAssetPath);
            string dir = Path.GetDirectoryName(Path.GetFullPath(Path.Combine(Application.dataPath, "..", DefaultInnerCsvAssetPath)));
            if (string.IsNullOrEmpty(dir))
            {
                dir = Application.dataPath;
            }

            string chosen = EditorUtility.SaveFilePanel("导出 I2 CSV", dir, defaultName, "csv");
            if (string.IsNullOrEmpty(chosen))
            {
                return;
            }

            string csv = _sourceData.Export_I2CSV(string.Empty, ',', specializationsAsRows: true, sortRows: true);
            File.WriteAllText(chosen, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
            EditorUtility.DisplayDialog("导出成功", chosen, "确定");
        }

        public static void EnsureRuntimeCsvFolders()
        {
            EnsureAssetFoldersExist(RuntimeInnerCsvAssetPath);
        }

        private static void EnsureAssetFoldersExist(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            if (parts.Length < 3)
            {
                return;
            }

            string current = parts[0];
            for (int i = 1; i < parts.Length - 1; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif