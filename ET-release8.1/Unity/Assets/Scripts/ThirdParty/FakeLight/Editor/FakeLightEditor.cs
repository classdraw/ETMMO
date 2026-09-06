using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

namespace CenturyGameEditor.FakeLight
{
    using CenturyGame.FakeLight;
    
    [CustomEditor(typeof(FakeLight))]
    public class FakeLightEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            FakeLight light = (FakeLight)target;

            EditorGUI.BeginChangeCheck();
            Undo.RecordObject(light, "Modified FakeLight");

            light.lightType = (FakeLight.FakeLightType)EditorGUILayout.EnumPopup("Light Type", light.lightType);

            if (light.lightType == FakeLight.FakeLightType.Spot)
            {
                Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
                rect = EditorGUI.PrefixLabel(rect, new GUIContent("Inner/Outer Spot Angle"));
                
                float fieldWidth = 45f;
                
                Rect leftField = new Rect(rect.x, rect.y, fieldWidth, rect.height);
                
                Rect rightField = new Rect(rect.xMax - fieldWidth, rect.y, fieldWidth, rect.height);
                
                Rect sliderRect = new Rect(rect.x + fieldWidth + 5f, rect.y, rect.width - (fieldWidth * 2 + 10f), rect.height);
                
                float innerAngle = light.innerSpotAngle;
                float outerAngle = light.outerSpotAngle;
                
                float newInnerAngle = EditorGUI.DelayedFloatField(leftField, innerAngle);
                light.innerSpotAngle = Mathf.Clamp(newInnerAngle, 0f, light.outerSpotAngle);
                
                EditorGUI.BeginChangeCheck();
                EditorGUI.MinMaxSlider(sliderRect, ref innerAngle, ref outerAngle, 0f, 179f);
                if (EditorGUI.EndChangeCheck())
                {
                    light.innerSpotAngle = innerAngle;
                    light.outerSpotAngle = outerAngle;
                }
                
                float newOuterAngle = EditorGUI.DelayedFloatField(rightField, outerAngle);
                light.outerSpotAngle = Mathf.Clamp(newOuterAngle, light.innerSpotAngle, 179f);
            }
            
            light.color = EditorGUILayout.ColorField("Color", light.color);

            float newIntensity = EditorGUILayout.FloatField("Intensity", light.intensity);
            light.intensity = Mathf.Max(0, newIntensity);

            float newRange = EditorGUILayout.FloatField("Range", light.range);
            light.range = Mathf.Max(0, newRange);
            
            int newPriority = EditorGUILayout.IntField("Priority", light.priority);
            light.priority = Mathf.Max(0, newPriority);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(light);
            }
        }

        private void OnSceneGUI()
        {
            FakeLight light = (FakeLight)target;
         
            Color linearColor = light.color.linear;
            Vector3 pos = light.transform.position;
            float range = light.range;
            
            Handles.color = linearColor;

            if ((int)light.lightType == 0)
            {
                Handles.DrawWireDisc(pos, Vector3.up, range);
                Handles.DrawWireDisc(pos, Vector3.right, range);
                Handles.DrawWireDisc(pos, Vector3.forward, range);
            }
            else
            {
                Vector3 up = light.transform.up;
                Vector3 right = light.transform.right;
                Vector3 forward = light.transform.forward;
                
                float outerAngle = light.outerSpotAngle;
                float innerAngle = light.innerSpotAngle;
                
                var outerDiscRadius = range * Mathf.Sin(outerAngle * Mathf.Deg2Rad * 0.5f);
                var outerDiscDistance = Mathf.Cos(Mathf.Deg2Rad * outerAngle * 0.5f) * range;
                var vectorLineUp = Vector3.Normalize(forward * outerDiscDistance + up * outerDiscRadius);
                var vectorLineLeft = Vector3.Normalize(forward * outerDiscDistance - right * outerDiscRadius);
                
                var rangeCenter = pos + forward * range;
                Handles.DrawLine(pos, rangeCenter);
                
                if (innerAngle > 0f)
                {
                    var innerDiscRadius = range * Mathf.Sin(innerAngle * Mathf.Deg2Rad * 0.5f);
                    var innerDiscDistance = Mathf.Cos(Mathf.Deg2Rad * innerAngle * 0.5f) * range;
                    
                    var rangeCenter1 = pos + forward * innerDiscDistance;
                    
                    var rangeUp = rangeCenter1 + up * innerDiscRadius;
                    Handles.DrawLine(pos, rangeUp);
                    var rangeDown = rangeCenter1 - up * innerDiscRadius;
                    Handles.DrawLine(pos, rangeDown);
                    
                    Handles.DrawWireDisc(rangeCenter1, forward, innerDiscRadius);
                }
                
                var rangeCenter2 = pos + forward * outerDiscDistance;
                var rangeRight = rangeCenter2 + right * outerDiscRadius;
                Handles.DrawLine(pos, rangeRight);
                var rangeLeft = rangeCenter2 - right * outerDiscRadius;
                Handles.DrawLine(pos, rangeLeft);
                
                Handles.DrawWireArc(pos, right, vectorLineUp, outerAngle, range);
                Handles.DrawWireArc(pos, up, vectorLineLeft, outerAngle, range);
                
                Handles.DrawWireDisc(rangeCenter2, forward, outerDiscRadius);
            }
        }
    }
    
    
}

