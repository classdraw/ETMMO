using TEngine.Localization;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>Unity 编辑器常用维护操作。</summary>
    public class CommonToolsWindow : EditorWindow
    {
        [MenuItem("Tools/常用", false, 5)]
        public static void Open()
        {
            var window = GetWindow<CommonToolsWindow>(true, "常用", true);
            window.minSize = new Vector2(360, 200);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "开发期维护：清理本地缓存后，下次 Play 会按 Init 场景语言、GameEntry 等配置重新初始化（不再沿用旧的 I2 语言存档）。",
                MessageType.Info);
            EditorGUILayout.Space(8);

            if (GUILayout.Button("清理 PlayerPrefs（含 I2 Language 等）", GUILayout.Height(36)))
            {
                TryClearPlayerPrefs();
            }

            EditorGUILayout.Space(4);
            if (GUILayout.Button("仅删除 I2 语言存档 (I2 Language)", GUILayout.Height(30)))
            {
                TryClearI2LanguageOnly();
            }

        }

        private static void TryClearPlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog(
                    "清理 PlayerPrefs",
                    "将删除本 Unity 项目在编辑器下的全部 PlayerPrefs（含 I2 记住的语言、各类本地开关等）。\n\n确定继续？",
                    "清理",
                    "取消"))
            {
                return;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[常用工具] 已清理全部 PlayerPrefs。");
            EditorUtility.DisplayDialog("常用", "PlayerPrefs 已清理完成。", "确定");
        }

        private static void TryClearI2LanguageOnly()
        {
            if (!EditorUtility.DisplayDialog(
                    "删除 I2 语言存档",
                    $"仅删除键「{ClientViewDebugSettings.I2SavedLanguageKey}」，不影响其他 PlayerPrefs。\n\n确定？",
                    "删除",
                    "取消"))
            {
                return;
            }

            PersistentStorage.DeleteSetting(ClientViewDebugSettings.I2SavedLanguageKey);
            PersistentStorage.ForceSaveSettings();
            Debug.Log($"[常用工具] 已删除 I2 语言存档：{ClientViewDebugSettings.I2SavedLanguageKey}");
            EditorUtility.DisplayDialog("常用", "I2 语言存档已删除。", "确定");
        }
    }
}