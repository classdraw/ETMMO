using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// Synty 模块化角色：拆解、默认部件配置导入。
    /// </summary>
    public class SyntyModularProcessingWindow : EditorWindow
    {
        const string SourcePrefabEditorPrefKey = "ET.SyntyModularProcessing.SourcePrefabPath";

        GameObject sourceCharacterPrefab;
        List<string> roleDefaultPartNames = new List<string>();
        ReorderableList partsReorderableList;
        Vector2 scrollPosition;
        string cachedKeyForPartsList;

        [MenuItem("Tools/预制体处理/Synty模块化处理", false, 51)]
        public static void Open()
        {
            var w = GetWindow<SyntyModularProcessingWindow>(true, "Synty模块化处理", true);
            w.minSize = new Vector2(480, 520);
        }

        void OnEnable()
        {
            LoadSourcePrefabFromPrefs();
            BuildPartsList();
            cachedKeyForPartsList = GetEffectiveRoleDefaultPartsKey();
        }

        void LoadSourcePrefabFromPrefs()
        {
            string path = EditorPrefs.GetString(SourcePrefabEditorPrefKey, SyntyModularCharacterDismantleTool.SourcePrefabPath);
            sourceCharacterPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (sourceCharacterPrefab == null)
            {
                sourceCharacterPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(SyntyModularCharacterDismantleTool.SourcePrefabPath);
            }
        }

        void SaveSourcePrefabToPrefs()
        {
            if (sourceCharacterPrefab == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(sourceCharacterPrefab);
            if (!string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString(SourcePrefabEditorPrefKey, path);
            }
        }

        void BuildPartsList()
        {
            partsReorderableList = new ReorderableList(roleDefaultPartNames, typeof(string), true, true, true, true)
            {
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "默认显示部件（Chr_*）"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    roleDefaultPartNames[index] = EditorGUI.TextField(rect, roleDefaultPartNames[index]);
                },
                onAddCallback = list =>
                {
                    roleDefaultPartNames.Add(string.Empty);
                },
                onRemoveCallback = list =>
                {
                    if (list.index >= 0 && list.index < roleDefaultPartNames.Count)
                    {
                        roleDefaultPartNames.RemoveAt(list.index);
                    }
                },
            };
        }

        void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "指定 Synty 模块化角色预制体（名称 _F / _M）后：应用性别、拆解 RolePart、读写默认部件配置。\n" +
                $"配置：{RoleDefaultVisiblePartsConfig.DefaultAssetPath}",
                MessageType.Info);

            EditorGUILayout.LabelField("角色预制体", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            sourceCharacterPrefab = (GameObject)EditorGUILayout.ObjectField(
                "预制体",
                sourceCharacterPrefab,
                typeof(GameObject),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                SaveSourcePrefabToPrefs();
                SyncPartsListWhenKeyChanged();
            }

            using (new EditorGUI.DisabledScope(sourceCharacterPrefab == null))
            {
                if (GUILayout.Button("跳转到模型位置", GUILayout.Height(26)))
                {
                    PingSourcePrefab();
                }
            }

            string sourceLabel = sourceCharacterPrefab != null
                ? AssetDatabase.GetAssetPath(sourceCharacterPrefab)
                : "（未指定）";
            EditorGUILayout.LabelField("资源路径", sourceLabel, EditorStyles.miniLabel);

            if (sourceCharacterPrefab != null &&
                SyntyModularRolePartUtil.TryParseRolePrefabGender(sourceCharacterPrefab.name, out bool applyFemale, out _))
            {
                EditorGUILayout.LabelField(
                    "性别",
                    applyFemale ? "女（_F）→ Female_Parts" : "男（_M）→ Male_Parts",
                    EditorStyles.miniLabel);
            }
            else if (sourceCharacterPrefab != null)
            {
                EditorGUILayout.HelpBox("预制体名称须以 _F 或 _M 结尾。", MessageType.Warning);
            }

            using (new EditorGUI.DisabledScope(sourceCharacterPrefab == null ||
                                                !SyntyModularRolePartUtil.TryParseRolePrefabGender(
                                                    sourceCharacterPrefab?.name,
                                                    out _,
                                                    out _)))
            {
                if (GUILayout.Button("应用性别到预制体", GUILayout.Height(30)))
                {
                    SyntyModularGenderSyncTool.ApplyGenderToPrefab(sourceCharacterPrefab);
                }
            }

            EditorGUILayout.Space(6);
            using (new EditorGUI.DisabledScope(sourceCharacterPrefab == null))
            {
                if (GUILayout.Button("拆解指定模型 → Role / RolePart", GUILayout.Height(36)))
                {
                    SyntyModularCharacterDismantleTool.DismantleModularCharacter(sourceCharacterPrefab);
                }

                if (GUILayout.Button("从指定模型读到下列列表", GUILayout.Height(30)))
                {
                    LoadPartsFromSourceModel();
                }
            }

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("单条 Role 默认部件（Female_Parts / Male_Parts 显示网格）", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Key = 上方预制体名称（只读）；后缀 _F/_M 决定读 Female_Parts 或 Male_Parts。\n" +
                "「从模型读到列表」收集 activeSelf 部件；切换预制体时自动从配置刷新列表。",
                MessageType.None);

            DrawRolePrefabKeyReadOnly();

            string keyForGender = GetEffectiveRoleDefaultPartsKey();
            if (SyntyModularRolePartUtil.TryParseRolePrefabGender(keyForGender, out bool keyIsFemale, out _))
            {
                EditorGUILayout.LabelField(
                    "部件路径",
                    keyIsFemale
                        ? "Female_Parts / RolePart/Female_*"
                        : "Male_Parts / RolePart/Male_*",
                    EditorStyles.miniLabel);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("从配置读取", GUILayout.Height(28)))
                {
                    LoadPartsFromConfig();
                }

                if (GUILayout.Button("清空列表", GUILayout.Height(28)))
                {
                    ReplacePartList(System.Array.Empty<string>());
                }
            }

            if (partsReorderableList == null)
            {
                BuildPartsList();
            }

            partsReorderableList.DoLayoutList();

            EditorGUILayout.Space(6);
            if (GUILayout.Button("保存到 ScriptableObject", GUILayout.Height(36)))
            {
                RoleDefaultVisiblePartsConfigImporter.ImportEntry(GetEffectiveRoleDefaultPartsKey(), roleDefaultPartNames);
            }

            EditorGUILayout.EndScrollView();
        }

        void PingSourcePrefab()
        {
            if (sourceCharacterPrefab == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(sourceCharacterPrefab);
            Selection.activeObject = sourceCharacterPrefab;
            EditorUtility.FocusProjectWindow();
        }

        void DrawRolePrefabKeyReadOnly()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                string key = GetEffectiveRoleDefaultPartsKey();
                EditorGUILayout.TextField(
                    "Key（Role 预制体名）",
                    string.IsNullOrEmpty(key) ? "（请先指定角色预制体）" : key);
            }
        }

        string GetEffectiveRoleDefaultPartsKey()
        {
            return sourceCharacterPrefab != null ? sourceCharacterPrefab.name.Trim() : string.Empty;
        }

        void SyncPartsListWhenKeyChanged()
        {
            string key = GetEffectiveRoleDefaultPartsKey();
            if (key == cachedKeyForPartsList)
            {
                return;
            }

            cachedKeyForPartsList = key;
            if (string.IsNullOrEmpty(key))
            {
                ReplacePartList(System.Array.Empty<string>());
                return;
            }

            string[] parts = RoleDefaultVisiblePartsConfigImporter.TryGetPartNames(key) ?? System.Array.Empty<string>();
            ReplacePartList(parts);
        }

        void LoadPartsFromConfig()
        {
            string key = GetEffectiveRoleDefaultPartsKey();
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            string[] parts = RoleDefaultVisiblePartsConfigImporter.TryGetPartNames(key);
            ReplacePartList(parts);
            Debug.Log($"[Synty] 从配置覆盖列表 Key={key}，共 {roleDefaultPartNames.Count} 项。");
        }

        void LoadPartsFromSourceModel()
        {
            if (sourceCharacterPrefab == null)
            {
                return;
            }

            string key = GetEffectiveRoleDefaultPartsKey();
            string[] parts = RoleDefaultVisiblePartsConfigBuilder.CollectVisiblePartNamesFromSourcePrefab(
                sourceCharacterPrefab,
                key);
            if (SyntyModularRolePartUtil.TryParseRolePrefabGender(key, out bool roleIsFemale, out _))
            {
                parts = SyntyModularRolePartUtil.NormalizePartNamesToRolePartPrefabs(parts, roleIsFemale);
            }

            ReplacePartList(parts);
            string genderHint = SyntyModularRolePartUtil.TryParseRolePrefabGender(key, out bool isFemale, out _)
                ? (isFemale ? "Female_Parts + All_Gender_Parts" : "Male_Parts + All_Gender_Parts")
                : "Female_Parts + Male_Parts（Key 未指定 _F/_M）";
            Debug.Log($"[Synty] 从模型覆盖列表 Key={key}，共 {roleDefaultPartNames.Count} 个部件（{genderHint}）。");
            if (roleDefaultPartNames.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    "Synty模块化处理",
                    "未读到任何 Chr。\n请确认源模型各槽位已勾选要用的部件（activeSelf），" +
                    "且预制体名为带 _F/_M 的 Role；必要时先「应用性别」并 Ctrl+S 保存后再读。",
                    "确定");
            }
        }

        /// <summary>仅改窗口列表：整表覆盖，不写 ScriptableObject。</summary>
        void ReplacePartList(IList<string> parts)
        {
            var next = new List<string>();
            if (parts != null)
            {
                foreach (string part in parts)
                {
                    if (string.IsNullOrWhiteSpace(part))
                    {
                        continue;
                    }

                    string trimmed = part.Trim();
                    if (!next.Contains(trimmed))
                    {
                        next.Add(trimmed);
                    }
                }
            }

            roleDefaultPartNames = next;
            partsReorderableList = null;
            BuildPartsList();
            GUI.FocusControl(null);
            Repaint();
        }
    }
}
