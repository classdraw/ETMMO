using UnityEditor;
using UnityEngine;

namespace ET
{
    [CustomPropertyDrawer(typeof(UnitPartSlotKeyAttribute))]
    public class UnitPartSlotKeyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.Integer)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            const float chineseWidth = 160f;
            const float keyWidth = 48f;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            Rect labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, lineHeight);
            Rect keyRect = new Rect(labelRect.xMax, position.y, keyWidth, lineHeight);
            Rect chineseRect = new Rect(keyRect.xMax + 4f, position.y, chineseWidth, lineHeight);

            EditorGUI.LabelField(labelRect, label);
            property.intValue = EditorGUI.IntField(keyRect, property.intValue);
            string chinese = UnitPartSlotNames.GetChineseName(property.intValue);
            EditorGUI.LabelField(chineseRect, chinese, EditorStyles.boldLabel);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
