using System.IO;
using TEngine;
using UnityEditor;
using UnityEngine;

public static class TEngineSettingsProvider
{
    private const string DefaultUpdateSettingAssetPath = "Assets/Scripts/ThirdParty/TEngine/Settings/UpdateSetting.asset";

    [MenuItem("TEngine/Settings/TEngine UpdateSettings", priority = -1)]
    public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/TEngine/UpdateSettings");

    private const string SettingsPath = "Project/TEngine/UpdateSettings";

    [SettingsProvider]
    public static SettingsProvider CreateMySettingsProvider()
    {
        return new SettingsProvider(SettingsPath, SettingsScope.Project)
        {
            label = "TEngine/UpdateSettings",
            guiHandler = searchContext =>
            {
                DrawHybridCLRSettings();
                var settings = Settings.UpdateSetting;
                if (settings == null)
                {
                    EditorGUILayout.HelpBox(
                        $"未找到 UpdateSetting 资源。标准路径：{DefaultUpdateSettingAssetPath}",
                        MessageType.Error);
                    if (GUILayout.Button("创建 UpdateSetting.asset"))
                    {
                        CreateDefaultUpdateSettingAsset();
                    }

                    return;
                }

                var serializedObject = new SerializedObject(settings);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("projectName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HotUpdateAssemblies"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AOTMetaAssemblies"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LogicMainDllName"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AssemblyTextAssetExtension"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AssemblyTextAssetPath"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateStyle"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UpdateNotice"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ResDownLoadPath"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FallbackResDownLoadPath"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LoadResWayWebGL"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("isAutoAssetCopeToBuildAddress"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuildAddress"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ReplaceAssetPathWithAddress"));
                serializedObject.ApplyModifiedProperties();
                if (serializedObject.hasModifiedProperties)
                {
                    EditorUtility.SetDirty(settings);
                }
            },
            keywords = new[] { "TEngine", "Settings", "Custom" }
        };
    }

    private static void CreateDefaultUpdateSettingAsset()
    {
        EnsureAssetFolder("Assets/Scripts/ThirdParty/TEngine/Settings");

        var existing = AssetDatabase.LoadAssetAtPath<UpdateSetting>(DefaultUpdateSettingAssetPath);
        if (existing != null)
        {
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            return;
        }

        var setting = ScriptableObject.CreateInstance<UpdateSetting>();
        AssetDatabase.CreateAsset(setting, DefaultUpdateSettingAssetPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = setting;
        EditorGUIUtility.PingObject(setting);
    }

    private static void EnsureAssetFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath)?.Replace('\\', '/');
        string folderName = Path.GetFileName(folderPath);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(folderName))
        {
            return;
        }

        EnsureAssetFolder(parent);
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }

    private static void DrawHybridCLRSettings()
    {
        EditorGUILayout.BeginVertical("HelpBox");
        {
            EditorGUILayout.LabelField(new GUIContent("HybridCLR 设置", "启用或禁用热更新功能"), EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            {
                var originalColor = GUI.color;

                GUI.color = new Color(0.2f, 0.8f, 0.3f, 1f);
                if (GUILayout.Button(new GUIContent("启用 HybridCLR", "启用热更新功能"),
                        GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("启用 HybridCLR", "确定要启用 HybridCLR 热更新功能吗？", "确定", "取消"))
                    {
                        BuildDLLCommand.EnableHybridCLR();
                        Debug.Log("HybridCLR 已启用");
                    }
                }

                GUI.color = new Color(1f, 0.4f, 0.3f, 1f);
                if (GUILayout.Button(new GUIContent("禁用 HybridCLR", "禁用热更新功能"),
                        GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("禁用 HybridCLR", "确定要禁用 HybridCLR 热更新功能吗？", "确定", "取消"))
                    {
                        BuildDLLCommand.DisableHybridCLR();
                        Debug.Log("HybridCLR 已禁用");
                    }
                }

                GUI.color = originalColor;
            }
            EditorGUILayout.EndHorizontal();

            bool isHybridCLREnabled =
#if ENABLE_HYBRIDCLR
                true;
#else
                false;
#endif
            string statusText = isHybridCLREnabled ? "HybridCLR 已启用" : "HybridCLR 已禁用";
            MessageType statusType = isHybridCLREnabled ? MessageType.Info : MessageType.Warning;

            EditorGUILayout.HelpBox(statusText, statusType);
        }
        EditorGUILayout.EndVertical();
        GUILayout.Space(10);
    }
}
