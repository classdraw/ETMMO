using ET;
using UnityEditor;
using UnityEngine;

namespace ET.Editor.Frame2D
{
    [CustomEditor(typeof(FrameSheetAnimConfig))]
    public class FrameSheetAnimConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty gridRowsProp;
        private SerializedProperty gridColumnsProp;
        private SerializedProperty clipsProp;

        private void OnEnable()
        {
            gridRowsProp = serializedObject.FindProperty("gridRows");
            gridColumnsProp = serializedObject.FindProperty("gridColumns");
            clipsProp = serializedObject.FindProperty("clips");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Grid Shared", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(gridRowsProp);
            EditorGUILayout.PropertyField(gridColumnsProp);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Clips", EditorStyles.boldLabel);

            if (clipsProp == null)
            {
                EditorGUILayout.HelpBox("未找到 clips 字段。", MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            for (int i = 0; i < clipsProp.arraySize; i++)
            {
                DrawClip(clipsProp.GetArrayElementAtIndex(i), i);
            }

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Add Clip"))
                {
                    clipsProp.InsertArrayElementAtIndex(clipsProp.arraySize);
                }

                using (new EditorGUI.DisabledScope(clipsProp.arraySize == 0))
                {
                    if (GUILayout.Button("Remove Last"))
                    {
                        clipsProp.DeleteArrayElementAtIndex(clipsProp.arraySize - 1);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawClip(SerializedProperty clipProp, int index)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginVertical("box");

            SerializedProperty animTypeProp = clipProp.FindPropertyRelative("animType");
            string title = animTypeProp != null ? animTypeProp.enumDisplayNames[animTypeProp.enumValueIndex] : $"Clip {index}";
            clipProp.isExpanded = EditorGUILayout.Foldout(clipProp.isExpanded, title, true);

            if (!clipProp.isExpanded)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            EditorGUI.indentLevel++;
            if (animTypeProp != null)
            {
                EditorGUILayout.PropertyField(animTypeProp);
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Default Part Texture Slots", EditorStyles.miniBoldLabel);
            DrawSlotPopup(clipProp, "bodyTextureSlot", "Body");
            DrawSlotPopup(clipProp, "headTextureSlot", "Head");
            DrawSlotPopup(clipProp, "tailTextureSlot", "Tail");
            DrawSlotPopup(clipProp, "shirtTextureSlot", "Shirt");
            DrawSlotPopup(clipProp, "pantsTextureSlot", "Pants");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Facing Texture Slot Overrides", EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox("仅勾选需要覆盖的部位；未勾选时沿用上方默认槽位。", MessageType.None);
            DrawFacingOverride(clipProp, "downTextureOverrides", "Down");
            DrawFacingOverride(clipProp, "leftTextureOverrides", "Left");
            DrawFacingOverride(clipProp, "rightTextureOverrides", "Right");
            DrawFacingOverride(clipProp, "upTextureOverrides", "Up");

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Rows By Facing", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("rowDown"), new GUIContent("Row Down"));
            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("rowLeft"), new GUIContent("Row Left"));
            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("rowRight"), new GUIContent("Row Right"));
            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("rowUp"), new GUIContent("Row Up"));

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Columns", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("startColumn"), new GUIContent("Start Column"));
            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("endColumn"), new GUIContent("End Column"));

            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("loop"));
            EditorGUILayout.PropertyField(clipProp.FindPropertyRelative("interval"));

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private static void DrawFacingOverride(SerializedProperty clipProp, string fieldName, string facingLabel)
        {
            SerializedProperty overrideProp = clipProp.FindPropertyRelative(fieldName);
            if (overrideProp == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.LabelField(facingLabel, EditorStyles.miniBoldLabel);
            DrawOptionalSlotOverride(overrideProp, "overrideBody", "bodyTextureSlot", "Body");
            DrawOptionalSlotOverride(overrideProp, "overrideHead", "headTextureSlot", "Head");
            DrawOptionalSlotOverride(overrideProp, "overrideTail", "tailTextureSlot", "Tail");
            DrawOptionalSlotOverride(overrideProp, "overrideShirt", "shirtTextureSlot", "Shirt");
            DrawOptionalSlotOverride(overrideProp, "overridePants", "pantsTextureSlot", "Pants");
            EditorGUILayout.EndVertical();
        }

        private static void DrawOptionalSlotOverride(
            SerializedProperty overrideProp,
            string toggleFieldName,
            string slotFieldName,
            string label)
        {
            SerializedProperty toggleProp = overrideProp.FindPropertyRelative(toggleFieldName);
            SerializedProperty slotProp = overrideProp.FindPropertyRelative(slotFieldName);
            if (toggleProp == null || slotProp == null)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                toggleProp.boolValue = EditorGUILayout.ToggleLeft(label, toggleProp.boolValue, GUILayout.Width(120));
                using (new EditorGUI.DisabledScope(!toggleProp.boolValue))
                {
                    slotProp.enumValueIndex = EditorGUILayout.Popup(slotProp.enumValueIndex, slotProp.enumDisplayNames);
                }
            }
        }

        private static void DrawSlotPopup(SerializedProperty clipProp, string fieldName, string label)
        {
            SerializedProperty slotProp = clipProp.FindPropertyRelative(fieldName);
            if (slotProp == null)
            {
                return;
            }

            slotProp.enumValueIndex = EditorGUILayout.Popup(label, slotProp.enumValueIndex, slotProp.enumDisplayNames);
        }
    }
}
