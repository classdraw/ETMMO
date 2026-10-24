#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TEngine.Localization
{
    public partial class I2LocalizationOpsWindow
    {
        private const string ContentSpecializationAny = "Any";
        private const int ContentTermsPageSize = 20;

        private Vector2 _contentTermListScroll;
        private Vector2 _contentTranslationScroll;
        private string _contentTermFilter = string.Empty;
        private string _contentNewTermName = string.Empty;
        private int _selectedTermIndex = -1;
        private int _contentTermPage;
        private bool _contentShowDisabledLanguages;
        private bool _contentDescriptionExpanded;

        private void DrawContentTab()
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

            _sourceData.UpdateDictionary();

            EditorGUILayout.LabelField("词条翻译配置", EditorStyles.boldLabel);

            DrawSelectedTermTranslations();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("词条列表", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            GUI.backgroundColor = new Color(1f, 0.75f, 0.75f);
            if (GUILayout.Button("删除所有词条", GUILayout.Width(100)))
            {
                DeleteAllTerms();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _contentTermFilter = EditorGUILayout.TextField(
                new GUIContent("筛选列表", "模糊匹配词条名、Description、各语言译文（空格分隔多个关键词）"),
                _contentTermFilter);
            if (EditorGUI.EndChangeCheck())
            {
                _contentTermPage = 0;
            }

            if (GUILayout.Button("清空筛选", GUILayout.Width(72)))
            {
                _contentTermFilter = string.Empty;
                _contentTermPage = 0;
            }

            EditorGUILayout.EndHorizontal();

            List<int> visibleIndices = BuildVisibleTermIndices();
            ClampSelectedTermIndex(visibleIndices);

            int totalPages = GetTotalPages(visibleIndices.Count);
            _contentTermPage = Mathf.Clamp(_contentTermPage, 0, Mathf.Max(0, totalPages - 1));
            DrawTermListPagination(visibleIndices.Count, totalPages);

            int pageStart = _contentTermPage * ContentTermsPageSize;
            int pageEnd = Mathf.Min(pageStart + ContentTermsPageSize, visibleIndices.Count);

            _contentTermListScroll = EditorGUILayout.BeginScrollView(_contentTermListScroll, GUILayout.MinHeight(140), GUILayout.ExpandHeight(true));
            for (int vi = pageStart; vi < pageEnd; vi++)
            {
                int termIndex = visibleIndices[vi];
                TermData term = _sourceData.mTerms[termIndex];
                bool selected = _selectedTermIndex == termIndex;

                if (GUILayout.Button(term.Term, selected ? EditorStyles.toolbarButton : EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
                {
                    _selectedTermIndex = termIndex;
                }
            }

            if (visibleIndices.Count == 0)
            {
                EditorGUILayout.HelpBox("没有匹配的词条。", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            _contentNewTermName = EditorGUILayout.TextField("新词条", _contentNewTermName);
            if (GUILayout.Button("添加词条", GUILayout.Width(80)) && !string.IsNullOrWhiteSpace(_contentNewTermName))
            {
                string termName = _contentNewTermName.Trim();
                TermData added = _sourceData.AddTerm(termName, eTermType.Text, false);
                if (added != null)
                {
                    added.Validate();
                    _selectedTermIndex = _sourceData.mTerms.IndexOf(added);
                    _contentNewTermName = string.Empty;
                    MarkContentTabDirty();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            DrawTabSaveBar("保存内容页签", _contentTabDirty, SaveContentTab);
        }

        private void DrawSelectedTermTranslations()
        {
            if (_selectedTermIndex < 0 || _selectedTermIndex >= _sourceData.mTerms.Count)
            {
                EditorGUILayout.HelpBox("在下方列表中选中一个词条，在此编辑各语言译文。", MessageType.Info);
                return;
            }

            TermData term = _sourceData.mTerms[_selectedTermIndex];
            term.Validate();

            GUI.backgroundColor = new Color(0.85f, 0.92f, 1f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            EditorGUILayout.LabelField("当前词条", term.Term, EditorStyles.boldLabel);

            _contentDescriptionExpanded = EditorGUILayout.Foldout(_contentDescriptionExpanded, "Description", true);
            if (_contentDescriptionExpanded)
            {
                EditorGUI.BeginChangeCheck();
                string newDesc = EditorGUILayout.TextArea(term.Description ?? string.Empty, GUILayout.MinHeight(48));
                if (EditorGUI.EndChangeCheck())
                {
                    term.Description = newDesc;
                    MarkContentTabDirty();
                }
            }
            else if (!string.IsNullOrEmpty(term.Description))
            {
                EditorGUILayout.HelpBox(term.Description, MessageType.None);
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Specialization", GUILayout.Width(100));
            GUILayout.Label(ContentSpecializationAny, EditorStyles.textField, GUILayout.Width(80));
            GUILayout.FlexibleSpace();
            _contentShowDisabledLanguages = GUILayout.Toggle(
                _contentShowDisabledLanguages,
                new GUIContent("L", "显示已禁用的语言"),
                EditorStyles.miniButton,
                GUILayout.Width(24));
            EditorGUILayout.EndHorizontal();

            _contentTranslationScroll = EditorGUILayout.BeginScrollView(_contentTranslationScroll, GUILayout.MinHeight(120), GUILayout.MaxHeight(220));

            for (int langIndex = 0; langIndex < _sourceData.mLanguages.Count; langIndex++)
            {
                LanguageData lang = _sourceData.mLanguages[langIndex];
                bool langEnabled = lang.IsEnabled();
                if (!langEnabled && !_contentShowDisabledLanguages)
                {
                    continue;
                }

                if (!langEnabled)
                {
                    GUI.color = new Color(1f, 1f, 1f, 0.4f);
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(lang.Name, GUILayout.Width(140));

                string translation = term.GetTranslation(langIndex, ContentSpecializationAny, editMode: true) ?? string.Empty;
                EditorGUI.BeginChangeCheck();
                string newTranslation = EditorGUILayout.TextArea(translation, GUILayout.MinHeight(36), GUILayout.ExpandWidth(true));
                if (EditorGUI.EndChangeCheck())
                {
                    term.SetTranslation(langIndex, newTranslation, ContentSpecializationAny);
                    MarkContentTabDirty();
                }

                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("删除词条", GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("确认删除", $"确定删除词条「{term.Term}」？", "删除", "取消"))
                {
                    _sourceData.RemoveTerm(term.Term);
                    _selectedTermIndex = -1;
                    _sourceData.UpdateDictionary(true);
                    MarkContentTabDirty();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private List<int> BuildVisibleTermIndices()
        {
            var indices = new List<int>();
            string filter = _contentTermFilter?.Trim();
            if (string.IsNullOrEmpty(filter))
            {
                for (int i = 0; i < _sourceData.mTerms.Count; i++)
                {
                    indices.Add(i);
                }

                return indices;
            }

            string[] keywords = filter.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < _sourceData.mTerms.Count; i++)
            {
                if (TermMatchesFuzzyFilter(_sourceData.mTerms[i], keywords))
                {
                    indices.Add(i);
                }
            }

            return indices;
        }

        private bool TermMatchesFuzzyFilter(TermData term, string[] keywords)
        {
            if (keywords == null || keywords.Length == 0)
            {
                return true;
            }

            term.Validate();
            var searchable = new System.Text.StringBuilder();
            searchable.Append(term.Term).Append('\n');
            if (!string.IsNullOrEmpty(term.Description))
            {
                searchable.Append(term.Description).Append('\n');
            }

            for (int langIndex = 0; langIndex < term.Languages.Length; langIndex++)
            {
                string text = term.GetTranslation(langIndex, ContentSpecializationAny, editMode: true);
                if (!string.IsNullOrEmpty(text))
                {
                    searchable.Append(text).Append('\n');
                }
            }

            string haystack = searchable.ToString();
            for (int k = 0; k < keywords.Length; k++)
            {
                if (haystack.IndexOf(keywords[k], System.StringComparison.OrdinalIgnoreCase) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private void ClampSelectedTermIndex(List<int> visibleIndices)
        {
            if (_selectedTermIndex < 0)
            {
                return;
            }

            if (!visibleIndices.Contains(_selectedTermIndex))
            {
                _selectedTermIndex = visibleIndices.Count > 0 ? visibleIndices[0] : -1;
            }
        }

        private static int GetTotalPages(int itemCount)
        {
            if (itemCount <= 0)
            {
                return 1;
            }

            return (itemCount + ContentTermsPageSize - 1) / ContentTermsPageSize;
        }

        private void DrawTermListPagination(int visibleCount, int totalPages)
        {
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = _contentTermPage > 0;
            if (GUILayout.Button("首页", EditorStyles.miniButtonLeft, GUILayout.Width(40)))
            {
                _contentTermPage = 0;
            }

            if (GUILayout.Button("上一页", EditorStyles.miniButtonMid, GUILayout.Width(48)))
            {
                _contentTermPage--;
            }

            GUI.enabled = true;
            GUILayout.Label(
                $"第 {_contentTermPage + 1} / {totalPages} 页（共 {visibleCount} 条，每页 {ContentTermsPageSize} 条）",
                EditorStyles.miniLabel,
                GUILayout.ExpandWidth(true));

            GUI.enabled = _contentTermPage < totalPages - 1;
            if (GUILayout.Button("下一页", EditorStyles.miniButtonMid, GUILayout.Width(48)))
            {
                _contentTermPage++;
            }

            if (GUILayout.Button("末页", EditorStyles.miniButtonRight, GUILayout.Width(40)))
            {
                _contentTermPage = totalPages - 1;
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DeleteAllTerms()
        {
            int count = _sourceData.mTerms.Count;
            if (count == 0)
            {
                EditorUtility.DisplayDialog("删除所有词条", "当前没有任何词条。", "确定");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    "删除所有词条",
                    $"将删除全部 {count} 个词条，且不可撤销（需保存后写入资源）。\n确定继续？",
                    "全部删除",
                    "取消"))
            {
                return;
            }

            _sourceData.mTerms.Clear();
            _sourceData.UpdateDictionary(true);
            _selectedTermIndex = -1;
            _contentTermPage = 0;
            MarkContentTabDirty();
        }
    }
}
#endif
