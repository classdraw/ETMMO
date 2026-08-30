using ET;
using UnityEditor;
using UnityEngine;

namespace ET.Editor.Frame2D
{
    [CustomEditor(typeof(FrameRoleTextureCatalog))]
    public class FrameRoleTextureCatalogEditor : UnityEditor.Editor
    {
        private int lookupDisplayId;
        private string lookupResult = string.Empty;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            FrameRoleTextureCatalog catalog = (FrameRoleTextureCatalog)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Excel Display 查图", EditorStyles.boldLabel);
            lookupDisplayId = EditorGUILayout.IntField("Display", lookupDisplayId);
            if (GUILayout.Button("查询贴图", GUILayout.Height(26)))
            {
                catalog.RebuildLookup();
                if (catalog.TryGetEntry(lookupDisplayId, out FrameRoleTextureEntry entry))
                {
                    FrameRoleTextureId.Decode(lookupDisplayId, out int partKey, out int raceKey, out int genderKey, out int index);
                    string texName = entry.texture != null ? entry.texture.name : "(空引用)";
                    lookupResult = $"{partKey}/{raceKey}/{genderKey}[{index}] -> {texName}";
                }
                else
                {
                    lookupResult = "未找到";
                }
            }

            if (!string.IsNullOrEmpty(lookupResult))
            {
                EditorGUILayout.HelpBox(lookupResult, MessageType.None);
            }
        }
    }
}
