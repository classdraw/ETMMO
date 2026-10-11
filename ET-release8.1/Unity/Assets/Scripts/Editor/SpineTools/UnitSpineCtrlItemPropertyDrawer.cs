using UnityEditor;
using UnityEngine;

namespace ET
{
    [CustomPropertyDrawer(typeof(UnitSpineCtrlItem))]
    public class UnitSpineCtrlItemPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property, BuildHeader(property, label), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, BuildHeader(property, label), true);
        }

        private static GUIContent BuildHeader(SerializedProperty property, GUIContent label)
        {
            SerializedProperty slotPartProp = property.FindPropertyRelative("slotPart");
            int slotPart = slotPartProp != null ? slotPartProp.intValue : -1;
            string chinese = slotPart >= 0 ? UnitPartSlotNames.GetChineseName(slotPart) : "未知部位";

            SerializedProperty renderProp = property.FindPropertyRelative("render");
            string renderHint = string.Empty;
            if (renderProp?.objectReferenceValue is Component renderComponent)
            {
                renderHint = $" · {renderComponent.gameObject.name}";
            }

            string header = slotPart >= 0
                ? $"[{slotPart}] {chinese}{renderHint}"
                : label.text;

            return new GUIContent(header);
        }
    }
}
