using System.Collections.Generic;
using ET;
using UnityEditor;
using UnityEngine;

namespace ET.Editor.Frame2D
{
    [CustomEditor(typeof(FrameRoleTextureConfig))]
    public class FrameRoleTextureConfigEditor : UnityEditor.Editor
    {
        private int lookupDisplayId;
        private string lookupResult = string.Empty;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            FrameRoleTextureConfig config = (FrameRoleTextureConfig)target;

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Display", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("display = 部位Key*10000000 + 种族Key*100000 + 性别Key*1000 + 列表序号", MessageType.Info);

            if (GUILayout.Button("重算全部 Display", GUILayout.Height(26)))
            {
                Undo.RecordObject(config, "Rebuild FrameRole Display");
                config.RebuildDisplayIds();
                EditorUtility.SetDirty(config);
            }

            EditorGUILayout.Space(6);
            lookupDisplayId = EditorGUILayout.IntField("按 Display 查图", lookupDisplayId);
            if (GUILayout.Button("查询"))
            {
                config.RebuildLookup();
                if (config.TryGetEntry(lookupDisplayId, out FrameRoleTextureEntry entry))
                {
                    string texName = entry.texture != null ? entry.texture.name : "(空引用)";
                    lookupResult = $"命中 {entry.name} / {texName}";
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

    public static class FrameRoleTextureAssetMenu
    {
        private const string ScriptObjectFolder = "Assets/Bundles/ScriptObject/FrameRole";

        [MenuItem("Tools/Frame2D/Create Default Role Texture Assets")]
        public static void CreateDefaultAssets()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Bundles/ScriptObject"))
            {
                AssetDatabase.CreateFolder("Assets/Bundles", "ScriptObject");
            }

            if (!AssetDatabase.IsValidFolder(ScriptObjectFolder))
            {
                AssetDatabase.CreateFolder("Assets/Bundles/ScriptObject", "FrameRole");
            }

            var created = new List<FrameRoleTextureConfig>();
            created.Add(CreatePartAsset((int)FrameRolePartType.Body, "FrameRoleTexture_Body"));
            created.Add(CreatePartAsset((int)FrameRolePartType.Head, "FrameRoleTexture_Head"));
            created.Add(CreatePartAsset((int)FrameRolePartType.Tail, "FrameRoleTexture_Tail"));
            created.Add(CreatePartAsset((int)FrameRolePartType.Shirt, "FrameRoleTexture_Shirt"));
            created.Add(CreatePartAsset((int)FrameRolePartType.Pants, "FrameRoleTexture_Pants"));

            string catalogPath = $"{ScriptObjectFolder}/FrameRoleTextureCatalog.asset";
            FrameRoleTextureCatalog catalog = AssetDatabase.LoadAssetAtPath<FrameRoleTextureCatalog>(catalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<FrameRoleTextureCatalog>();
                AssetDatabase.CreateAsset(catalog, catalogPath);
            }

            catalog.partConfigs = created;
            catalog.RebuildLookup();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorGUIUtility.PingObject(catalog);
        }

        private static FrameRoleTextureConfig CreatePartAsset(int partKey, string fileName)
        {
            string path = $"{ScriptObjectFolder}/{fileName}.asset";
            FrameRoleTextureConfig config = AssetDatabase.LoadAssetAtPath<FrameRoleTextureConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<FrameRoleTextureConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.partKey = partKey;
            EditorUtility.SetDirty(config);
            return config;
        }
    }
}
