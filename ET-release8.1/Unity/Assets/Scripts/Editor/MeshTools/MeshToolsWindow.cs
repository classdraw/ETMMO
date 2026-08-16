using System.IO;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// Mesh 创建与编辑工具。
    /// </summary>
    public class MeshToolsWindow : EditorWindow
    {
        private const string DefaultOutputPath = "Assets/Art/Project/Meshs";

        private static readonly string[] TabNames = { "创建Quad", "SpriteRenderer转换" };

        private int selectedTab;
        private string outputPath = DefaultOutputPath;
        private string meshName = "Quad";
        private float quadWidth = 1f;
        private float quadHeight = 1f;
        private DefaultAsset spriteRendererOutputFolder;
        private UnityEngine.GameObject spriteRendererSourcePrefab;
        private int spriteRendererPreviewCount = -1;
        private string spriteRendererPreviewPrefabPath = string.Empty;

        [MenuItem("Tools/Mesh", false, 51)]
        public static void Open()
        {
            var window = GetWindow<MeshToolsWindow>(true, "Mesh", true);
            window.minSize = new Vector2(420, 320);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(8);
            selectedTab = GUILayout.Toolbar(selectedTab, TabNames);
            EditorGUILayout.Space(8);

            switch (selectedTab)
            {
                case 0:
                    DrawCreateQuadTab();
                    break;
                case 1:
                    DrawSpriteRendererConvertTab();
                    break;
            }
        }

        private void DrawCreateQuadTab()
        {
            EditorGUILayout.HelpBox("在 XZ 平面上生成 Quad 面片 Mesh，并保存到指定目录。", MessageType.Info);
            EditorGUILayout.Space(6);

            DrawOutputPathField();
            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mesh 名称", GUILayout.Width(80));
            meshName = EditorGUILayout.TextField(meshName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("宽度 (X)", GUILayout.Width(80));
            quadWidth = EditorGUILayout.FloatField(quadWidth);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("高度 (Z)", GUILayout.Width(80));
            quadHeight = EditorGUILayout.FloatField(quadHeight);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(12);

            bool canGenerate = CanGenerateQuad(out string validationMessage);
            using (new EditorGUI.DisabledScope(!canGenerate))
            {
                if (GUILayout.Button("生成", GUILayout.Height(34)))
                {
                    GenerateQuadMesh();
                }
            }

            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }
        }

        private void DrawSpriteRendererConvertTab()
        {
            EditorGUILayout.HelpBox(
                "拖入输出文件夹和 Sprite Prefab。会在文件夹下创建以预设名命名的子目录，输出「原名_mesh.prefab」、各节点 Mesh（.asset）及材质（Custom/SR_WorldSpriteTransparent）。节点缩放保持原值，Sprite 尺寸烘焙进 Mesh。",
                MessageType.Info);
            EditorGUILayout.Space(6);

            spriteRendererOutputFolder = (DefaultAsset)EditorGUILayout.ObjectField(
                "输出文件夹",
                spriteRendererOutputFolder,
                typeof(DefaultAsset),
                false);

            EditorGUI.BeginChangeCheck();
            spriteRendererSourcePrefab = (UnityEngine.GameObject)EditorGUILayout.ObjectField(
                "Sprite Prefab",
                spriteRendererSourcePrefab,
                typeof(UnityEngine.GameObject),
                false);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshSpriteRendererPreviewCount();
            }

            if (spriteRendererOutputFolder != null || spriteRendererSourcePrefab != null)
            {
                if (spriteRendererOutputFolder != null)
                {
                    EditorGUILayout.LabelField("输出路径", AssetDatabase.GetAssetPath(spriteRendererOutputFolder));
                }

                if (spriteRendererSourcePrefab != null)
                {
                    string prefabPath = AssetDatabase.GetAssetPath(spriteRendererSourcePrefab);
                    EditorGUILayout.LabelField("Prefab 路径", prefabPath);
                    EditorGUILayout.LabelField("SpriteRenderer 数量", spriteRendererPreviewCount >= 0 ? spriteRendererPreviewCount.ToString() : "计算中...");
                    EditorGUILayout.LabelField("输出子目录", GetSpriteRendererOutputDirPreview());
                    EditorGUILayout.LabelField("输出 Prefab", GetSpriteRendererOutputPrefabPreview());
                }

                EditorGUILayout.LabelField("Mesh", "按 Sprite 尺寸生成 Quad（XZ）");
                EditorGUILayout.LabelField("Shader", "Custom/SR_WorldSpriteTransparent");
            }

            EditorGUILayout.Space(12);

            bool canConvert = CanConvertSpriteRenderer(out string validationMessage);
            using (new EditorGUI.DisabledScope(!canConvert))
            {
                if (GUILayout.Button("转换", GUILayout.Height(34)))
                {
                    ConvertSpriteRendererPrefab();
                }
            }

            if (!string.IsNullOrEmpty(validationMessage))
            {
                EditorGUILayout.HelpBox(validationMessage, MessageType.Warning);
            }
        }

        private string GetSpriteRendererOutputDirPreview()
        {
            if (spriteRendererOutputFolder == null || spriteRendererSourcePrefab == null)
            {
                return string.Empty;
            }

            string folderPath = AssetDatabase.GetAssetPath(spriteRendererOutputFolder);
            string prefabName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(spriteRendererSourcePrefab));
            return $"{folderPath}/{prefabName}";
        }

        private string GetSpriteRendererOutputPrefabPreview()
        {
            string outputDir = GetSpriteRendererOutputDirPreview();
            if (string.IsNullOrEmpty(outputDir))
            {
                return string.Empty;
            }

            string prefabName = Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(spriteRendererSourcePrefab));
            return $"{outputDir}/{prefabName}_mesh.prefab";
        }

        private void RefreshSpriteRendererPreviewCount()
        {
            if (spriteRendererSourcePrefab == null)
            {
                spriteRendererPreviewCount = -1;
                spriteRendererPreviewPrefabPath = string.Empty;
                return;
            }

            string prefabPath = AssetDatabase.GetAssetPath(spriteRendererSourcePrefab);
            if (string.IsNullOrEmpty(prefabPath))
            {
                spriteRendererPreviewCount = 0;
                spriteRendererPreviewPrefabPath = string.Empty;
                return;
            }

            if (prefabPath == spriteRendererPreviewPrefabPath)
            {
                return;
            }

            spriteRendererPreviewPrefabPath = prefabPath;
            spriteRendererPreviewCount = SpriteRendererMeshConverter.CountSpriteRenderersInPrefab(spriteRendererSourcePrefab);
        }

        private bool CanConvertSpriteRenderer(out string validationMessage)
        {
            validationMessage = string.Empty;

            if (spriteRendererOutputFolder == null)
            {
                validationMessage = "请先拖入输出文件夹。";
                return false;
            }

            string folderPath = AssetDatabase.GetAssetPath(spriteRendererOutputFolder);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            {
                validationMessage = "请拖入 Project 中的输出文件夹。";
                return false;
            }

            if (spriteRendererSourcePrefab == null)
            {
                validationMessage = "请先拖入 Prefab。";
                return false;
            }

            string prefabPath = AssetDatabase.GetAssetPath(spriteRendererSourcePrefab);
            if (string.IsNullOrEmpty(prefabPath) || !prefabPath.EndsWith(".prefab"))
            {
                validationMessage = "请拖入 Project 中的 Prefab 资源。";
                return false;
            }

            if (PrefabUtility.GetPrefabAssetType(spriteRendererSourcePrefab) == PrefabAssetType.NotAPrefab)
            {
                validationMessage = "所选对象不是 Prefab 资源。";
                return false;
            }

            RefreshSpriteRendererPreviewCount();
            if (spriteRendererPreviewCount <= 0)
            {
                validationMessage = "Prefab 中未找到带 Sprite 的 SpriteRenderer 组件。";
                return false;
            }

            return true;
        }

        private void ConvertSpriteRendererPrefab()
        {
            SpriteRendererMeshConverter.ConvertResult result = SpriteRendererMeshConverter.ConvertPrefab(
                spriteRendererOutputFolder,
                spriteRendererSourcePrefab);
            if (!result.Success)
            {
                EditorUtility.DisplayDialog("SpriteRenderer 转换", result.Message, "确定");
                return;
            }

            UnityEngine.Object outputPrefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(result.OutputPrefabPath);
            if (outputPrefab != null)
            {
                EditorGUIUtility.PingObject(outputPrefab);
                Selection.activeObject = outputPrefab;
            }

            Debug.Log(result.Message);
            EditorUtility.DisplayDialog("SpriteRenderer 转换", result.Message, "确定");
        }

        private void DrawOutputPathField()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("输出路径", GUILayout.Width(80));
            outputPath = EditorGUILayout.TextField(NormalizeAssetPath(outputPath));

            if (GUILayout.Button("定位", GUILayout.Width(60)))
            {
                PingOutputFolder();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void PingOutputFolder()
        {
            string path = NormalizeAssetPath(outputPath);
            if (!AssetDatabase.IsValidFolder(path))
            {
                EditorUtility.DisplayDialog("定位失败", $"目录不存在：\n{path}", "确定");
                return;
            }

            UnityEngine.Object folder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (folder == null)
            {
                return;
            }

            EditorGUIUtility.PingObject(folder);
            Selection.activeObject = folder;
        }

        private bool CanGenerateQuad(out string validationMessage)
        {
            validationMessage = string.Empty;

            string path = NormalizeAssetPath(outputPath);
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith("Assets/"))
            {
                validationMessage = "输出路径必须位于 Assets 目录下。";
                return false;
            }

            if (string.IsNullOrWhiteSpace(meshName))
            {
                validationMessage = "Mesh 名称不能为空。";
                return false;
            }

            if (meshName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                validationMessage = "Mesh 名称包含非法字符。";
                return false;
            }

            if (quadWidth <= 0f || quadHeight <= 0f)
            {
                validationMessage = "宽度和高度必须大于 0。";
                return false;
            }

            return true;
        }

        private void GenerateQuadMesh()
        {
            string folderPath = NormalizeAssetPath(outputPath);
            EnsureFolderExists(folderPath);

            string trimmedName = meshName.Trim();
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{trimmedName}.asset");

            Mesh mesh = CreateQuadMesh(quadWidth, quadHeight);
            mesh.name = Path.GetFileNameWithoutExtension(assetPath);

            AssetDatabase.CreateAsset(mesh, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UnityEngine.Object createdAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (createdAsset != null)
            {
                EditorGUIUtility.PingObject(createdAsset);
                Selection.activeObject = createdAsset;
            }

            Debug.Log($"Quad Mesh 已生成：{assetPath}（宽 {quadWidth}，高 {quadHeight}，XZ 平面）");
        }

        private static Mesh CreateQuadMesh(float width, float height)
        {
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            var vertices = new[]
            {
                new Vector3(-halfWidth, 0f, -halfHeight),
                new Vector3(halfWidth, 0f, -halfHeight),
                new Vector3(halfWidth, 0f, halfHeight),
                new Vector3(-halfWidth, 0f, halfHeight),
            };

            var triangles = new[]
            {
                0, 1, 2,
                0, 2, 3,
            };

            var uvs = new[]
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs,
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }

        private static string NormalizeAssetPath(string path)
        {
            return path?.Replace('\\', '/').Trim() ?? string.Empty;
        }

        private static void EnsureFolderExists(string assetFolderPath)
        {
            assetFolderPath = NormalizeAssetPath(assetFolderPath);
            if (AssetDatabase.IsValidFolder(assetFolderPath))
            {
                return;
            }

            string parentPath = Path.GetDirectoryName(assetFolderPath)?.Replace('\\', '/');
            string folderName = Path.GetFileName(assetFolderPath);
            if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(folderName))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parentPath))
            {
                EnsureFolderExists(parentPath);
            }

            AssetDatabase.CreateFolder(parentPath, folderName);
        }
    }
}
