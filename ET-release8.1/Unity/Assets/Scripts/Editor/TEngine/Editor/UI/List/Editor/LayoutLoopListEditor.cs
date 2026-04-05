using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UI;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Pool;


namespace TEngine {
    
    [CustomEditor(typeof(LayoutLoopList), true)]
    [CanEditMultipleObjects]
    public class LayoutLoopListEditor : ScrollRectEditor {

        List<SerializedProperty> properties = new List<SerializedProperty>();

        private const float BindObjectItem_Btn_Width = 80;
        private readonly GUIContent Type_Label = new GUIContent("选择类型");
        private Rect m_TempRect = Rect.zero;
        private ReorderableList m_ReorderableList;
        private SerializedProperty m_Templates;
        private GenericMenu typeMenu = new GenericMenu();

        protected override void OnEnable() {
            base.OnEnable();

            var fields = typeof(LayoutLoopList).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields) {
                var property = serializedObject.FindProperty(field.Name);
                if (property == null || property.propertyPath == "m_Templates") continue;
                properties.Add(property);
            }
            m_Templates = serializedObject.FindProperty("m_Templates");
            m_ReorderableList = new ReorderableList(serializedObject, m_Templates, true, true, true, true);
            m_ReorderableList.elementHeight = 20;

            m_ReorderableList.drawElementCallback = DrawElementBindObjectItem;
            m_ReorderableList.drawHeaderCallback = rect => {
                EditorGUI.LabelField(rect, "模版列表：");
            };
        }

        private void DrawElementBindObjectItem(Rect rect, int index, bool isActive, bool isFocused) {
            SerializedProperty element = m_Templates.GetArrayElementAtIndex(index);

            float object_width = rect.width - (BindObjectItem_Btn_Width + 20);
            m_TempRect.Set(rect.x, rect.y, object_width, 16);
            EditorGUI.BeginChangeCheck();
            element.objectReferenceValue = EditorGUI.ObjectField(m_TempRect, element.objectReferenceValue, typeof(Component), true);
            m_TempRect.Set(rect.x + object_width + 10, rect.y, BindObjectItem_Btn_Width, 16);
            if (GUI.Button(m_TempRect, Type_Label) || EditorGUI.EndChangeCheck()) {
                if (element.objectReferenceValue) {
                    OpenTypeSelect(element);
                }
            }
            GUI.color = Color.white;
        }

        /// <summary>
        /// 类型选中
        /// </summary>
        private void OpenTypeSelect(SerializedProperty element) {
            List<Component> components = ListPool<Component>.Get();
            (element.objectReferenceValue as Component).gameObject.GetComponents<Component>(components);
            typeMenu = new GenericMenu();
            for (int i = 0; i < components.Count; i++) {
                Component component = components[i];
                typeMenu.AddItem(new GUIContent($"{i + 1} {component.GetType().Name}"), false, (comp) => {
                    element.objectReferenceValue = comp as Component;
                    serializedObject.ApplyModifiedProperties();
                }, component);
            }
            typeMenu.ShowAsContext();
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("------------------------");

            m_ReorderableList.DoLayoutList();
            foreach (var property in properties) {
                EditorGUILayout.PropertyField(property);
            }
            serializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
        }
    }
}