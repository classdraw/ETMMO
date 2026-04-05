#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TEngine.Editor.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [CustomEditor(typeof(UIBindComponent))]
    public class UIComponentInspectorEditor : Editor
    {
        private UIBindComponent m_uiBindComponent;
        private ReorderableList m_reorderableList;
        private SerializedProperty m_componentsProperty;
        private SerializedProperty m_className;

        private void OnEnable()
        {
            m_uiBindComponent = (UIBindComponent)target;
            m_componentsProperty = serializedObject.FindProperty("m_components");
            m_className = serializedObject.FindProperty("className");

            serializedObject.Update();
            if (string.IsNullOrEmpty(m_className.stringValue))
            {
                m_className.stringValue = $"{target.name}Component";
            }

            serializedObject.ApplyModifiedProperties();
            CreateReorderableList();
        }

        private void CreateReorderableList()
        {
            m_reorderableList = new ReorderableList(serializedObject, m_componentsProperty, true, true, true, true);
            m_reorderableList.drawHeaderCallback = rect =>
            {
                float width = rect.width - 20;
                float indexWidth = 90f;
                float nameWidth = 150f;
                float componentWidth = width - indexWidth - nameWidth - 15f;

                EditorGUI.LabelField(new Rect(rect.x, rect.y, indexWidth, rect.height), "序号");
                EditorGUI.LabelField(new Rect(rect.x + indexWidth, rect.y, nameWidth, rect.height), "对象名称");
                EditorGUI.LabelField(new Rect(rect.x + indexWidth + nameWidth, rect.y, componentWidth, rect.height),
                    "组件引用");
            };

            m_reorderableList.drawElementCallback = (rect, index, _, _) =>
            {
                SerializedProperty element = m_componentsProperty.GetArrayElementAtIndex(index);
                Component component = element.objectReferenceValue as Component;

                float height = EditorGUIUtility.singleLineHeight;
                float padding = 2f;
                float indexWidth = 70f;
                float nameWidth = 150f;
                float componentWidth = rect.width - indexWidth - nameWidth - 10f;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.LabelField(new Rect(rect.x, rect.y + padding, indexWidth, height), $"【{index}】");
                string objectName = component != null ? component.gameObject.name : "Null Reference";
                EditorGUI.TextField(new Rect(rect.x + indexWidth, rect.y + padding, nameWidth, height), objectName);
                EditorGUI.EndDisabledGroup();

                EditorGUI.PropertyField(
                    new Rect(rect.x + indexWidth + nameWidth + 8, rect.y + padding, componentWidth, height),
                    element, GUIContent.none);
            };

            m_reorderableList.elementHeight = EditorGUIUtility.singleLineHeight + 4f;
            m_reorderableList.onAddCallback = _ =>
            {
                m_componentsProperty.arraySize++;
                serializedObject.ApplyModifiedProperties();
            };
            m_reorderableList.onRemoveCallback = list =>
            {
                if (list.index >= 0 && list.index < m_componentsProperty.arraySize)
                {
                    m_componentsProperty.DeleteArrayElementAtIndex(list.index);
                    serializedObject.ApplyModifiedProperties();
                }
            };
            m_reorderableList.drawNoneElementCallback = rect =>
            {
                EditorGUI.LabelField(rect, "列表为空 - 点击「重新绑定组件」进行重绑");
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("重新绑定组件", GUILayout.Height(25)))
            {
                RebindComponents();
            }

            if (GUILayout.Button("生产脚本", GUILayout.Height(25)))
            {
                GenerateBindTextFile();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.LabelField("生成类名", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_className, GUIContent.none);
            if (GUILayout.Button("物体名+Component", GUILayout.Width(120), GUILayout.Height(18)))
            {
                m_className.stringValue = $"{target.name}Component";
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("生成的 txt 输出到 Assets/Temp 目录。", MessageType.Info);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            m_reorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void RebindComponents()
        {
            if (m_uiBindComponent == null)
            {
                return;
            }

            // 生成逻辑以 Selection.activeTransform 为根，与 Inspector 目标对齐，避免清空后绑到错误物体
            Selection.activeTransform = m_uiBindComponent.transform;
            Undo.RecordObject(m_uiBindComponent, "重新绑定 UI 组件");
            ScriptGenerator.GenerateUIComponentScript();
            EditorUtility.SetDirty(m_uiBindComponent);
            serializedObject.Update();
            Repaint();
        }

        private void GenerateBindTextFile()
        {
            serializedObject.ApplyModifiedProperties();
            string className = m_className.stringValue;
            if (string.IsNullOrWhiteSpace(className))
            {
                className = $"{m_uiBindComponent.gameObject.name}Component";
                m_className.stringValue = className;
            }

            string tempDir = Path.Combine(Application.dataPath, "Temp").Replace("\\", "/");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string filePath = Path.Combine(tempDir, $"{className}_Bind.txt").Replace("\\", "/");
            string content = BuildBindText(className);
            File.WriteAllText(filePath, content, Encoding.UTF8);
            AssetDatabase.Refresh();
            Debug.Log($"已生成绑定预览文本: Assets/Temp/{className}_Bind.txt");
        }

        private string BuildBindText(string className)
        {
            HashSet<string> extraUsings = new HashSet<string>();
            StringBuilder fields = new StringBuilder();
            StringBuilder binds = new StringBuilder();
            List<string> buttonHandlerMethods = new List<string>();
            HashSet<string> buttonHandlerSeen = new HashSet<string>();

            binds.AppendLine("\t\t\tUIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();");

            for (int i = 0; i < m_componentsProperty.arraySize; i++)
            {
                SerializedProperty element = m_componentsProperty.GetArrayElementAtIndex(i);
                Component comp = element.objectReferenceValue as Component;
                if (comp == null)
                {
                    continue;
                }

                AppendFieldAndBind(comp, i, fields, binds, extraUsings, buttonHandlerMethods, buttonHandlerSeen);
            }

            StringBuilder usings = new StringBuilder();
            usings.AppendLine("using ET;");
            usings.AppendLine("using GameLogic;");
            usings.AppendLine("using UnityEngine;");
            if (extraUsings.Contains("UnityEngine.UI"))
            {
                usings.AppendLine("using UnityEngine.UI;");
            }

            if (extraUsings.Contains("TEngine"))
            {
                usings.AppendLine("using TEngine;");
            }

#if ENABLE_TEXTMESHPRO
            if (extraUsings.Contains("TMPro"))
            {
                usings.AppendLine("using TMPro;");
            }
#endif

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//----------------------------------------------------------");
            sb.AppendLine("// UIBindComponent 生成 — 文本预览，可复制到工程中使用");
            sb.AppendLine("//----------------------------------------------------------");
            sb.AppendLine();
            sb.Append(usings);
            sb.AppendLine();
            sb.AppendLine("namespace ET.Client");
            sb.AppendLine("{");
            sb.AppendLine("\t[ComponentOf(typeof(UI))]");
            sb.AppendLine($"\tpublic class {className} : Entity, IAwake, IDestroy");
            sb.AppendLine("\t{");
            sb.Append(fields);
            sb.AppendLine("\t}");
            sb.AppendLine("}");
            sb.AppendLine();
            sb.AppendLine("namespace ET.Client");
            sb.AppendLine("{");
            sb.AppendLine($"\t[EntitySystemOf(typeof({className}))]");
            sb.AppendLine($"\t[FriendOf(typeof({className}))]");
            sb.AppendLine($"\tpublic static partial class {className}System");
            sb.AppendLine("\t{");
            sb.AppendLine("\t\t[EntitySystem]");
            sb.AppendLine($"\t\tprivate static void Awake(this {className} self)");
            sb.AppendLine("\t\t{");
            sb.Append(binds);
            sb.AppendLine("\t\t}");
            sb.AppendLine();
            sb.AppendLine("\t\t[EntitySystem]");
            sb.AppendLine($"\t\tprivate static void Destroy(this {className} self)");
            sb.AppendLine("\t\t{");
            sb.AppendLine("\t\t}");

            foreach (string handlerName in buttonHandlerMethods)
            {
                sb.AppendLine();
                sb.AppendLine($"\t\tpublic static void {handlerName}(this {className} self)");
                sb.AppendLine("\t\t{");
                sb.AppendLine("\t\t}");
            }

            sb.AppendLine("\t}");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AppendFieldAndBind(Component comp, int index, StringBuilder fields, StringBuilder binds,
            HashSet<string> extraUsings, List<string> buttonHandlerMethods, HashSet<string> buttonHandlerSeen)
        {
            string field = FormatFieldName(comp.gameObject.name);
            System.Type t = comp.GetType();

            if (t == typeof(RectTransform))
            {
                fields.AppendLine($"\t\tpublic GameObject {field};");
                binds.AppendLine(
                    $"\t\t\tself.{field} = m_bindComponent.GetComponent<RectTransform>({index}).gameObject;");
                return;
            }

            string typeName = GetDeclarationTypeName(t, extraUsings);
            fields.AppendLine($"\t\tpublic {typeName} {field};");
            binds.AppendLine($"\t\t\tself.{field} = m_bindComponent.GetComponent<{typeName}>({index});");

            if (typeof(Button).IsAssignableFrom(t))
            {
                extraUsings.Add("UnityEngine.UI");
                string handlerName = GetButtonClickMethodName(field);
                binds.AppendLine(
                    $"\t\t\tself.{field}.onClick.AddListener(() => {{ self.{handlerName}(); }});");
                if (buttonHandlerSeen.Add(handlerName))
                {
                    buttonHandlerMethods.Add(handlerName);
                }
            }
        }

        /// <summary>
        /// m_btnLogin → OnLogin；m_btnCancelMatch → OnCancelMatch
        /// </summary>
        private static string GetButtonClickMethodName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return "OnClick";
            }

            string s = fieldName;
            if (s.StartsWith("m_", StringComparison.Ordinal))
            {
                s = s.Substring(2);
            }

            if (s.Length >= 3 && s.StartsWith("btn", StringComparison.OrdinalIgnoreCase))
            {
                s = s.Substring(3);
            }
            else if (s.Length >= 3 && s.EndsWith("Btn", StringComparison.Ordinal))
            {
                s = s.Substring(0, s.Length - 3);
            }

            if (string.IsNullOrEmpty(s))
            {
                return "OnClick";
            }

            s = char.ToUpperInvariant(s[0]) + (s.Length > 1 ? s.Substring(1) : string.Empty);
            return "On" + s;
        }

        private static string GetDeclarationTypeName(System.Type t, HashSet<string> extraUsings)
        {
            string ns = t.Namespace;
            if (ns == "UnityEngine.UI")
            {
                extraUsings.Add("UnityEngine.UI");
                return t.Name;
            }

            if (ns == "TEngine")
            {
                extraUsings.Add("TEngine");
                return t.Name;
            }

            if (ns == "TMPro")
            {
#if ENABLE_TEXTMESHPRO
                extraUsings.Add("TMPro");
                return t.Name;
#else
                return t.FullName?.Replace('.', '+') ?? t.Name;
#endif
            }

            if (ns == "UnityEngine" || string.IsNullOrEmpty(ns))
            {
                return t.Name;
            }

            return t.Name;
        }

        private static string FormatFieldName(string goName)
        {
            if (string.IsNullOrEmpty(goName))
            {
                return "m_item";
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in goName)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    sb.Append(c);
                }
            }

            string s = sb.Length > 0 ? sb.ToString() : "item";
            if (char.IsDigit(s[0]))
            {
                s = "_" + s;
            }

            if (!s.StartsWith("m_"))
            {
                s = "m_" + char.ToLowerInvariant(s[0]) + s.Substring(1);
            }

            return s;
        }
    }
}

#endif
