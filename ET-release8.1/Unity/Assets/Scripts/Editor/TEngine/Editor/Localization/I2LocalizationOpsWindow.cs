#if UNITY_EDITOR
using System.Collections.Generic;
using TEngine;
using UnityEditor;
using UnityEngine;

namespace TEngine.Localization
{
    /// <summary>
    /// I2Languages 中间层编辑器：文件 / 语言 / 内容配置。
    /// </summary>
    public partial class I2LocalizationOpsWindow : EditorWindow
    {
        private const string DefaultAssetPath = LocalizationUtility.I2GlobalSourcesEditorPath;

        private static readonly string[] TabTitles = { "文件操作", "语言操作", "内容配置" };

        private int _selectedTab;

        private LanguageSourceAsset _languageAsset;
        private LanguageSourceData _sourceData;
        private SerializedObject _serializedAsset;
        private SerializedProperty _propLanguages;

        private Vector2 _languageScroll;
        private List<string> _languageCodePopupList;
        private string _newLanguageName = string.Empty;
        private int _addLanguagePopupIndex;

        private bool _fileTabDirty;
        private bool _languageTabDirty;
        private bool _contentTabDirty;

        private const float LangColDelete = 24f;
        private const float LangColCode = 48f;
        private const float LangColCodePicker = 220f;
        private const float LangColMove = 22f;
        private const float LangColEnable = 22f;

        [MenuItem("Tools/多语言操作", false, 200)]
        public static void Open()
        {
            var window = GetWindow<I2LocalizationOpsWindow>(false, "多语言操作", true);
            window.minSize = new Vector2(720, 420);
        }

        private void OnEnable()
        {
            if (_languageAsset == null)
            {
                LoadLanguageAsset(DefaultAssetPath);
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(4);
            _selectedTab = GUILayout.Toolbar(_selectedTab, TabTitles);
            EditorGUILayout.Space(6);

            switch (_selectedTab)
            {
                case 0:
                    DrawFileTab();
                    break;
                case 1:
                    DrawLanguageTab();
                    break;
                case 2:
                    DrawContentTab();
                    break;
            }
        }

        #region 文件操作

        private void DrawFileTab()
        {
            EditorGUILayout.LabelField("I2 语言源资源", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            var asset = (LanguageSourceAsset)EditorGUILayout.ObjectField(
                "Language Source", _languageAsset, typeof(LanguageSourceAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                BindLanguageAsset(asset);
                _fileTabDirty = true;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("从默认路径加载", GUILayout.Width(120)))
            {
                LoadLanguageAsset(DefaultAssetPath);
                _fileTabDirty = true;
            }

            if (GUILayout.Button("在 Project 中定位", GUILayout.Width(120)) && _languageAsset != null)
            {
                EditorGUIUtility.PingObject(_languageAsset);
                Selection.activeObject = _languageAsset;
            }
            EditorGUILayout.EndHorizontal();

            if (_languageAsset == null)
            {
                EditorGUILayout.HelpBox($"未找到语言源，默认路径：{DefaultAssetPath}", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField("路径", AssetDatabase.GetAssetPath(_languageAsset));
            }

            EditorGUILayout.Space(8);
            DrawTabSaveBar("保存文件页签", _fileTabDirty, SaveFileTab);
        }

        private void SaveFileTab()
        {
            if (_languageAsset == null)
            {
                EditorUtility.DisplayDialog("多语言操作", "请先指定 I2Languages 资源。", "确定");
                return;
            }

            PersistAsset();
            _fileTabDirty = false;
            ShowNotification(new GUIContent("文件页签已保存"));
        }

        #endregion

        #region 语言操作

        private void DrawLanguageTab()
        {
            if (_sourceData == null)
            {
                EditorGUILayout.HelpBox("请先在「文件操作」页签加载 I2Languages 资源。", MessageType.Info);
                if (GUILayout.Button("加载默认 I2Languages"))
                {
                    LoadLanguageAsset(DefaultAssetPath);
                }

                return;
            }

            EnsureLanguageCodePopupList();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Space(LangColDelete);
            GUILayout.Label("语言名称", EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
            GUILayout.Label("Code", EditorStyles.miniLabel, GUILayout.Width(LangColCode));
            GUILayout.Label("语言代码选择", EditorStyles.miniLabel, GUILayout.Width(LangColCodePicker));
            GUILayout.Label("序", EditorStyles.miniLabel, GUILayout.Width(LangColMove * 2));
            GUILayout.Label("启用", EditorStyles.miniLabel, GUILayout.Width(LangColEnable));
            EditorGUILayout.EndHorizontal();

            _languageScroll = EditorGUILayout.BeginScrollView(_languageScroll, GUILayout.ExpandHeight(true));

            int indexToDelete = -1;
            int moveUp = -1;
            int moveDown = -1;

            for (int i = 0; i < _sourceData.mLanguages.Count; i++)
            {
                LanguageData lang = _sourceData.mLanguages[i];
                bool enabled = lang.IsEnabled();

                GUI.color = enabled ? Color.white : new Color(1f, 1f, 1f, 0.45f);
                EditorGUILayout.BeginHorizontal(GUILayout.Height(20));

                if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(LangColDelete)))
                {
                    indexToDelete = i;
                }

                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField(lang.Name, GUILayout.MinWidth(160), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck() && !string.IsNullOrWhiteSpace(newName))
                {
                    lang.Name = newName.Trim();
                    MarkLanguageTabDirty();
                }

                if (enabled)
                {
                    EditorGUILayout.LabelField(lang.Code ?? string.Empty, EditorStyles.textField, GUILayout.Width(LangColCode));
                    string currentCode = "[" + lang.Code + "]";
                    int codeIndex = _languageCodePopupList.FindIndex(c => c.Contains(currentCode));
                    if (codeIndex < 0)
                    {
                        codeIndex = 0;
                    }

                    EditorGUI.BeginChangeCheck();
                    codeIndex = EditorGUILayout.Popup(codeIndex, _languageCodePopupList.ToArray(), GUILayout.Width(LangColCodePicker));
                    if (EditorGUI.EndChangeCheck() && codeIndex >= 0)
                    {
                        ApplyLanguageCodeFromPopup(lang, _languageCodePopupList[codeIndex]);
                        MarkLanguageTabDirty();
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    string code = EditorGUILayout.TextField(lang.Code, GUILayout.Width(LangColCode));
                    if (EditorGUI.EndChangeCheck())
                    {
                        lang.Code = code;
                        MarkLanguageTabDirty();
                    }

                    GUILayout.Space(LangColCodePicker);
                }

                GUI.enabled = i > 0;
                if (GUILayout.Button("\u25B2", EditorStyles.miniButton, GUILayout.Width(LangColMove)))
                {
                    moveUp = i;
                }

                GUI.enabled = i < _sourceData.mLanguages.Count - 1;
                if (GUILayout.Button("\u25BC", EditorStyles.miniButton, GUILayout.Width(LangColMove)))
                {
                    moveDown = i;
                }

                GUI.enabled = true;

                EditorGUI.BeginChangeCheck();
                bool toggleEnabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(LangColEnable));
                if (EditorGUI.EndChangeCheck())
                {
                    lang.SetEnabled(toggleEnabled);
                    MarkLanguageTabDirty();
                }

                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;
            }

            EditorGUILayout.EndScrollView();

            DrawAddLanguageRow();

            if (indexToDelete >= 0)
            {
                string name = _sourceData.mLanguages[indexToDelete].Name;
                if (EditorUtility.DisplayDialog("确认删除", $"确定删除语言「{name}」？相关词条译文列也会一并移除。", "删除", "取消"))
                {
                    _sourceData.RemoveLanguage(name);
                    RefreshSerializedLanguages();
                    MarkLanguageTabDirty();
                }
            }

            if (moveUp >= 0)
            {
                SwapLanguages(moveUp, moveUp - 1);
                MarkLanguageTabDirty();
            }

            if (moveDown >= 0)
            {
                SwapLanguages(moveDown, moveDown + 1);
                MarkLanguageTabDirty();
            }

            EditorGUILayout.Space(6);
            DrawTabSaveBar("保存语言页签", _languageTabDirty, SaveLanguageTab);
        }

        private void DrawAddLanguageRow()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("添加语言", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _newLanguageName = EditorGUILayout.TextField("名称", _newLanguageName);
            GUI.enabled = !string.IsNullOrWhiteSpace(_newLanguageName);
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                AddLanguageByName(_newLanguageName.Trim());
                _newLanguageName = string.Empty;
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            string codesToExclude = string.Empty;
            foreach (LanguageData lan in _sourceData.mLanguages)
            {
                codesToExclude += "[" + lan.Code + "]";
            }

            List<string> dropdown = GoogleLanguages.GetLanguagesForDropdown(_newLanguageName, codesToExclude);
            EditorGUILayout.BeginHorizontal();
            _addLanguagePopupIndex = EditorGUILayout.Popup("从列表", _addLanguagePopupIndex, dropdown.ToArray());
            if (GUILayout.Button("添加", GUILayout.Width(60)) && dropdown.Count > 0)
            {
                string formatted = GoogleLanguages.GetFormatedLanguageName(dropdown[_addLanguagePopupIndex]);
                AddLanguageByName(formatted);
                _newLanguageName = string.Empty;
            }

            EditorGUILayout.EndHorizontal();
        }

        private void AddLanguageByName(string languageName)
        {
            if (string.IsNullOrWhiteSpace(languageName) || _sourceData == null)
            {
                return;
            }

            if (_sourceData.GetLanguageIndex(languageName, false, false) >= 0)
            {
                EditorUtility.DisplayDialog("多语言操作", $"语言「{languageName}」已存在。", "确定");
                return;
            }

            _sourceData.AddLanguage(languageName, GoogleLanguages.GetLanguageCode(languageName));
            RefreshSerializedLanguages();
            MarkLanguageTabDirty();
        }

        private void SaveLanguageTab()
        {
            if (_sourceData == null || _languageAsset == null)
            {
                EditorUtility.DisplayDialog("多语言操作", "请先加载 I2Languages 资源。", "确定");
                return;
            }

            PersistAsset();
            _languageTabDirty = false;
            ShowNotification(new GUIContent("语言页签已保存"));
        }

        private static void ApplyLanguageCodeFromPopup(LanguageData lang, string popupEntry)
        {
            if (string.IsNullOrEmpty(popupEntry))
            {
                return;
            }

            int i0 = popupEntry.IndexOf('[');
            int i1 = popupEntry.IndexOf(']');
            if (i0 >= 0 && i1 > i0)
            {
                lang.Code = popupEntry.Substring(i0 + 1, i1 - i0 - 1);
            }
        }

        private void EnsureLanguageCodePopupList()
        {
            if (_languageCodePopupList != null && _languageCodePopupList.Count > 0)
            {
                return;
            }

            _languageCodePopupList = GoogleLanguages.GetLanguagesForDropdown(string.Empty, string.Empty);
            _languageCodePopupList.Sort();
            _languageCodePopupList.Insert(0, string.Empty);
        }

        private void SwapLanguages(int first, int second)
        {
            SwapList(_sourceData.mLanguages, first, second);
            foreach (TermData term in _sourceData.mTerms)
            {
                SwapArray(term.Languages, first, second);
                SwapArray(term.Flags, first, second);
            }

            RefreshSerializedLanguages();
        }

        private static void SwapList(List<LanguageData> list, int a, int b)
        {
            (list[a], list[b]) = (list[b], list[a]);
        }

        private static void SwapArray(string[] array, int a, int b)
        {
            (array[a], array[b]) = (array[b], array[a]);
        }

        private static void SwapArray(byte[] array, int a, int b)
        {
            (array[a], array[b]) = (array[b], array[a]);
        }

        #endregion

        #region 内容配置

        private void SaveContentTab()
        {
            if (_languageAsset == null)
            {
                EditorUtility.DisplayDialog("多语言操作", "请先加载 I2Languages 资源。", "确定");
                return;
            }

            _sourceData?.UpdateDictionary(true);
            PersistAsset();
            _contentTabDirty = false;
            ShowNotification(new GUIContent("内容页签已保存"));
        }

        private void MarkContentTabDirty()
        {
            _contentTabDirty = true;
        }

        #endregion

        #region Asset

        private void LoadLanguageAsset(string assetPath)
        {
            LanguageSourceAsset asset = AssetDatabase.LoadAssetAtPath<LanguageSourceAsset>(assetPath);
            BindLanguageAsset(asset);
        }

        private void BindLanguageAsset(LanguageSourceAsset asset)
        {
            _languageAsset = asset;
            _sourceData = asset != null ? asset.mSource : null;

            if (_sourceData != null && asset != null)
            {
                _sourceData.owner = asset;
            }

            _serializedAsset = asset != null ? new SerializedObject(asset) : null;
            _propLanguages = _serializedAsset?.FindProperty("mSource.mLanguages");
            _languageTabDirty = false;
            _fileTabDirty = false;
            _contentTabDirty = false;
            _selectedTermIndex = -1;
            _contentTermFilter = string.Empty;
            _contentTermPage = 0;
        }

        private void RefreshSerializedLanguages()
        {
            _serializedAsset?.Update();
        }

        private void PersistAsset()
        {
            if (_languageAsset == null)
            {
                return;
            }

            _sourceData?.Editor_SetDirty();
            EditorUtility.SetDirty(_languageAsset);
            _serializedAsset?.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void MarkLanguageTabDirty()
        {
            _languageTabDirty = true;
        }

        private static void DrawTabSaveBar(string label, bool dirty, System.Action onSave)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = dirty;
            if (GUILayout.Button(label, GUILayout.Height(28)))
            {
                onSave?.Invoke();
            }

            GUI.enabled = true;
            if (dirty)
            {
                GUILayout.Label("* 有未保存的修改", EditorStyles.miniLabel);
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion
    }
}
#endif
